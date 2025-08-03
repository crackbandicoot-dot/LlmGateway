
  # Developer's Guide: Using the LlmApiLibrary
 
  ## What is this library?
 
  This library provides a single, unified interface to communicate with various Large Language Models (LLMs) from different providers (like OpenAI, Google, Anthropic, etc.).
  Its key feature is that you can add new models or even entirely new API providers by simply editing a JSON configuration file, without needing to recompile the project.
  It abstracts away the specific API request/response formats, authentication methods, and error structures of each provider.
 
  ## How It Works
 
  1.  **Configuration-Driven:** The entire system is controlled by a central `llm-config.json` file. In this file, you define each "Provider" (their base URL, auth method) and the "Models" they offer (their specific endpoint, aliases, and how to map our standard request to their specific JSON format).
  2.  **The `LlmClient`:** This is your main entry point. When you create an instance of `LlmClient`, you give it the path to your config file. It reads and processes the file, making all defined models ready for use.
  3.  **Making a Call:** When you call `GetChatCompletionAsync`, you refer to a model by one of its aliases (e.g., "gpt4"). The client looks up this alias in its configuration, finds the correct provider, endpoint, and API format, builds the provider-specific HTTP request, and sends it.
  4.  **Unified Response & Errors:** The client parses the provider's unique response and transforms it into a standard `LlmResponse` object. If anything goes wrong, it throws a standardized `LlmException`, which contains details about the error regardless of which provider it came from.
 
  ## How to Use It: A Quick Example
 
  **Step 1: Create your `llm-config.json` file.**
 
  ```json
  {
  "Providers": [
    {
      "ProviderName": "Google",
      "BaseUrl": "https://generativelanguage.googleapis.com",
      "AuthHeaderName": "x-goog-api-key",
      "AuthHeaderValueTemplate": "{ApiKey}",
      "Models": [
        {
          "ModelName": "gemini-1.5-flash-latest",
          "Aliases": [ "gemini-flash", "google-default" ],
          "Endpoint": "/v1beta/models/gemini-1.5-flash-latest:generateContent",
          "Mapping": {
            "SystemPromptPath": "systemInstruction.parts[0].text",
            "MessagesArrayPath": "contents",
            "MessageRolePath": "role",
            "MessageContentPath": "parts[0].text",
            "TemperaturePath": "generationConfig.temperature",
            "ResponseContentPath": "candidates[0].content.parts[0].text",
            "ResponseErrorPath": "error.message"
          }
        },
        {
          "ModelName": "gemini-1.5-pro-latest",
          "Aliases": [ "gemini-pro" ],
          "Endpoint": "/v1beta/models/gemini-1.5-pro-latest:generateContent",
          "Mapping": {
            "SystemPromptPath": "systemInstruction.parts[0].text",
            "MessagesArrayPath": "contents",
            "MessageRolePath": "role",
            "MessageContentPath": "parts[0].text",
            "TemperaturePath": "generationConfig.temperature",
            "ResponseContentPath": "candidates[0].content.parts[0].text",
            "ResponseErrorPath": "error.message"
          }
        }
      ]
    }
  ]
}
  ```
 
  **Step 2: Write your C# code.**
 
 ```csharp
 using LlmGateway;
using LlmGateway.Exceptions;
using LlmGateway.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            // 1. Initialize the client with the path to your config file 
            //    (this file should now contain the Google/Gemini provider configuration).
            var llmClient = new LlmClient("route\to\your\config\llm-config.json");

            // 2. Set the API key for the Google provider.
            // The provider name "Google" must match the "ProviderName" in your JSON.
            llmClient.SetApiKey("Google", "YOUR_GOOGLE_API_KEY");

            // 3. Define your conversation history (optional).
            // Note: The Gemini API has specific requirements for conversation history roles.
            // It expects an alternating sequence of 'user' and 'model' roles.
            var history = new List<ChatMessage>
              {
                  new ChatMessage(MessageRole.User, "What is the capital of Spain?"),
                  // The assistant's response role must be 'Assistant' for our library, 
                  // which will be mapped to 'model' for the Gemini API.
                  new ChatMessage(MessageRole.Assistant, "The capital of Spain is Madrid.")
              };

            // 4. Call the model using its alias (e.g., "gemini-flash").
            Console.WriteLine("Sending prompt to Gemini model...");
            LlmResponse response = await llmClient.GetChatCompletionAsync(
                modelAlias: "gemini-flash",
                userPrompt: "What is a fun fact about it?",
                conversationHistory: history,
                systemPrompt: "You are a knowledgeable and friendly tour guide."
            );

            // 5. Use the response.
            Console.WriteLine($"\nModel Response: {response.Content}");
        }
        catch (LlmException ex)
        {
            // Handle any API or configuration errors in a unified way.
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.WriteLine($"Provider: {ex.ProviderName}, Status Code: {ex.StatusCode}");
            Console.WriteLine($"Provider Error: {ex.ProviderErrorMessage}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }
    }
}
  ```
 