namespace LlmGateway.Exceptions
{
    /// <summary>
    /// Represents a provider-agnostic exception that occurs during an interaction with an LLM API.
    /// This exception is thrown for issues like network errors, API authentication failures,
    /// invalid requests, or errors returned by the provider's server.
    /// </summary>
    public class LlmException : System.Exception
    {
        /// <summary>
        /// Gets the name of the LLM provider that was being used when the exception occurred (e.g., "OpenAI", "Google").
        /// </summary>
        public string? ProviderName { get; }

        /// <summary>
        /// Gets the alias of the model that was targeted by the request.
        /// </summary>
        public string? ModelAlias { get; }

        /// <summary>
        /// Gets the HTTP status code returned by the provider's API, if available.
        /// </summary>
        public System.Net.HttpStatusCode? StatusCode { get; }

        /// <summary>
        /// Gets the original error message or content returned by the LLM provider's API, if available.
        /// </summary>
        public string? ProviderErrorMessage { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LlmException"/> class.
        /// </summary>
        public LlmException() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LlmException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public LlmException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LlmException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="inner">The exception that is the cause of the current exception.</param>
        public LlmException(string message, System.Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LlmException"/> class with detailed information about the API failure.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="providerName">The name of the LLM provider.</param>
        /// <param name="modelAlias">The alias of the model being used.</param>
        /// <param name="statusCode">The HTTP status code from the API response.</param>
        /// <param name="providerErrorMessage">The raw error message from the API provider.</param>
        /// <param name="inner">The exception that is the cause of the current exception, if any.</param>
        public LlmException(
            string message,
            string? providerName,
            string? modelAlias,
            System.Net.HttpStatusCode? statusCode,
            string? providerErrorMessage,
            System.Exception? inner = null) : base(message, inner)
        {
            ProviderName = providerName;
            ModelAlias = modelAlias;
            StatusCode = statusCode;
            ProviderErrorMessage = providerErrorMessage;
        }
    }
}
