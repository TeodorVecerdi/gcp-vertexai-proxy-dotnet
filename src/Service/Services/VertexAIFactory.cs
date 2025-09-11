using Google.Apis.Auth.OAuth2;
using Mscc.GenerativeAI;

namespace Vecerdi.VertexAIProxy.Service.Services;

public interface IVertexAIFactory {
    Task<VertexAI> CreateAsync(string projectId, string region);
}

public sealed class VertexAIFactory(IAccessTokenProvider accessTokenProvider) : IVertexAIFactory {
    public async Task<VertexAI> CreateAsync(string projectId, string region) {
        Environment.SetEnvironmentVariable("GOOGLE_ACCESS_TOKEN", await accessTokenProvider.GetAccessTokenAsync());
        return new VertexAI(projectId, region: region);
    }
}
