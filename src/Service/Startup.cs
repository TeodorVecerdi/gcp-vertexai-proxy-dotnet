using Google.Cloud.Functions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Mscc.GenerativeAI;

namespace Vecerdi.VertexAIProxy.Service;

public sealed class Startup : FunctionsStartup {
    public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection services) {
        base.ConfigureServices(context, services);

        services.AddRouting();
        services.AddHttpContextAccessor();
        services.AddProblemDetails();

        services.ConfigureHttpJsonOptions(options => {
            var referenceOptions = BaseModel.DefaultJsonSerializerOptions();
            options.SerializerOptions.WriteIndented = referenceOptions.WriteIndented;
            options.SerializerOptions.DefaultIgnoreCondition = referenceOptions.DefaultIgnoreCondition;
            options.SerializerOptions.PropertyNamingPolicy = referenceOptions.PropertyNamingPolicy;
            options.SerializerOptions.DictionaryKeyPolicy = referenceOptions.DictionaryKeyPolicy;
            options.SerializerOptions.NumberHandling = referenceOptions.NumberHandling;
            options.SerializerOptions.PropertyNameCaseInsensitive = referenceOptions.PropertyNameCaseInsensitive;
            options.SerializerOptions.ReadCommentHandling = referenceOptions.ReadCommentHandling;
            options.SerializerOptions.AllowTrailingCommas = referenceOptions.AllowTrailingCommas;
            options.SerializerOptions.UnmappedMemberHandling = referenceOptions.UnmappedMemberHandling;
            options.SerializerOptions.RespectNullableAnnotations = referenceOptions.RespectNullableAnnotations;
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, new GenerativeJsonSerializerContext());
        });

        services.Configure<RouteOptions>(options => options.SetParameterPolicy<RegexInlineRouteConstraint>("regex"));
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo {
            Title = "Vertex AI Proxy API",
            Version = "v1",
        }));
    }

    public override void Configure(WebHostBuilderContext context, IApplicationBuilder app) {
        base.Configure(context, app);

        if (context.HostingEnvironment.IsDevelopment()) {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vertex AI Proxy API v1"));
        }

        app.UseRouting();
        app.UseEndpoints(endpoints => {
                endpoints.MapPost("/v1/projects/{project}/locations/{region}/publishers/google/models/{model}:generateContent", async (string project, string region, string model, GenerateContentRequest request) => {
                        var vertexAi = new VertexAI(project, region: region);
                        var client = vertexAi.GenerativeModel(model);
                        var result = await client.GenerateContent(request);
                        return new List<GenerateContentResponse> { result };
                    })
                    .WithName("GenerateContent")
                    .WithDescription("Generate content from a model")
                    .Produces<List<GenerateContentResponse>>();

                endpoints.MapPost("/v1/projects/{project}/locations/{region}/publishers/google/models/{model}:streamGenerateContent", (string project, string region, string model, GenerateContentRequest request) => {
                        var vertexAi = new VertexAI(project, region: region);
                        var client = vertexAi.GenerativeModel(model);
                        return client.GenerateContentStream(request);
                    })
                    .WithName("StreamGenerateContent")
                    .WithDescription("Generate content from a model in streaming mode")
                    .Produces<IAsyncEnumerable<GenerateContentResponse>>();
            }
        );
    }
}
