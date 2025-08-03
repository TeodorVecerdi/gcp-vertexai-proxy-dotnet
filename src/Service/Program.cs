using Scalar.AspNetCore;
using Vecerdi.VertexAIProxy.Service;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options
    => options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/", GetNames)
    .WithName("GetNames")
    .WithDescription("Get all names");
app.MapGet("/{name}", (string name) => $"Hello, {name}!")
    .WithName("Greet")
    .WithDescription("Say hello to a name");

// app.MapGet("/{id}", Results<Ok<Todo>, NotFound> (int id) =>
//       sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
//           ? TypedResults.Ok(todo)
//           : TypedResults.NotFound())
//   .WithName("GetTodoById");

app.Run();
return;

static async IAsyncEnumerable<string> GetNames() {
    await Task.CompletedTask;
    yield return "John";
    yield return "Jane";
    yield return "Bob";
}
