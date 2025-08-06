using Google.Cloud.Functions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Mscc.GenerativeAI;
using Vecerdi.VertexAIProxy.Service.Configuration;
using Vecerdi.VertexAIProxy.Service.Extensions;
using Vecerdi.VertexAIProxy.Service.Services;

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

        services.AddHealthChecks();

        services.Configure<GenerativeModelPoolOptions>(context.Configuration.GetSection("GenerativeModelPool"));
        services.AddSingleton<IAccessTokenProvider, AccessTokenProvider>();
        services.AddSingleton<IVertexAIFactory, VertexAIFactory>();
        services.AddSingleton<IGenerativeModelPool, GenerativeModelPool>();
    }

    public override void Configure(WebHostBuilderContext context, IApplicationBuilder app) {
        base.Configure(context, app);

        if (context.HostingEnvironment.IsDevelopment()) {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vertex AI Proxy API v1"));
        }

        app.UseRouting();
        app.UseEndpoints(endpoints => {
                endpoints.MapHealthChecks("/alive");
                endpoints.MapVertexEndpoints();
            }
        );
    }
}
