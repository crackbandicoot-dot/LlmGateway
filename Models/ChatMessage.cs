namespace LlmGateway.Models
{
    /// <summary>
    /// Represents a single message within a chat conversation. It includes the role of the message
    /// author and the text content of the message.
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// Gets or sets the role of the message author (e.g., System, User, or Assistant).
        /// </summary>
        public MessageRole Role { get; set; }

        /// <summary>
        /// Gets or sets the text content of the message.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatMessage"/> class.
        /// </summary>
        /// <param name="role">The role of the message author.</param>
        /// <param name="content">The content of the message.</param>
        public ChatMessage(MessageRole role, string content)
        {
            Role = role;
            Content = content;
        }
    }
}
