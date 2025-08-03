#if NET472_OR_GREATER || NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
#endif
using System.Diagnostics;

namespace Mscc.GenerativeAI
{
    /// <summary>
    /// The base structured datatype containing multipart content of a message.
    /// Ref: https://ai.google.dev/api/rest/v1beta/Content
    /// </summary>
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public sealed class Content
    {
        /// <summary>
        /// Ordered Parts that constitute a single message. Parts may have different MIME types.
        /// </summary>
        public List<Part>? Parts { get; set; }
        /// <summary>
        /// Optional. The producer of the content. Must be either 'user' or 'model'.
        /// Useful to set for multi-turn conversations, otherwise can be left blank or unset.
        /// </summary>
        [JsonPropertyOrder(-1)]
        public string? Role { get; set; }

        /// <summary>
        /// The ETag of the item.
        /// </summary>
        public string? ETag { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Content"/> class.
        /// </summary>
        public Content()
        {
            Parts = new List<Part>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Content"/> class.
        /// </summary>
        /// <param name="text">String to process.</param>
        /// <param name="role">Provide the <see cref="GenerativeAI.Role"/> of the text.</param>
        public Content(string text, string role = GenerativeAI.Role.User) : this()
        {
            Role = role;
            Parts?.Add(new Part { Text = text });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Content"/> class.
        /// </summary>
        /// <param name="part">The part to add.</param>
        /// <param name="role">Provide the <see cref="GenerativeAI.Role"/> of the text.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="part"/> is null.</exception>
        public Content(Part part, string role = GenerativeAI.Role.User) : this()
        {
            if (part is null) throw new ArgumentNullException(nameof(part));
            Role = role;
            Parts?.Add(part);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Content"/> class.
        /// </summary>
        /// <param name="parts">The parts to add.</param>
        /// <param name="role">Provide the <see cref="GenerativeAI.Role"/> of the text.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parts"/> is null.</exception>
        public Content(IEnumerable<Part> parts, string role = GenerativeAI.Role.User) : this()
        {
            if (parts is null) throw new ArgumentNullException(nameof(parts));
            Role = role;
            Parts?.AddRange(parts);
        }

        private string GetDebuggerDisplay()
        {
            return $"Role: {Role} - Parts: {Parts?.Count}";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public sealed class ContentResponse
    {
        public List<Part> Parts { get; set; }
        public string Role { get; set; }

        [JsonIgnore]
        public string Text
        {
            get
            {
                if (Parts.Count > 0)
                {
                    return Parts[0].Text ?? "";
                }
                return string.Empty;
            }
            set
            {
                if (Parts.Count == 0)
                {
                    Parts.Add(new Part() { Text = value });
                }
                else
                {
                    Parts[0].Text = value;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentResponse"/> class.
        /// </summary>
        public ContentResponse() => Parts = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentResponse"/> class.
        /// </summary>
        /// <param name="text">String to process.</param>
        /// <param name="role">Role of the content. Must be either 'user' or 'model'.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="text"/> or <paramref name="role"/> is empty or null.</exception>
        public ContentResponse(string text, string role = GenerativeAI.Role.User) : this()
        {
            // if (string.IsNullOrEmpty(text)) throw new ArgumentException(nameof(text));
            // if (string.IsNullOrEmpty(role)) throw new ArgumentException(nameof(role));

            Parts.Add(new Part() { Text = text });
            Role = role;
        }

        private string GetDebuggerDisplay()
        {
            return $"Role: {Role} - Parts: {Parts.Count} - {Text}";
        }
    }
}