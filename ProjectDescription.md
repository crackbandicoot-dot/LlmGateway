

  # Project Architecture Summary
 
  This library provides a unified, extensible interface for interacting with various Large Language Model (LLM) APIs.
  Its core design principle is configuration-driven extensibility, allowing the addition of new LLM providers and models
  via a JSON file without modifying the source code.
 
  ## Components and Types:
 
  1.  **Core Communication Component:** The primary entry point for users.
      *   `LlmClient`: A high-level client that orchestrates requests. It takes a model alias and chat parameters,
        and returns the LLM's response. It handles locating the correct model configuration and delegating the
        API call.
 
  2.  **Configuration Component:** Manages loading and providing access to LLM provider/model settings from a JSON file.
      *   `ConfigurationManager`: A class responsible for parsing the configuration file and providing model-specific
        details based on a given alias.
      *   `LlmConfiguration`: Root object for the entire JSON configuration, holding a list of providers.
      *   `ProviderConfig`: Represents an API provider (e.g., OpenAI, Google), containing its base URL and model definitions.
      *   `ModelConfig`: Defines a specific model, its API endpoint, aliases, and crucially, the mappings for request/response fields.
      *   `RequestResponseMapping`: A data structure holding JSON path mappings to adapt the library's standard request/response
        to the specific format required by a provider's API.
 
  3.  **API Client Component:** Handles the low-level HTTP communication.
      *   `IApiClient`: An interface defining the contract for sending a request to an LLM API and receiving a standardized response.
      *   `HttpApiClient`: A generic, concrete implementation of `IApiClient`. It uses `HttpClient` to dynamically construct
        and send HTTP requests based on the `ModelConfig` provided for a given model.
 
  4.  **Data Models (DTOs):** Standardized data structures used throughout the library.
      *   `ChatMessage`: Represents a single message in a conversation, with a role and content.
      *   `MessageRole`: An enum (`System`, `User`, `Assistant`) to specify the author of a `ChatMessage`.
      *   `LlmRequest`: An internal data structure encapsulating all parameters for an LLM request.
      *   `LlmResponse`: An internal data structure for the standardized response from an LLM.
 
  5.  **Exception Handling Component:** Provides a unified error-handling mechanism.
      *   `LlmException`: A custom, provider-agnostic exception thrown on API errors, containing details like the provider name,
        status code, and the original error message.
 
  ## Workflow:
  User -> LlmClient -> ConfigurationManager -> IApiClient -> (HTTP) -> LLM API
 
 

