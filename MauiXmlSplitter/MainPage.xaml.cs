using System.Globalization;
using CommunityToolkit.Maui.Storage;

namespace MauiXmlSplitter
{
    /// <inheritdoc />
    public partial class MainPage : ContentPage
    {
        /// <inheritdoc />
        public MainPage()
        {
            InitializeComponent();
            CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture = new CultureInfo(
                Preferences.Default.Get(nameof(SettingsViewModel.Culture),
                    CultureInfo.CurrentCulture.TwoLetterISOLanguageName));
        }
    }
}
