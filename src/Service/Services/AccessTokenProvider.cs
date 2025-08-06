using Google.Apis.Auth.OAuth2;

namespace Vecerdi.VertexAIProxy.Service.Services;

public interface IAccessTokenProvider {
    Task<string> GetAccessTokenAsync();
}

internal sealed class AccessTokenProvider : IAccessTokenProvider {
    private string? m_CachedAccessToken;
    private DateTime m_TokenExpiry = DateTime.MinValue;

    public async Task<string> GetAccessTokenAsync() {
        if (m_CachedAccessToken == null || DateTime.UtcNow >= m_TokenExpiry) {
            var credential = await GoogleCredential.GetApplicationDefaultAsync();
            m_CachedAccessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
            m_TokenExpiry = DateTime.UtcNow.AddMinutes(5);
        }

        return m_CachedAccessToken;
    }
}
