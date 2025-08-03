
namespace LlmGateway.Configuration
{
    /// <summary>
    /// Defines the JSON path mappings required to translate the library's standard request/response
    /// models to a specific LLM provider's API format. This allows for adapting to different
    /// JSON structures without changing the code.
    /// </summary>
    public class RequestResponseMapping
    {
        /// <summary>
        /// Gets or sets the JSONPath expression to locate the system prompt in the request payload.
        /// Can be null if not supported or part of the messages array.
        /// </summary>
        public string? SystemPromptPath { get; set; }

        /// <summary>
        /// Gets or sets the JSONPath expression for the array where the list of ChatMessage objects should be placed.
        /// </summary>
        public string MessagesArrayPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the relative JSONPath from an object within the MessagesArrayPath to its role property.
        /// </summary>
        public string MessageRolePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the relative JSONPath from an object within the MessagesArrayPath to its content property.
        /// </summary>
        public string MessageContentPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the JSONPath expression to locate the temperature setting in the request payload.
        /// </summary>
        public string TemperaturePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the JSONPath expression to extract the main text content from the provider's successful JSON response.
        /// </summary>
        public string ResponseContentPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the JSONPath expression to extract the error message from the provider's error JSON response.
        /// </summary>
        public string ResponseErrorPath { get; set; } = string.Empty;
    }
}
