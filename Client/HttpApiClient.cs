using LlmGateway.Configuration;
using LlmGateway.Exceptions;
using LlmGateway.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Net.Http.Headers;
using System.Text;
using System.Linq;

namespace LlmGateway.Client
{
    /// <summary>
    /// A generic, concrete implementation of <see cref="IApiClient"/> that uses <see cref="HttpClient"/>
    /// to communicate with LLM APIs over HTTP. It dynamically constructs and sends HTTP requests
    /// based on the provided configuration.
    /// </summary>
    public class HttpApiClient : IApiClient
    {
        /// <summary>
        /// A static, shared HttpClient instance is used for performance and proper socket management.
        /// </summary>
        private static readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Asynchronously sends a request to an LLM provider's API using HttpClient.
        /// This method builds the HTTP request, including headers and a JSON body, based on the
        /// provider and model configurations, sends it, and parses the response.
        /// </summary>
        /// <param name="providerConfig">The configuration of the target provider.</param>
        /// <param name="modelConfig">The configuration of the target model.</param>
        /// <param name="request">The standardized LlmRequest object.</param>
        /// <param name="apiKey">The API key for authentication.</param>
        /// <returns>A task that resolves to the string content of the LLM's response.</returns>
        /// <exception cref="LlmException">Thrown on API call failure.</exception>
        public async Task<string> SendRequestAsync(ProviderConfig providerConfig, ModelConfig modelConfig, LlmRequest request, string apiKey)
        {
            // 1. Construct the full request URI
            var requestUri = providerConfig.BaseUrl.TrimEnd('/') + "/" + modelConfig.Endpoint.TrimStart('/');

            // 2. Create a new HttpRequestMessage
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri);

            // 3. Add the authentication header
            var authHeaderValue = providerConfig.AuthHeaderValueTemplate.Replace("{ApiKey}", apiKey);
            var authHeaderParts = authHeaderValue.Split(new[] { ' ' }, 2);
            if (authHeaderParts.Length == 2)
            {
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue(authHeaderParts[0], authHeaderParts[1]);
            }
            else
            {
                httpRequest.Headers.Add(providerConfig.AuthHeaderName, authHeaderValue);
            }

            // 4. Dynamically build the JSON request body
            var rootNode = new JsonObject();

            // Add model name
           // SetValueByPath(rootNode, "model", modelConfig.ModelName);

            // Add system prompt if available and mapped
            if (!string.IsNullOrEmpty(request.SystemPrompt) && !string.IsNullOrEmpty(modelConfig.Mapping.SystemPromptPath))
            {
                SetValueByPath(rootNode, modelConfig.Mapping.SystemPromptPath, request.SystemPrompt);
            }

            // Add temperature
            SetValueByPath(rootNode, modelConfig.Mapping.TemperaturePath, request.Temperature);

            // Add messages
            var messagesArray = new JsonArray();
            foreach (var message in request.ConversationHistory)
            {
                var messageObject = new JsonObject();
                // Note: Enum.ToString() is used, but many APIs expect lowercase roles.
                SetValueByPath(messageObject, modelConfig.Mapping.MessageRolePath, message.Role.ToString().ToLowerInvariant());
                SetValueByPath(messageObject, modelConfig.Mapping.MessageContentPath, message.Content);
                messagesArray.Add(messageObject);
            }
            SetValueByPath(rootNode, modelConfig.Mapping.MessagesArrayPath, messagesArray);

            // 5. Serialize and create StringContent
            var jsonPayload = rootNode.ToJsonString();
            httpRequest.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {
                // 6. Send the request
                response = await _httpClient.SendAsync(httpRequest);
            }
            catch (HttpRequestException ex)
            {
                throw new LlmException(
                    "A network error occurred while sending the request to the provider.",
                    providerConfig.ProviderName,
                    request.ModelAlias,
                    null,
                    ex.Message,
                    ex);
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            // 7. Check response status
            if (!response.IsSuccessStatusCode)
            {
                // 8. Handle failure
                string providerErrorMessage = responseContent; // Default to raw content
                if (!string.IsNullOrEmpty(modelConfig.Mapping.ResponseErrorPath))
                {
                    try
                    {
                        var errorJson = JsonNode.Parse(responseContent);
                        var errorNode = GetValueByPath(errorJson, modelConfig.Mapping.ResponseErrorPath);
                        if (errorNode != null)
                        {
                            providerErrorMessage = errorNode.ToString();
                        }
                    }
                    catch (JsonException)
                    {
                        // Ignore if response is not valid JSON, raw content is already set
                    }
                }

                throw new LlmException(
                    $"API call failed with status code {response.StatusCode}.",
                    providerConfig.ProviderName,
                    request.ModelAlias,
                    response.StatusCode,
                    providerErrorMessage);
            }

            // 9. Handle success
            try
            {
                
                JsonNode? successJson = JsonNode.Parse(
                    responseContent);
                var contentNode = GetValueByPath(successJson, modelConfig.Mapping.ResponseContentPath);

                if (contentNode == null)
                {
                    throw new LlmException(
                        "Failed to extract content from the successful API response. Check the 'ResponseContentPath' mapping.",
                        providerConfig.ProviderName,
                        request.ModelAlias,
                        response.StatusCode,
                        "Content path not found in JSON response.");
                }

                // Use GetValue<string>() to unescape and get the raw string value
                return contentNode.GetValue<string>();
            }
            catch (JsonException ex)
            {
                throw new LlmException(
                    "Failed to parse the successful API response as JSON.",
                    providerConfig.ProviderName,
                    request.ModelAlias,
                    response.StatusCode,
                    responseContent,
                    ex);
            }
        }

        /// <summary>
        /// Sets a value within a JsonObject using a simple dot-separated path.
        /// Creates nested JsonObjects as needed.
        /// </summary>
        private static void SetValueByPath(JsonObject root, string path, JsonNode? value)
        {
            var segments = path.Split('.');
            var current = (JsonNode)root;

            // Process intermediate segments
            for (int i = 0; i < segments.Length - 1; i++)
            {
                string segment = segments[i];
                if (TryParseArraySegment(segment, out string? propertyName, out int index))
                {
                    // Handle array access (e.g., "parts[0]")
                    JsonArray array = GetOrCreateArray(current, propertyName);
                    EnsureArraySize(array, index);

                    if (array[index] == null)
                    {
                        // Create a new JSON object for the next segment
                        array[index] = new JsonObject();
                    }
                    current = array[index]!;
                }
                else
                {
                    // Handle simple property (e.g., "text")
                    if (current[segment] == null)
                    {
                        current[segment] = new JsonObject();
                    }
                    current = current[segment]!;
                }
            }

            // Process last segment
            string lastSegment = segments[segments.Length - 1];
            if (TryParseArraySegment(lastSegment, out string? lastProp, out int lastIndex))
            {
                JsonArray array = GetOrCreateArray(current, lastProp);
                EnsureArraySize(array, lastIndex);
                array[lastIndex] = value;
            }
            else
            {
                current[lastSegment] = value;
            }
        }

        // Helper to parse array segments (e.g., "parts[0]")
        private static bool TryParseArraySegment(string segment, out string? propertyName, out int index)
        {
            propertyName = null;
            index = -1;

            int openBracket = segment.IndexOf('[');
            int closeBracket = segment.IndexOf(']', openBracket + 1);

            // Validate brackets and index format
            if (openBracket < 1 || closeBracket != segment.Length - 1 || closeBracket <= openBracket + 1)
                return false;

            string indexPart = segment.Substring(openBracket + 1, closeBracket - openBracket - 1);
            if (!int.TryParse(indexPart, out index))
                return false;

            propertyName = segment.Substring(0, openBracket);
            return true;
        }

        // Get or create a JSON array at a property
        private static JsonArray GetOrCreateArray(JsonNode node, string propertyName)
        {
            if (node[propertyName] is JsonArray existingArray)
                return existingArray;

            var newArray = new JsonArray();
            node[propertyName] = newArray;
            return newArray;
        }

        // Ensure array has enough elements (fill with nulls if needed)
        private static void EnsureArraySize(JsonArray array, int index)
        {
            while (array.Count <= index)
            {
                array.Add(null);
            }
        }

        /// <summary>
        /// Retrieves a JsonNode from a root node using a simple dot-separated path.
        /// </summary>
        private static JsonNode? GetValueByPath(JsonNode root, string path)
        {
            var segments = path.Split('.');
            var current = root;

            foreach (var segment in segments)
            {
                if (current == null) return null;

                // Handle array index notation (e.g., "property[index]")
                if (segment.Contains('[') && segment.EndsWith(']'))
                {
                    int bracketIndex = segment.IndexOf('[');
                    string propertyName = segment.Substring(0, bracketIndex);
                    string indexPart = segment.Substring(bracketIndex + 1, segment.Length - bracketIndex - 2);

                    // Get property value
                    if (current is JsonObject obj && obj.TryGetPropertyValue(propertyName, out var value))
                    {
                        current = value;
                    }
                    else
                    {
                        return null;
                    }

                    // Handle array indexing
                    if (int.TryParse(indexPart, out int index))
                    {
                        if (current is JsonArray arr && index >= 0 && index < arr.Count)
                        {
                            current = arr[index];
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null; // Invalid index format
                    }
                }
                // Handle standalone array index (e.g., "[0]")
                else if (segment.StartsWith('[') && segment.EndsWith(']'))
                {
                    string indexPart = segment.Substring(1, segment.Length - 2);

                    if (int.TryParse(indexPart, out int index))
                    {
                        if (current is JsonArray arr && index >= 0 && index < arr.Count)
                        {
                            current = arr[index];
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null; // Invalid index format
                    }
                }
                // Handle standard properties
                else
                {
                    if (current is JsonObject obj)
                    {
                        if (obj.TryGetPropertyValue(segment, out var value))
                        {
                            current = value;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else if (current is JsonArray arr)
                    {
                        // Fallback for array index without brackets
                        if (int.TryParse(segment, out int index) && index >= 0 && index < arr.Count)
                        {
                            current = arr[index];
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null; // Can't traverse further
                    }
                }
            }

            return current;
        }
    }
}
