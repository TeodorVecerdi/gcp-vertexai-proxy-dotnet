namespace Mscc.GenerativeAI
{
    [JsonSerializable(typeof(Credentials))]
    [JsonSerializable(typeof(GenerateContentRequest))]
    [JsonSerializable(typeof(List<GenerateContentResponse>))]
    [JsonSerializable(typeof(IAsyncEnumerable<GenerateContentResponse>))]
    [JsonSerializable(typeof(JsonElement))]
    [JsonSourceGenerationOptions(
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonKnownNamingPolicy.CamelCase,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
        RespectNullableAnnotations = true
    )]
    public partial class GenerativeJsonSerializerContext : JsonSerializerContext { }
}