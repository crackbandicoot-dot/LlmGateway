using LlmGateway.Models;
namespace LlmGateway.Providers
{
    /// <summary>
    /// Defines the contract for an LLM provider.
    /// Implementations of this interface are responsible for handling API requests
    /// to a specific LLM service, using a provider-specific API key.
    /// </summary>
    public interface IProvider
    {
        /// <summary>
        /// Gets the API key used to authenticate requests to the provider's service.
        /// </summary>
        string APIKey { get; }

        /// <summary>
        /// Sends a standardized <see cref="LlmRequest"/> to the provider's API and returns a <see cref="LlmResponse"/>.
        /// </summary>
        /// <param name="llmRequest">The request containing all necessary information for the API call.</param>
        /// <returns>A task representing the asynchronous operation, with a <see cref="LlmResponse"/> as the result.</returns>
        Task<LlmResponse> GetApiResponse(LlmRequest llmRequest);
    }
}