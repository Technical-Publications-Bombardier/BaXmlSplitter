namespace MauiXmlSplitter
{
    /// <summary>
    /// Actions for the settings
    /// </summary>
    public static class SettingsActions
    {
        /// <summary>
        /// Provisionally set language
        /// </summary>
        public record SetProvisionalLanguage(string Language);
        /// <summary>
        /// Commit the language choice.
        /// </summary>
        public record CommitLanguage();
        /// <summary>
        /// Provisionally set the secret.
        /// </summary>
        public record SetProvisionalSecret(string Secret);
        /// <summary>
        /// Commit the secret.
        /// </summary>
        public record CommitSecret();
    }

}
