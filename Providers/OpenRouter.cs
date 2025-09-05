
using LlmGateway.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization; // Required for JsonPropertyName
using System.Threading.Tasks;

namespace LlmGateway.Providers
{
    /// <summary>
    /// This class implements the OpenRouter API client.
    /// </summary>
    internal class OpenRouter : IProvider
    {
        private const string ApiEndpoint = "https://openrouter.ai/api/v1/chat/completions";
        private static readonly HttpClient _httpClient = new HttpClient();

        public string APIKey => Environment.GetEnvironmentVariable("OpenRouter");

        public async Task<LlmResponse> GetApiResponse(LlmRequest llmRequest)
        {
            if (string.IsNullOrEmpty(APIKey))
            {
                throw new InvalidOperationException("OPENROUTER_API_KEY environment variable is not set.");
            }

            // --- FIX 1: Add Guard Clause ---
            // Ensure there is at least one message to send to the API.
            if (string.IsNullOrWhiteSpace(llmRequest.SystemPrompt) && (llmRequest.ConversationHistory == null || !llmRequest.ConversationHistory.Any()))
            {
                throw new ArgumentException("LlmRequest must have either a SystemPrompt or at least one message in ConversationHistory.", nameof(llmRequest));
            }

            // Set up the required header for authentication.
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", APIKey);

            var messages = new List<OpenRouterMessage>();

            if (!string.IsNullOrWhiteSpace(llmRequest.SystemPrompt))
            {
                messages.Add(new OpenRouterMessage("system", llmRequest.SystemPrompt));
            }

            for (int i = 0; i < llmRequest.ConversationHistory.Count; i++)
            {
                var role = (i % 2 == 0) ? "user" : "assistant";
                messages.Add(new OpenRouterMessage(role, llmRequest.ConversationHistory[i]));
            }

            var requestPayload = new OpenRouterRequest
            {
                Model = llmRequest.ModelName,
                Messages = messages,
                Temperature = llmRequest.Temperature
            };

            HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync(ApiEndpoint, requestPayload);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error calling OpenRouter API: {httpResponse.StatusCode}. Details: {errorContent}");
            }

            var apiResponse = await httpResponse.Content.ReadFromJsonAsync<OpenRouterResponse>();
            string content = apiResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;

            return new LlmResponse(content, llmRequest);
        }

        #region Helper Classes for JSON Deserialization

        // --- FIX 2: Use JsonPropertyName for explicit serialization ---
        private class OpenRouterRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; }

            [JsonPropertyName("messages")]
            public List<OpenRouterMessage> Messages { get; set; }

            [JsonPropertyName("temperature")]
            public double Temperature { get; set; }
        }

        private class OpenRouterMessage
        {
            [JsonPropertyName("role")]
            public string Role { get; set; }

            [JsonPropertyName("content")]
            public string Content { get; set; }

            public OpenRouterMessage(string role, string content)
            {
                Role = role;
                Content = content;
            }
        }

        private class OpenRouterResponse
        {
            [JsonPropertyName("choices")]
            public List<OpenRouterChoice> Choices { get; set; }
        }

        private class OpenRouterChoice
        {
            [JsonPropertyName("message")]
            public OpenRouterMessage Message { get; set; }
        }

        #endregion
    }
}
