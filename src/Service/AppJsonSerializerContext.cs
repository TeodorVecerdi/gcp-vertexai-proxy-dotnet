using System.Text.Json.Serialization;

namespace Vecerdi.VertexAIProxy.Service;

[JsonSerializable(typeof(IAsyncEnumerable<string>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }
