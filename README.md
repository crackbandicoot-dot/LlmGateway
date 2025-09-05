
  # Developer's Guide: Using the LlmGateway Library
 
  ## What is this library?
 
  This library provides a single, unified interface to communicate with various Large Language Models (LLMs) from different providers (like Google, OpenRouter).
  Its key feature is that you can interact with this API's new models or even entirely new API providers whith a Standarized Request and Response Formats.
  It abstracts away the specific API request/response formats, authentication methods.
 
  ## How It Works 
  1.  **The `LlmClient`:** This is your main entry point. When you create an instance of `LlmInteraction`, it creates a reausable chat with a LLM.
  2.  **Making a Call:** When you call `GetChatCompletionAsync`, you refer to a model by its name on the API  (e.g., "gemini-2.5-pro"), and the provider name (currently supported are Google and OpenRouter). A *HTTP* conection with the provider is created and the request is translated to the Provider's API syntax.
  3.  **Unified Response** The client parses the provider's unique response and transforms it into a standard `LlmResponse` object. 
 
  ## How to Use It: A Quick Example
 
  **Step 1: Configure your API Keys as *Enviorment Variables*, the name of the API Key should be the same of the provider's name.**
  - [How to configure Enviorment Variables on Windows.](https://www.howtogeek.com/787217/how-to-edit-environment-variables-on-windows-10-or-11/)
  - [How to configure Enviorment Variables on Ubuntu.](https://itslinuxfoss.com/set-environment-variable-ubuntu-24-04/)
 
  **Step 2:Download and copy LlmGateway.dll into your C# project.**
  
  **Step 3: Write your C# code.**

  ```csharp
using LlmGateway;
using LlmGateway.Models;
namespace TestingProject
{
    class Program
    {

        public static async Task Main()
        {
            var deepseekInteraction = new LlmInteraction();
            var deepseekAnswer = await deepseekInteraction.GetChatCompletionAsync(
                "OpenRouter",
                new LlmRequest("deepseek/deepseek-r1:free",
                "You are a usefull assistant",
                ["Hello, hey there"],
                0.5)
            );
            Console.WriteLine(deepseekAnswer.Content);
        }

    }
}
  ```

 
