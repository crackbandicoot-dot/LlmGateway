namespace LlmGateway.Models
{
    /// <summary>
    /// Represents a standardized, internal response received from an LLM.
    /// This object provides a consistent structure for the result of an API call,
    /// regardless of the provider.
    /// </summary>
    public class LlmResponse
    {
        /// <summary>
        /// Gets the TougthLine followed by the model in its response 
        /// </summary>
        public string Content { get; }
        
        /// <summary>
        /// Gets the original request that prompted this response. This is useful for
        /// maintaining context in a conversation.
        /// </summary>

        public LlmRequest OriginalRequest { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LlmResponse"/> class.
        /// </summary>
        /// <param name="content">The content of the model's response.</param>
        /// <param name="originalRequest">The request that generated this response.</param>
        public LlmResponse(string content, LlmRequest originalRequest)
        {
            Content = content;
            OriginalRequest = originalRequest;
        }
    }
}
