using LlmGateway.Models;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LlmGateway.Providers
{
    /// <summary>
    /// Google API for LLM models, it should be implemented using HttpClient C# class.
    /// This implementation uses prompt engineering to extract a "thought process" when requested.
    /// </summary>
    internal class Google : IProvider
    {
        // It's best practice to retrieve secrets from environment variables or a secret manager.
        public string APIKey => Environment.GetEnvironmentVariable("Google")!;

        // HttpClient is designed to be reused. A single static instance is recommended.
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string ApiBaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/";

        public async Task<LlmResponse> GetApiResponse(LlmRequest llmRequest)
        {
            var apiRequest = BuildApiRequest(llmRequest);
            var requestUrl = $"{ApiBaseUrl}{llmRequest.ModelName}:generateContent?key={APIKey}";

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync(requestUrl, apiRequest, jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    // Provide a more informative error response
                    return new LlmResponse($"Error: {response.StatusCode}"+ $"API call failed. Details: {errorContent}", llmRequest);
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<GoogleApiResponse>(jsonOptions);
                var fullResponseText = apiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? string.Empty;
                // If not including thinking, the whole response is the content.
                return new LlmResponse(fullResponseText, llmRequest);
            }
            catch (Exception ex)
            {
                // In a real application, you'd use a proper logging framework.
                Console.WriteLine($"An error occurred: {ex.Message}");
                return new LlmResponse($"An internal error occurred."+ $"Exception: {ex.Message}", llmRequest);
            }
        }

        private GoogleApiRequest BuildApiRequest(LlmRequest llmRequest)
        {
            var contents = new List<ContentItem>();
            bool isUserTurn = true; // Assuming the history starts with a user message

            // Gemini API expects an alternating user/model conversation.
            // We need to ensure the last message is from the user for the model to respond to.
            foreach (var message in llmRequest.ConversationHistory)
            {
                contents.Add(new ContentItem
                {
                    Role = isUserTurn ? "user" : "model",
                    Parts = new List<Part> { new Part { Text = message } }
                });
                isUserTurn = !isUserTurn;
            }

            
            var apiRequest = new GoogleApiRequest
            {
                Contents = contents,
                GenerationConfig = new GenerationConfig
                {
                    // Clamp the temperature to the valid range for the API [0.0, 1.0]
                    Temperature = Math.Clamp(llmRequest.Temperature, 0.0, 1.0)
                }
            };

            if (!string.IsNullOrWhiteSpace(llmRequest.SystemPrompt))
            {
                // The Gemini API's system_instruction field expects a 'Content' object (which contains 'parts'), not a 'Part' object directly.
                apiRequest.SystemInstruction = new ContentItem
                {
                    Parts = new List<Part> { new Part { Text = llmRequest.SystemPrompt } }
                };
            }

            return apiRequest;
        }

        #region DTOs for Google API Serialization

        private class GoogleApiRequest
        {
            // FIX: Changed type from Part to ContentItem to match the API's expected structure.
            public ContentItem? SystemInstruction { get; set; }
            public List<ContentItem> Contents { get; set; } = new();
            public GenerationConfig? GenerationConfig { get; set; }
        }

        private class ContentItem
        {
            public string? Role { get; set; }
            public List<Part> Parts { get; set; } = new();
        }

        private class Part
        {
            public string? Text { get; set; }
        }

        private class GenerationConfig
        {
            public double Temperature { get; set; }
        }

        private class GoogleApiResponse
        {
            public List<Candidate>? Candidates { get; set; }
        }

        private class Candidate
        {
            public ContentItem? Content { get; set; }
        }

        #endregion
    }
}
