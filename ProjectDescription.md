

  # Project Architecture Summary
 
  This library provides a unified, extensible interface for interacting with various Large Language Model (LLM) APIs.
 
  ## Components and Types:
 
  1.  **Core Communication Component:** The primary entry point for users.
      *   `LlmInteraction`: A high-level class that orchestrates requests. It takes a provider,a model name and chat parameters,
        and returns the LLM's response. It handles locating the correct model and delegating the
        API call.Is inspired in *factory pattern*.
 
  2.  **Providers:** Manages loading and providing access to LLM provider.
      *   `IProvider`: A contract defined  for get a response from a provider given a request.Using the standarized Data Models (e.g `LlmRequest`, `LlmResponse`)
 
  4.  **Data Models (DTOs):** Standardized data structures used throughout the library.
      *   `LlmRequest`: An internal data structure encapsulating all parameters for an LLM request.
      *   `LlmResponse`: An internal data structure for the standardized response from an LLM.
 
  5.  **Exception Handling Component:** Provides a unified error-handling mechanism.
      *   `LlmException`: A custom, provider-agnostic exception thrown on API errors, containing details like the provider name,
        status code, and the original error message.
 
  ## Workflow:
  User -> LlmInteraction -> Providers ->  LLM API -> Response -> User
 
 

