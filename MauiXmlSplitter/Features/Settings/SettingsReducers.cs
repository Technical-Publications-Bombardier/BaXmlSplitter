using Fluxor;

namespace MauiXmlSplitter
{
    /// <summary>
    /// Reducers for the settings
    /// </summary>
    public class SettingsReducers
    {
        /// <summary>
        /// Reduces the set provisional language action.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        [ReducerMethod]
        public static SettingsState ReduceSetProvisionalLanguageAction(SettingsState state, SettingsActions.SetProvisionalLanguage action)
            => state with { ProvisionalLanguage = action.Language };

        /// <summary>
        /// Reduces the commit language action.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        [ReducerMethod]
        public static SettingsState ReduceCommitLanguageAction(SettingsState state, SettingsActions.CommitLanguage action)
            => state with { Language = state.ProvisionalLanguage };

        /// <summary>
        /// Reduces the set provisional secret action.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        [ReducerMethod]
        public static SettingsState ReduceSetProvisionalSecretAction(SettingsState state, SettingsActions.SetProvisionalSecret action)
            => state with { ProvisionalSecret = action.Secret };

        /// <summary>
        /// Reduces the commit secret action.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        [ReducerMethod]
        public static SettingsState ReduceCommitSecretAction(SettingsState state, SettingsActions.CommitSecret action)
            => state with { Secret = state.ProvisionalSecret };
    }

}
