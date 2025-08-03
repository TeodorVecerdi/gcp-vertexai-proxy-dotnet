#if NET472_OR_GREATER || NETSTANDARD2_0
using System.Text.Json.Serialization;
#endif

namespace Mscc.GenerativeAI
{
    /// <summary>
    /// The media resolution
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<MediaResolution>))]
    public enum MediaResolution
    {
        /// <summary>
        /// Media resolution has not been set.
        /// </summary>
        MediaResolutionUnspecified,
        /// <summary>
        /// Media resolution set to low (64 tokens).
        /// </summary>
        MediaResolutionLow,
        /// <summary>
        /// Media resolution set to medium (256 tokens).
        /// </summary>
        MediaResolutionMedium,
        /// <summary>
        /// Media resolution set to high (zoomed reframing with 256 tokens).
        /// </summary>
        MediaResolutionHigh
    }
}