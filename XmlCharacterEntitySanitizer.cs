using Microsoft.Extensions.Logging;
using System.Management.Automation;
using System.Text;

namespace BaXmlSplitter
{
    internal class XmlCharacterEntitySanitizer
    {
        internal const string ValidCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789~#`\t*';&@_\\,!=:\"<>/+- ?.{}()[]àâçéèêëîïôùûüÿñÀÂÄÇÉÈÊËÎÏÔÙÛÜŸÑ";
        /// <summary>
        /// The character entities indexed by their <see cref="Int32"/> ASCII code-point.
        /// <code language="powershell">
        /// Select-String -Path "$env:OneDriveCommercial\specialCharactersSanitizer\xmlEntitySanitizer\app.properties" `
        /// -Pattern "ASCII_(?&lt;ascii&gt;\d+)[\s=]+(?&lt;entity&gt;\S+)" | Select-Object -ExpandProperty Matches | `
        /// Where-Object { $_.Groups['entity'].Success } | ForEach-Object -Begin { $dict = `
        /// [System.Collections.Generic.Dictionary[int,string]]::new() } -Process { `
        /// $dict.Add([int]::Parse($_.Groups['ascii'].Value),$_.Groups['entity'].Value) } -End { $dict } | `
        /// Export-Clixml -Depth 99 -Path ".\BaXmlSplitter\Resources\LookupEntities.xml"
        /// </code>
        /// </summary>
        internal readonly Dictionary<int, string> LookupEntities = [];

        internal XmlCharacterEntitySanitizer()
        {
            dynamic psLookupEntities = PSSerializer.Deserialize(Properties.Resources.LookupEntities);
            foreach (var key in psLookupEntities.Keys)
            {
                LookupEntities.Add(key, psLookupEntities[key]);
            }
        }

        /// <summary>
        /// Sanitizes the specified XML stream.
        /// </summary>
        /// <param name="xmlStream">The XML stream.</param>
        /// <returns></returns>
        internal Stream Sanitize(in Stream xmlStream, ILogger logger)
        {
            var outputStream = new MemoryStream();
            using var writer = new StreamWriter(outputStream, leaveOpen: true);
            using var reader = new StreamReader(xmlStream);

            while (reader.Peek() >= 0)
            {
                var c = (char)reader.Read();
                if (ValidCharacters.Contains(c, StringComparison.OrdinalIgnoreCase))
                {
                    writer.Write(c);
                }
                else if (LookupEntities.TryGetValue((int)c, out var entity))
                {
                    writer.Write(entity);
                }
                else
                {
                    Log.UnrecognizedXmlCharacterEntity(logger, c, null);
                }
            }

            writer.Flush();
            outputStream.Position = 0;
            return outputStream;
        }

        private static class Log
        {
            public static readonly Action<ILogger, char, Exception?> UnrecognizedXmlCharacterEntity =
                LoggerMessage.Define<char>(LogLevel.Warning, new EventId(0, nameof(UnrecognizedXmlCharacterEntity)), "Unrecognized XML character entity: {Char}");
        }
    }
}
