using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Mscc.GenerativeAI;
using Vecerdi.VertexAIProxy.Service.Services;

namespace Vecerdi.VertexAIProxy.Service.Extensions;

public static class VertexEndpointRouteBuilderExtensions {
    public static IEndpointRouteBuilder MapVertexEndpoints(this IEndpointRouteBuilder endpoints) {
        endpoints.MapPost("/v1/projects/{project}/locations/{region}/publishers/google/models/{model}:generateContent", GenerateContent)
            .WithName("GenerateContent")
            .WithDescription("Generate content from a model")
            .Produces<List<GenerateContentResponse>>();
        endpoints.MapPost("/v1/projects/{project}/locations/{region}/publishers/google/models/{model}:streamGenerateContent", StreamGenerateContent)
            .WithName("StreamGenerateContent")
            .WithDescription("Generate content from a model in streaming mode")
            .Produces<IAsyncEnumerable<GenerateContentResponse>>();
        return endpoints;
    }

    private static async Task<List<GenerateContentResponse>> GenerateContent(string project, string region, string model, GenerateContentRequest request, IGenerativeModelPool modelPool) {
        var client = await modelPool.GetModelAsync(project, region, model);
        try {
            var result = await client.GenerateContent(request);
            return [result];
        } finally {
            modelPool.ReturnModel(client);
        }
    }

    private static async Task<IAsyncEnumerable<GenerateContentResponse>> StreamGenerateContent(string project, string region, string model, GenerateContentRequest request, IGenerativeModelPool modelPool) {
        var client = await modelPool.GetModelAsync(project, region, model);
        return PooledStreamGenerateContent(modelPool, client, request);
    }

    private static async IAsyncEnumerable<GenerateContentResponse> PooledStreamGenerateContent(IGenerativeModelPool modelPool, GenerativeModel model, GenerateContentRequest request) {
        try {
            await foreach (var response in model.GenerateContentStream(request)) {
                yield return response;
            }
        } finally {
            modelPool.ReturnModel(model);
        }
    }
}
