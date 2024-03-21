#if ANDROID
using Android.Runtime;
#endif
using Foundation;

namespace MauiXmlSplitter
{
    /// <inheritdoc />
    [Register("AppDelegate")]
    public partial class AppDelegate : MauiUIApplicationDelegate
    {
        /// <inheritdoc />
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
