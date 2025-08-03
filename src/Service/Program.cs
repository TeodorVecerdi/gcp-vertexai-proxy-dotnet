using Mscc.GenerativeAI;
using Scalar.AspNetCore;
using Vecerdi.VertexAIProxy.Service;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options
    => options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));
BaseModel.ConfigureJsonSerializerOptions = options => options.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapPost("/v1/projects/{project}/locations/{region}/publishers/google/models/{model}:generateContent", async (string project, string region, string model, GenerateContentRequest request) => {
        var vertexAi = new VertexAI(project, region: region);
        var client = vertexAi.GenerativeModel(model);

        return await client.GenerateContent(request);
    })
    .WithName("GenerateContent")
    .WithDescription("Generate content from a model");

app.Run();
