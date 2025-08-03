
using System.Collections.Generic;

namespace LlmGateway.Configuration
{
    /// <summary>
    /// Represents the root configuration object that holds all provider settings.
    /// This class is the top-level object when deserializing the main JSON configuration file.
    /// </summary>
    public class LlmConfiguration
    {
        /// <summary>
        /// Gets or sets the list of all configured LLM providers.
        /// </summary>
        public List<ProviderConfig> Providers { get; set; } = new List<ProviderConfig>();
    }
}
