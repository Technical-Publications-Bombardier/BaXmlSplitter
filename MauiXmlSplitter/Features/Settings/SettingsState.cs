using Fluxor;
namespace MauiXmlSplitter
{
    /// <summary>
    /// State for settings.
    /// </summary>
    /// <seealso cref="SettingsState" />
    [FeatureState]
    public record SettingsState
    {
        /// <summary>
        /// Gets the language.
        /// </summary>
        /// <value>
        /// The language.
        /// </value>
        public string Language { get; init; }
        /// <summary>
        /// Gets the provisional language.
        /// </summary>
        /// <value>
        /// The provisional language.
        /// </value>
        public string ProvisionalLanguage { get; init; }
        /// <summary>
        /// Gets the secret.
        /// </summary>
        /// <value>
        /// The secret.
        /// </value>
        public string Secret { get; init; }
        /// <summary>
        /// Gets the provisional secret.
        /// </summary>
        /// <value>
        /// The provisional secret.
        /// </value>
        public string ProvisionalSecret { get; init; }
    }

}
