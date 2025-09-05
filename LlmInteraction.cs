using LlmGateway.Exceptions;
using LlmGateway.Models;
using LlmGateway.Providers;

namespace LlmGateway
{
    
    ///<summary>
    /// Provides a unified high-level client for interacting with Large Language Models (LLMs).
    /// This class abstracts the details of different LLM providers, allowing requests to be sent
    /// using a simple provider alias. It manages provider instances and handles API differences internally.
    /// </summary>
    public class LlmInteraction
    {
        private Dictionary<string, IProvider> _providers;

        /// <summary>
        /// Initializes a new instance of the <see cref="LlmInteraction"/> class,
        /// registering available LLM providers.
        /// </summary>
        public LlmInteraction()
        {
            _providers = new Dictionary<string, IProvider>
            {
                { "Google", new Google() },
                { "OpenRouter", new OpenRouter() }
            };
        }

        /// <summary>
        /// Asynchronously sends a prompt to the specified provider's model and returns the response.
        /// </summary>
        /// <param name="providerName">The alias of the provider to use currently just "Google" and "OpenRouter" are avaible .</param>
        /// <param name="request">The standardized request to send to the provider.</param>
        /// <returns>A task that resolves to an <see cref="LlmResponse"/> containing the model's answer.</returns>
        /// <exception cref="LlmException">
        /// Thrown if the provider alias is not found, the API key is missing, or an API error occurs.
        /// </exception>
        public async Task<LlmResponse> GetChatCompletionAsync(
            string providerName,
            LlmRequest request
            )
        {
            var provider = _providers[providerName];
            var llmResponse = await provider.GetApiResponse(request);
            return llmResponse;
        }
    }
}
