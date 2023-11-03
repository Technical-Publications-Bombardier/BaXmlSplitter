using MauiXmlSplitter.Models;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Maui.Storage;

namespace MauiXmlSplitter
{
    public class XmlSplitterViewModel : INotifyPropertyChanged
    {

        /// <summary>
        /// The XML file type
        /// </summary>
        private static readonly FilePickerFileType XmlType = new(
            new Dictionary<DevicePlatform, IEnumerable<string>> {
                { DevicePlatform.iOS, new[] { "public.xml" } },
                { DevicePlatform.Android, new[] { "application/xml" } },
                { DevicePlatform.WinUI, new[] { ".xml" } },
                { DevicePlatform.MacCatalyst, new[] { "public.xml" } },
                { DevicePlatform.watchOS, new[] { "public.xml" } },
                { DevicePlatform.tvOS, new[] { "public.xml" } },
                { DevicePlatform.macOS, new[] { "public.xml" } },
                { DevicePlatform.Tizen, new[] { "*/*" } }
            });
        /// <summary>
        /// The units-of-work states file type (plaintext)
        /// </summary>
        private static readonly FilePickerFileType UowType = new(
            new Dictionary<DevicePlatform, IEnumerable<string>> {
                { DevicePlatform.iOS, new[] { "public.text" } },
                { DevicePlatform.Android, new[] { "text/plain" } },
                { DevicePlatform.WinUI, new[] { ".txt" } },
                { DevicePlatform.MacCatalyst, new[] { "public.text" } },
                { DevicePlatform.watchOS, new[] { "public.text" } },
                { DevicePlatform.tvOS, new[] { "public.text" } },
                { DevicePlatform.macOS, new[] { "public.text" } },
                { DevicePlatform.Tizen, new[] { "*/*" } }
            });


        public async void PickXmlFile(object sender, EventArgs e)
        {
            PickOptions options = new()
            {
                FileTypes = XmlType,
                PickerTitle = "Select XML File",
            };
            try
            {
                var result = await FilePicker.Default.PickAsync(options);
            }
            catch (OperationCanceledException)
            {

            }
        }

        public async void PickUowStatesFile(object sender, EventArgs e)
        {
            PickOptions options = new()
            {
                FileTypes = UowType,
                PickerTitle = "Select UOW File",
            };
            try
            {
                var result = await FilePicker.Default.PickAsync(options);
            }
            catch (OperationCanceledException)
            {

            }
        }
        public async void PickOutputFolder(object sender, EventArgs e)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2.0));
            var result = await FolderPicker.Default.PickAsync(cts.Token);
        }

        private readonly XmlSplitter xmlSplitter = new();
        public async void SplitXmlCommand(object sender, EventArgs e)
        {
            xmlSplitter.ExecuteSplit(sender, e);
        }


        public string SourceXml
        {
            get => xmlSplitter.XmlSourceFile; set
            {
                xmlSplitter.XmlSourceFile = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SourceXml)));
            }
        }

        public string UowStatesFile
        {
            get => xmlSplitter.UowStatesFile; set
            {
                xmlSplitter.UowStatesFile = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UowStatesFile)));
            }
        }

        public string OutputDirectory
        {
            get => xmlSplitter.OutputDirectory; set
            {
                xmlSplitter.OutputDirectory = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OutputDirectory)));
            }
        }

        public string XPath
        {
            get => xmlSplitter.XPath; set
            {
                xmlSplitter.XPath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(XPath)));
            }
        }



        public event PropertyChangedEventHandler? PropertyChanged;

        public async Task InitializeAsync()  // Where should I be calling this?
        {
            await xmlSplitter.LoadAssets();
        }
    }
}