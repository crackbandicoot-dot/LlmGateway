using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace LlmGateway.Configuration
{
    /// <summary>
    /// Manages loading, parsing, and providing access to the LLM configuration from a JSON file.
    /// It creates an in-memory representation of the provider and model settings and allows for
    /// efficient lookups based on model aliases.
    /// </summary>
    public class ConfigurationManager
    {
        /// <summary>
        /// The fully loaded and parsed configuration containing all providers and models.
        /// </summary>
        private  LlmConfiguration _configuration;

        /// <summary>
        /// A pre-processed dictionary that maps a model alias directly to its provider and model configuration
        /// for fast O(1) lookups. The key is the alias (string), and the value is a tuple containing
        /// the corresponding ProviderConfig and ModelConfig.
        /// </summary>
        private readonly Dictionary<string, (ProviderConfig Provider, ModelConfig Model)> _modelAliasMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationManager"/> class by loading and processing
        /// the configuration from the specified file path.
        /// </summary>
        /// <param name="configFilePath">The path to the JSON configuration file.</param>
        /// <exception cref="System.IO.FileNotFoundException">Thrown if the configuration file is not found.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the JSON is malformed, contains duplicate aliases, or is otherwise invalid.</exception>
        public ConfigurationManager(string configFilePath)
        {
            _modelAliasMap = new Dictionary<string, (ProviderConfig Provider, ModelConfig Model)>();
            LoadAndProcessConfiguration(configFilePath);
        }

        /// <summary>
        /// Attempts to retrieve the provider and model configuration associated with the specified model alias.
        /// </summary>
        /// <param name="alias">The alias of the model to find.</param>
        /// <param name="providerConfig">When this method returns, contains the ProviderConfig for the model if the alias is found; otherwise, null.</param>
        /// <param name="modelConfig">When this method returns, contains the ModelConfig for the model if the alias is found; otherwise, null.</param>
        /// <returns>true if a model with the specified alias is found; otherwise, false.</returns>
        public bool TryGetModelConfigByAlias(string alias, out ProviderConfig? providerConfig, out ModelConfig? modelConfig)
        {
            if (_modelAliasMap.TryGetValue(alias, out var config))
            {
                providerConfig = config.Provider;
                modelConfig = config.Model;
                return true;
            }

            providerConfig = null;
            modelConfig = null;
            return false;
        }

        /// <summary>
        /// Loads the configuration from the specified JSON file, deserializes it, and builds the alias map.
        /// This method is called by the constructor.
        /// </summary>
        /// <param name="configFilePath">The path to the JSON configuration file.</param>
        private void LoadAndProcessConfiguration(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                throw new FileNotFoundException("Configuration file not found.", configFilePath);
            }

            string jsonContent = File.ReadAllText(configFilePath);

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                _configuration = JsonSerializer.Deserialize<LlmConfiguration>(jsonContent, options)
                                 ?? throw new InvalidOperationException("JSON deserialization resulted in a null object.");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to parse the JSON configuration file. Check for malformed JSON.", ex);
            }

            if (_configuration.Providers == null)
            {
                throw new InvalidOperationException("The 'Providers' array in the configuration is missing or null.");
            }

            foreach (var provider in _configuration.Providers)
            {
                if (provider.Models == null) continue;

                foreach (var model in provider.Models)
                {
                    if (model.Aliases == null) continue;

                    foreach (var alias in model.Aliases)
                    {
                        if (string.IsNullOrWhiteSpace(alias)) continue;

                        if (_modelAliasMap.ContainsKey(alias))
                        {
                            throw new InvalidOperationException($"Duplicate model alias found in configuration: '{alias}'. Aliases must be unique across all providers.");
                        }
                        _modelAliasMap.Add(alias, (provider, model));
                    }
                }
            }
        }
    }
}
