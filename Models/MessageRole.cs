namespace LlmGateway.Models
{
    /// <summary>
    /// Specifies the role of the author of a message in a chat conversation.
    /// This is used to differentiate between system-level instructions, user input,
    /// and model-generated responses.
    /// </summary>
    public enum MessageRole
    {
        /// <summary>
        /// The message provides system-level instructions or context to the model.
        /// </summary>
        System,

        /// <summary>
        /// The message is from the end-user.
        /// </summary>
        User,

        /// <summary>
        /// The message is a response from the language model (the assistant).
        /// </summary>
        Assistant
    }
}
