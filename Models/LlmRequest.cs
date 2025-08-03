using System.Collections.Generic;
using System.Linq;

namespace LlmGateway.Models
{
    /// <summary>
    /// Represents a standardized, internal request to be sent to an LLM.
    /// This object decouples the high-level client from the low-level API client,
    /// containing all necessary information for an API call in a provider-agnostic format.
    /// </summary>
    public class LlmRequest
    {
        /// <summary>
        /// Gets the alias of the model to which the request is targeted.
        /// </summary>
        public string ModelAlias { get; }

        /// <summary>
        /// Gets the system prompt or instructions for the model. Can be null or empty.
        /// </summary>
        public string? SystemPrompt { get; }

        /// <summary>
        /// Gets the list of messages forming the conversation history.
        /// </summary>
        public IReadOnlyList<ChatMessage> ConversationHistory { get; }

        /// <summary>
        /// Gets the temperature setting for the request, controlling the randomness of the output.
        /// </summary>
        public double Temperature { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LlmRequest"/> class.
        /// </summary>
        /// <param name="modelAlias">The alias of the target model.</param>
        /// <param name="systemPrompt">The system-level instructions for the model.</param>
        /// <param name="conversationHistory">The history of the conversation.</param>
        /// <param name="temperature">The temperature for the model's response generation.</param>
        public LlmRequest(string modelAlias, string? systemPrompt, IEnumerable<ChatMessage> conversationHistory, double temperature)
        {
            ModelAlias = modelAlias;
            SystemPrompt = systemPrompt;
            ConversationHistory = conversationHistory.ToList().AsReadOnly();
            Temperature = temperature;
        }
    }
}
