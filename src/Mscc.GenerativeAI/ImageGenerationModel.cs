#if NET472_OR_GREATER || NETSTANDARD2_0
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
#endif
using Microsoft.Extensions.Logging;
using System.Text;

namespace Mscc.GenerativeAI
{
    /// <summary>
    /// Name of the model that supports image generation.
    /// The <see cref="ImageGenerationModel"/> can create high quality visual assets in seconds and brings Google's state-of-the-art vision and multimodal generative AI capabilities to application developers.
    /// </summary>
    public sealed class ImageGenerationModel : BaseModel
    {
        private const string UrlGoogleAi = "{BaseUrlGoogleAi}/{model}:{method}";
        private const string UrlVertexAi = "{BaseUrlVertexAi}/publishers/{publisher}/{model}:{method}";

        private static readonly string[] AspectRatio = ["1:1", "9:16", "16:9", "4:3", "3:4"];
        private static readonly string[] SafetyFilterLevel =
            ["BLOCK_LOW_AND_ABOVE", "BLOCK_MEDIUM_AND_ABOVE", "BLOCK_ONLY_HIGH", "BLOCK_NONE"];

        private readonly bool _useVertexAi;

        internal override string Version => _useVertexAi ? ApiVersion.V1 : ApiVersion.V1Beta;

        private string Url => _useVertexAi ? UrlVertexAi : UrlGoogleAi;
        
        private string Method => GenerativeAI.Method.Predict;
        
        internal bool IsVertexAI => _useVertexAi;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageGenerationModel"/> class.
        /// </summary>
        public ImageGenerationModel() : this(logger: null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageGenerationModel"/> class.
        /// The default constructor attempts to read <c>.env</c> file and environment variables.
        /// Sets default values, if available.
        /// </summary>
        public ImageGenerationModel(ILogger? logger = null) : base(logger) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageGenerationModel"/> class with access to Google AI Gemini API.
        /// </summary>
        /// <param name="apiKey">API key provided by Google AI Studio</param>
        /// <param name="model">Model to use</param>
        /// <param name="logger">Optional. Logger instance used for logging</param>
        internal ImageGenerationModel(string? apiKey = null,
            string? model = null, 
            ILogger? logger = null) : this(logger)
        {
            ApiKey = apiKey ?? _apiKey;
            Model = model ?? _model;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageGenerationModel"/> class with access to Vertex AI Gemini API.
        /// </summary>
        /// <param name="projectId">Identifier of the Google Cloud project</param>
        /// <param name="region">Region to use</param>
        /// <param name="model">Model to use</param>
        /// <param name="logger">Optional. Logger instance used for logging</param>
        public ImageGenerationModel(string? projectId = null, string? region = null,
            string? model = null, ILogger? logger = null) : base(projectId, region, model, logger)
        {
            _useVertexAi = true;
        }

        /// <summary>
        /// Generates images from the specified <see cref="ImageGenerationRequest"/>.
        /// </summary>
        /// <param name="request">Required. The request to send to the API.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Response from the model for generated images.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="request"/> is <see langword="null"/>.</exception>
        public async Task<ImageGenerationResponse> GenerateImages(ImageGenerationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var url = ParseUrl(Url, Method);
            var json = Serialize(request);
            var payload = new StringContent(json, Encoding.UTF8, Constants.MediaType);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
            httpRequest.Content = payload;
            var response = await SendAsync(httpRequest, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await Deserialize<ImageGenerationResponse>(response);
        }

        /// <summary>
        /// Generates images from text prompt.
        /// </summary>
        /// <param name="prompt">Required. String to process.</param>
        /// <param name="numberOfImages">Number of images to generate. Range: 1..8.</param>
        /// <param name="negativePrompt">A description of what you want to omit in the generated images.</param>
        /// <param name="aspectRatio">Aspect ratio for the image.</param>
        /// <param name="guidanceScale">Controls the strength of the prompt. Suggested values are - * 0-9 (low strength) * 10-20 (medium strength) * 21+ (high strength)</param>
        /// <param name="language">Language of the text prompt for the image.</param>
        /// <param name="safetyFilterLevel">Adds a filter level to Safety filtering.</param>
        /// <param name="personGeneration">Allow generation of people by the model.</param>
        /// <param name="enhancePrompt">Option to enhance your provided prompt.</param>
        /// <param name="addWatermark">Explicitly set the watermark</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Response from the model for generated content.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="prompt"/> is <see langword="null"/>.</exception>
        /// <exception cref="HttpRequestException">Thrown when the request fails to execute.</exception>
        public async Task<ImageGenerationResponse> GenerateImages(string prompt,
            int numberOfImages = 1, string? negativePrompt = null, 
            string? aspectRatio = null, int? guidanceScale = null,
            ImagePromptLanguage? language = null, string? safetyFilterLevel = null,
            PersonGeneration? personGeneration = null, bool? enhancePrompt = null,
            bool? addWatermark = null,
            CancellationToken cancellationToken = default)
        {
            if (prompt == null) throw new ArgumentNullException(nameof(prompt));

            var request = new ImageGenerationRequest(prompt, numberOfImages);
            if (!string.IsNullOrEmpty(aspectRatio))
            {
                if (!AspectRatio.Contains(aspectRatio)) 
                    throw new ArgumentException("Not a valid aspect ratio", nameof(aspectRatio));
                request.Parameters.AspectRatio = aspectRatio;
            }
            request.Parameters.NegativePrompt ??= negativePrompt;
            request.Parameters.GuidanceScale ??= guidanceScale;
            request.Parameters.Language ??= language;
            if (!string.IsNullOrEmpty(safetyFilterLevel))
            {
                if (!SafetyFilterLevel.Contains(safetyFilterLevel.ToUpperInvariant()))
                    throw new ArgumentException("Not a valid safety filter level", nameof(safetyFilterLevel));
                request.Parameters.SafetyFilterLevel = safetyFilterLevel.ToUpperInvariant();
            }
            if (personGeneration is not null)
            {
                request.Parameters.PersonGeneration = personGeneration;
            }
            request.Parameters.EnhancePrompt = enhancePrompt;
            request.Parameters.AddWatermark = addWatermark;
            
            return await GenerateImages(request, cancellationToken);
        }

        /// <summary>
        /// Generates a response from the model given an input prompt and other parameters.
        /// </summary>
        /// <param name="prompt">Required. String to process.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Response from the model for generated content.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="prompt"/> is <see langword="null"/>.</exception>
        /// <exception cref="HttpRequestException">Thrown when the request fails to execute.</exception>
        public async Task<ImageGenerationResponse> GenerateContent(string prompt,
            CancellationToken cancellationToken = default)
        {
            return await GenerateImages(prompt, cancellationToken: cancellationToken);
        }
    }
}