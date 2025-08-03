using System.Text.Json.Serialization;

namespace Vecerdi.VertexAIProxy.Service;

[JsonSerializable(typeof(string))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }
