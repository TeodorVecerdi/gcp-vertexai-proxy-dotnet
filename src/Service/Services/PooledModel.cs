using Mscc.GenerativeAI;

namespace Vecerdi.VertexAIProxy.Service.Services;

internal sealed class PooledModel(GenerativeModel model, string poolKey) {
    public GenerativeModel Model { get; } = model;
    public string PoolKey { get; } = poolKey;
    public DateTime LastUsed { get; private set; } = DateTime.UtcNow;

    public void UpdateLastUsed() {
        LastUsed = DateTime.UtcNow;
    }

    public bool IsExpired(TimeSpan timeout) {
        return DateTime.UtcNow - LastUsed > timeout;
    }
}
