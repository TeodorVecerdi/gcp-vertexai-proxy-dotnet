using Google.Apis.Auth.OAuth2;
using Mscc.GenerativeAI;

namespace Vecerdi.VertexAIProxy.Service.Services;

public interface IVertexAIFactory {
    Task<VertexAI> CreateAsync(string projectId, string region);
}

public sealed class VertexAIFactory : IVertexAIFactory {
    private string? m_CachedAccessToken;
    private DateTime m_TokenExpiry = DateTime.MinValue;

    public async Task<VertexAI> CreateAsync(string projectId, string region) {
        if (m_CachedAccessToken == null || DateTime.UtcNow >= m_TokenExpiry.AddMinutes(-5)) {
            var credential = await GoogleCredential.GetApplicationDefaultAsync();
            m_CachedAccessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
            m_TokenExpiry = DateTime.UtcNow.AddMinutes(10);
        }

        Environment.SetEnvironmentVariable("VERTEX_ACCESS_TOKEN", m_CachedAccessToken);
        return new VertexAI(projectId, region: region);
    }
}
