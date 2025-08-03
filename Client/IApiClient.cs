
using LlmGateway.Configuration;
using LlmGateway.Models;
using System.Threading.Tasks;

namespace LlmGateway.Client
{
    /// <summary>
    /// Defines the contract for a low-level API client that communicates with an LLM provider.
    /// Implementations are responsible for constructing provider-specific requests,
    /// sending them, and parsing the responses into a standardized format.
    /// </summary>
    public interface IApiClient
    {
        /// <summary>
        /// Asynchronously sends a request to an LLM provider's API.
        /// </summary>
        /// <param name="providerConfig">The configuration of the target provider, including base URL and auth details.</param>
        /// <param name="modelConfig">The configuration of the target model, including the endpoint and JSON mappings.</param>
        /// <param name="request">The standardized LlmRequest object containing the prompt, history, and other parameters.</param>
        /// <param name="apiKey">The API key required for authenticating with the provider.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the string content
        /// of the LLM's response, extracted from the provider's JSON response according to the mapping.
        /// </returns>
        /// <exception cref="LlmApiLibrary.Exceptions.LlmException">
        /// Thrown if the API call fails due to network issues, authentication errors, invalid input,
        /// or an error response from the provider's server. The exception should be populated with
        /// details from the API response.
        /// </exception>
        Task<string> SendRequestAsync(ProviderConfig providerConfig, ModelConfig modelConfig, LlmRequest request, string apiKey);
    }
}
