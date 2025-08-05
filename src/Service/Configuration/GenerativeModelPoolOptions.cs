namespace Vecerdi.VertexAIProxy.Service.Configuration;

public sealed class GenerativeModelPoolOptions {
    public int MaxPoolSize { get; set; } = 20;
    public int MinPoolSize { get; set; } = 1;
    public TimeSpan ModelIdleTimeout { get; set; } = TimeSpan.FromMinutes(30);
}
