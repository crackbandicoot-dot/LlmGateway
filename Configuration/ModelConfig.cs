
using System.Collections.Generic;

namespace LlmGateway.Configuration
{
    /// <summary>
    /// Represents the configuration for a single Large Language Model.
    /// It contains details about the model's name, its API endpoint, user-friendly aliases,
    /// and the specific mapping needed to communicate with its API.
    /// </summary>
    public class ModelConfig
    {
        /// <summary>
        /// Gets or sets the official name of the model as used by the provider's API
        /// (e.g., "gpt-4-turbo", "claude-3-sonnet-20240229").
        /// </summary>
        public string ModelName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a list of user-friendly aliases that can be used to refer to this model.
        /// </summary>
        public List<string> Aliases { get; set; } = new List<string>();

        /// <summary>
        // Gets or sets the specific API endpoint path for this model (e.g., "/v1/chat/completions").
        /// This path is typically appended to the provider's base URL.
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the request and response field mappings for this model's API.
        /// This defines how to translate between the library's standard format and the provider's specific format.
        /// </summary>
        public RequestResponseMapping Mapping { get; set; } = new RequestResponseMapping();
    }
}
