#if NET472_OR_GREATER || NETSTANDARD2_0
using System.Text.Json.Serialization;
#endif

namespace Mscc.GenerativeAI
{
    /// <summary>
    /// The modality associated with a token count.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<Modality>))]
    public enum Modality
    {
        /// <summary>
        /// Unspecified modality.
        /// </summary>
        ModalityUnspecified = 0,
        /// <summary>
        /// Plain text.
        /// </summary>
        Text,
        /// <summary>
        /// Image.
        /// </summary>
        Image,
        /// <summary>
        /// Video.
        /// </summary>
        Video,
        /// <summary>
        /// Audio.
        /// </summary>
        Audio,
        /// <summary>
        /// Document, e.g. PDF.
        /// </summary>
        Document
    }
}