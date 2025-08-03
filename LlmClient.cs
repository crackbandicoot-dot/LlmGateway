using LlmGateway.Configuration;
using LlmGateway.Exceptions;
using LlmGateway.Models;
using LlmGateway.Client;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace LlmGateway
{
    /// <summary>
    /// The primary high-level client for interacting with Large Language Models.
    /// This class provides a unified entry point for sending requests to any configured LLM
    /// using a simple alias, and handles the underlying complexity of API differences.
    /// </summary>
    public class LlmClient
    {
        private readonly ConfigurationManager _configurationManager;
        private readonly IApiClient _apiClient;
        private readonly Dictionary<string, string> _apiKeys;

        /// <summary>
        /// Initializes a new instance of the <see cref="LlmClient"/> class.
        /// </summary>
        /// <param name="configFilePath">The file path to the JSON configuration file that defines providers and models.</param>
        public LlmClient(string configFilePath)
            : this(configFilePath, new HttpApiClient())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LlmClient"/> class with a specific IApiClient implementation.
        /// Used for dependency injection and testing.
        /// </summary>
        /// <param name="configFilePath">The file path to the JSON configuration file.</param>
        /// <param name="apiClient">An implementation of IApiClient to handle HTTP communication.</param>
        public LlmClient(string configFilePath, IApiClient apiClient)
        {
            _configurationManager = new ConfigurationManager(configFilePath);
            _apiClient = apiClient;
            _apiKeys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Sets the API key for a specific provider. The key will be stored and used for all subsequent
        /// requests to models from that provider.
        /// </summary>
        /// <param name="providerName">The name of the provider (e.g., "OpenAI", "Google"). Must match a name in the config file.</param>
        /// <param name="apiKey">The API key for the provider.</param>
        public void SetApiKey(string providerName, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(providerName))
            {
                throw new ArgumentException("Provider name cannot be null or empty.", nameof(providerName));
            }
            _apiKeys[providerName] = apiKey;
        }

        /// <summary>
        /// Asynchronously sends a prompt to a specified model and gets a response.
        /// </summary>
        /// <param name="modelAlias">The alias of the model to use (e.g., "gpt4", "default-claude").</param>
        /// <param name="userPrompt">The new user prompt to send to the model.</param>
        /// <param name="conversationHistory">Optional. A list of previous messages in the conversation to provide context.</param>
        /// <param name="systemPrompt">Optional. A system-level instruction for the model. Overrides any previous system message in the history.</param>
        /// <param name="temperature">Optional. The temperature for the model's response generation. Defaults to 0.7.</param>
        /// <returns>A task that resolves to an <see cref="LlmResponse"/> containing the model's answer.</returns>
        /// <exception cref="LlmException">Thrown if the model alias is not found, the API key is missing, or an API error occurs.</exception>
        public async Task<LlmResponse> GetChatCompletionAsync(
            string modelAlias,
            string userPrompt,
            IEnumerable<ChatMessage>? conversationHistory = null,
            string? systemPrompt = null,
            double temperature = 0.7)
        {
            // 1. Validate that userPrompt is not null or empty.
            if (string.IsNullOrWhiteSpace(userPrompt))
            {
                throw new ArgumentException("User prompt cannot be null or empty.", nameof(userPrompt));
            }

            // 2. Use _configurationManager.TryGetModelConfigByAlias to get the provider and model configs.
            if (!_configurationManager.TryGetModelConfigByAlias(modelAlias, out var providerConfig, out var modelConfig) || providerConfig == null || modelConfig == null)
            {
                throw new LlmException($"Model alias '{modelAlias}' not found in configuration.");
            }

            // 3. Try to get the API key for the provider.
            if (!_apiKeys.TryGetValue(providerConfig.ProviderName, out var apiKey) || string.IsNullOrEmpty(apiKey))
            {
                throw new LlmException($"API key for provider '{providerConfig.ProviderName}' has not been set. Use SetApiKey().");
            }

            // 4. Create a mutable list of ChatMessages.
            var finalConversation = new List<ChatMessage>();
            string? finalSystemPrompt = systemPrompt;

            var historyWithoutSystem = conversationHistory?.Where(m => m.Role != MessageRole.System);

            // 5. If a systemPrompt is provided, it takes precedence. Otherwise, look for one in the history.
            if (finalSystemPrompt == null)
            {
                finalSystemPrompt = conversationHistory?.FirstOrDefault(m => m.Role == MessageRole.System)?.Content;
            }

            // 6. Append the conversationHistory, if provided (excluding any system messages).
            if (historyWithoutSystem != null)
            {
                finalConversation.AddRange(historyWithoutSystem);
            }

            // 7. Append the new userPrompt as a User role message.
            finalConversation.Add(new ChatMessage(MessageRole.User, userPrompt));

            // 8. Create an LlmRequest object with all the prepared data.
            var llmRequest = new LlmRequest(modelAlias, finalSystemPrompt, finalConversation, temperature);

            // 9. Call _apiClient.SendRequestAsync.
            string responseContent = await _apiClient.SendRequestAsync(providerConfig, modelConfig, llmRequest, apiKey);

            // 10. Create an LlmResponse.
            var llmResponse = new LlmResponse(responseContent, llmRequest);

            // 11. Return the LlmResponse.
            return llmResponse;
        }
    }
}
