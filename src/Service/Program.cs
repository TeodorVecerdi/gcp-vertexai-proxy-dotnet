using Microsoft.AspNetCore.Routing.Constraints;
using Mscc.GenerativeAI;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options => {
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

builder.Services.Configure<RouteOptions>(options => options.SetParameterPolicy<RegexInlineRouteConstraint>("regex"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "Vertex AI Proxy API", Version = "v1" }));

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vertex AI Proxy API v1"));
}

app.MapPost("/v1/projects/{project}/locations/{region}/publishers/google/models/{model}:generateContent", async (string project, string region, string model, GenerateContentRequest request) => {
        var vertexAi = new VertexAI(project, region: region);
        var client = vertexAi.GenerativeModel(model);
        var result = await client.GenerateContent(request);
        return new List<GenerateContentResponse> { result };
    })
    .WithName("GenerateContent")
    .WithDescription("Generate content from a model")
    .Produces<List<GenerateContentResponse>>();

app.MapPost("/v1/projects/{project}/locations/{region}/publishers/google/models/{model}:streamGenerateContent", (string project, string region, string model, GenerateContentRequest request) => {
        var vertexAi = new VertexAI(project, region: region);
        var client = vertexAi.GenerativeModel(model);
        return client.GenerateContentStream(request);
    })
    .WithName("StreamGenerateContent")
    .WithDescription("Generate content from a model in streaming mode")
    .Produces<IAsyncEnumerable<GenerateContentResponse>>();

app.Run();
