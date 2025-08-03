﻿#if NET472_OR_GREATER || NETSTANDARD2_0
using System.Collections.Generic;
#endif

namespace Mscc.GenerativeAI
{
    /// <summary>
    /// A response from `CountTokens`. It returns the model's `token_count` for the `prompt`.
    /// </summary>
    public sealed class CountTokensResponse
    {
        private int _totalTokens;
        private int _totalCachedTokens;

        /// <summary>
        /// The total number of tokens counted across all instances from the request.
        /// </summary>
        public int TotalTokens
        {
            get => _totalTokens;
            set => _totalTokens = value < 0 ? 0 : value < int.MaxValue ? value : int.MaxValue;
        }

        /// <summary>
        /// The total number of tokens counted across all instances from the request.
        /// </summary>
        public int TokenCount
        {
            get => _totalTokens;
            set => _totalTokens = value < 0 ? 0 : value < int.MaxValue ? value : int.MaxValue;
        }

        /// <summary>
        /// The total number of billable characters counted across all instances from the request.
        /// </summary>
        public int TotalBillableCharacters { get; set; } = default;
        
        /// <summary>
        /// Number of tokens in the cached part of the prompt (the cached content).
        /// </summary>
        public int CachedContentTokenCount
        {
            get => _totalCachedTokens;
            set => _totalCachedTokens = value < 0 ? 0 : value < int.MaxValue ? value : int.MaxValue;
        }
        /// <summary>
        /// Output only. List of modalities that were processed in the request input.
        /// </summary>
        public List<ModalityTokenCount>? PromptTokensDetails { get; set; }
        /// <summary>
        /// Output only. List of modalities that were processed in the cached content.
        /// </summary>
        public List<ModalityTokenCount>? CacheTokensDetails { get; set; }
    }
}