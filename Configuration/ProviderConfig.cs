
using System.Collections.Generic;

namespace LlmGateway.Configuration
{
    /// <summary>
    /// Represents the configuration for a specific LLM provider (e.g., OpenAI, Google, Anthropic).
    /// It includes the provider's name, authentication details, base URL, and a list of all models
    /// available from that provider.
    /// </summary>
    public class ProviderConfig
    {
        /// <summary>
        /// Gets or sets the unique name of the provider.
        /// </summary>
        public string ProviderName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the base URL for the provider's API (e.g., "https://api.openai.com").
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the HTTP header used for authentication (e.g., "Authorization", "x-api-key").
        /// </summary>
        public string AuthHeaderName { get; set; } = "Authorization";

        /// <summary>
        /// Gets or sets a template for the authentication header value.
        /// The placeholder "{ApiKey}" will be replaced with the actual API key.
        /// For example: "Bearer {ApiKey}".
        /// </summary>
        public string AuthHeaderValueTemplate { get; set; } = "Bearer {ApiKey}";

        /// <summary>
        /// Gets or sets the list of models configured for this provider.
        /// </summary>
        public List<ModelConfig> Models { get; set; } = new List<ModelConfig>();
    }
}