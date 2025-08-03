using System.Text.Json.Serialization;
using Mscc.GenerativeAI;

namespace Vecerdi.VertexAIProxy.Service;

[JsonSerializable(typeof(IAsyncEnumerable<string>))]
[JsonSerializable(typeof(GenerateContentRequest))]
[JsonSerializable(typeof(GenerateContentResponse))]
[JsonSerializable(typeof(Content))]
[JsonSerializable(typeof(Credentials))]
[JsonSerializable(typeof(List<GenerateContentResponse>))]
public partial class AppJsonSerializerContext : JsonSerializerContext { }
