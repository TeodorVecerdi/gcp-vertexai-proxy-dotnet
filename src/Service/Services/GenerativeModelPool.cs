using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mscc.GenerativeAI;
using Vecerdi.VertexAIProxy.Service.Configuration;

namespace Vecerdi.VertexAIProxy.Service.Services;

public interface IGenerativeModelPool {
    Task<GenerativeModel> GetModelAsync(string project, string region, string model);
    void ReturnModel(GenerativeModel model);
}

public sealed class GenerativeModelPool : IGenerativeModelPool, IDisposable {
    private readonly ConcurrentDictionary<string, ConcurrentQueue<PooledModel>> m_ModelPools = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> m_PoolSemaphores = new();
    private readonly Timer m_CleanupTimer;
    private readonly ILogger<GenerativeModelPool> m_Logger;
    private readonly GenerativeModelPoolOptions m_Options;
    private readonly IAccessTokenProvider m_AccessTokenProvider;
    private readonly IVertexAIFactory m_VertexAIFactory;

    public GenerativeModelPool(IAccessTokenProvider accessTokenProvider, IVertexAIFactory vertexAIFactory, IOptions<GenerativeModelPoolOptions> options, ILogger<GenerativeModelPool> logger) {
        m_AccessTokenProvider = accessTokenProvider;
        m_VertexAIFactory = vertexAIFactory;
        m_Options = options.Value;
        m_Logger = logger;

        // Clean up idle models every 5 minutes
        m_CleanupTimer = new Timer(CleanupIdleModels, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    public async Task<GenerativeModel> GetModelAsync(string project, string region, string model) {
        var poolKey = GetPoolKey(project, region, model);
        var pool = m_ModelPools.GetOrAdd(poolKey, static _ => new ConcurrentQueue<PooledModel>());
        var semaphore = m_PoolSemaphores.GetOrAdd(poolKey, static (_, options) => new SemaphoreSlim(options.MaxPoolSize, options.MaxPoolSize), m_Options);

        await semaphore.WaitAsync();

        try {
            // Try to get from pool first
            while (pool.TryDequeue(out var pooledModel)) {
                if (pooledModel.IsExpired(m_Options.ModelIdleTimeout))
                    continue;
                pooledModel.UpdateLastUsed();
                m_Logger.LogDebug("Retrieved model {Model} from pool for {Project}/{Region}", model, project, region);
                pooledModel.Model.AccessToken = await m_AccessTokenProvider.GetAccessTokenAsync();
                return pooledModel.Model;
            }

            // No available model in pool, create new one
            m_Logger.LogDebug("Creating new model {Model} for {Project}/{Region}", model, project, region);
            var vertexAI = await m_VertexAIFactory.CreateAsync(project, region);
            var generativeModel = vertexAI.GenerativeModel(model);
            generativeModel.AccessToken = await m_AccessTokenProvider.GetAccessTokenAsync();
            return generativeModel;
        } catch {
            semaphore.Release();
            throw;
        }
    }

    public void ReturnModel(GenerativeModel model) {
        try {
            var poolKey = GetPoolKeyFromModel(model);
            var pool = m_ModelPools.GetOrAdd(poolKey, static _ => new ConcurrentQueue<PooledModel>());
            var semaphore = m_PoolSemaphores.GetOrAdd(poolKey, static (_, options) => new SemaphoreSlim(options.MaxPoolSize, options.MaxPoolSize), m_Options);

            // Only return to pool if we're under the max size
            if (pool.Count < m_Options.MaxPoolSize) {
                var pooledModel = new PooledModel(model, poolKey);
                pool.Enqueue(pooledModel);
                m_Logger.LogDebug("Returned model to pool {PoolKey}, pool size: {Size}", poolKey, pool.Count);
            } else {
                m_Logger.LogDebug("Pool full for {PoolKey}, disposing model", poolKey);
            }

            // Always release the semaphore since GetModelAsync always acquires it
            semaphore.Release();
        } catch (Exception ex) {
            m_Logger.LogError(ex, "Error returning model to pool");
        }
    }

    private void CleanupIdleModels(object? state) {
        m_Logger.LogDebug("Starting cleanup of idle models");

        foreach (var (poolKey, pool) in m_ModelPools) {
            var semaphore = m_PoolSemaphores.GetOrAdd(poolKey, static (_, options) => new SemaphoreSlim(options.MaxPoolSize, options.MaxPoolSize), m_Options);

            // Don't block cleanup if we can't get the lock immediately
            if (!semaphore.Wait(TimeSpan.FromSeconds(1)))
                continue;

            try {
                var modelsToRemove = new List<PooledModel>();
                var modelsToKeep = new List<PooledModel>();

                // Drain the queue to check all models
                while (pool.TryDequeue(out var pooledModel)) {
                    if (pooledModel.IsExpired(m_Options.ModelIdleTimeout)) {
                        modelsToRemove.Add(pooledModel);
                    } else {
                        modelsToKeep.Add(pooledModel);
                    }
                }

                // Sort models by last used time (oldest first) for cleanup priority
                modelsToRemove.Sort((a, b) => a.LastUsed.CompareTo(b.LastUsed));

                // Determine how many we can actually remove while respecting MinPoolSize
                var currentPoolSize = modelsToKeep.Count + modelsToRemove.Count;
                var maxToRemove = Math.Max(0, currentPoolSize - m_Options.MinPoolSize);
                var actualModelsToRemove = modelsToRemove.Take(maxToRemove).ToList();
                var modelsToKeepFromExpired = modelsToRemove.Skip(maxToRemove).ToList();

                // Put back all models we want to keep (non-expired + expired but needed for MinPoolSize)
                foreach (var modelToKeep in modelsToKeep) {
                    pool.Enqueue(modelToKeep);
                }

                foreach (var modelToKeep in modelsToKeepFromExpired) {
                    pool.Enqueue(modelToKeep);
                }

                if (actualModelsToRemove.Count > 0) {
                    m_Logger.LogDebug("Cleaned up {Count} expired models from pool {PoolKey} (kept {Kept} to maintain MinPoolSize of {MinSize})", actualModelsToRemove.Count, poolKey, modelsToKeepFromExpired.Count, m_Options.MinPoolSize);
                }

                if (modelsToKeepFromExpired.Count > 0) {
                    m_Logger.LogDebug("Retained {Count} expired models in pool {PoolKey} to maintain MinPoolSize of {MinSize}", modelsToKeepFromExpired.Count, poolKey, m_Options.MinPoolSize);
                }

                // Release semaphores for each model we actually removed
                for (var i = 0; i < actualModelsToRemove.Count; i++) {
                    semaphore.Release();
                }
            } finally {
                semaphore.Release(); // Release the cleanup lock
            }
        }
    }

    private static string GetPoolKey(string project, string region, string model) {
        return $"{project}:{region}:{model}";
    }

    private static string GetPoolKeyFromModel(GenerativeModel model) {
        // Normalize the model name to remove any "models/" prefix to match GetPoolKey format
        var normalizedModel = model.Model.StartsWith("models/")
            ? model.Model["models/".Length..]
            : model.Model;

        return $"{model.ProjectId}:{model.Region}:{normalizedModel}";
    }

    public void Dispose() {
        m_CleanupTimer.Dispose();

        // Dispose all pooled models
        foreach (var pool in m_ModelPools.Values) {
            pool.Clear();
        }

        // Dispose semaphores
        foreach (var semaphore in m_PoolSemaphores.Values) {
            semaphore.Dispose();
        }
    }
}
