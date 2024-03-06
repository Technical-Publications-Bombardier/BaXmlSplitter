using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

// ReSharper disable UnusedMember.Global

namespace TechPubsDatabase.Models
{
    /// <summary>
    /// The CSDB db context.
    /// </summary>
    public static class CsdbContext
    {
        /// <summary>The CSDB programs.</summary>
        internal static readonly string[] Programs = Enum.GetNames<CsdbProgram>();
        /// <summary>
        /// The CSDB Program.
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum CsdbProgram
        {
            /// <summary>
            /// No CSDB program selected
            /// </summary>
            None,
            /// <summary>
            /// The <c>B_IFM</c> program for instrument flight manuals (all models)
            /// </summary>
            B_IFM,
            /// <summary>
            /// The <c>CH604PROD</c> program for Challenger 6XX maintenance manuals
            /// </summary>
            CH604PROD,
            /// <summary>
            /// The <c>CTALPROD</c> program for Challenger 3XX maintenance manuals
            /// </summary>
            CTALPROD,
            /// <summary>
            /// The <c>GXPROD</c> program for Global and Global Express maintenance manuals
            /// </summary>
            GXPROD,
            /// <summary>
            /// The <c>LJ4045PROD</c> program for Learjet 40/45 maintenance manuals
            /// </summary>
            LJ4045PROD
        };
    }
    /// <summary>
    /// Deserializer for <see cref="CsdbContext.CsdbProgram"/>
    /// </summary>
    /// <seealso cref="CsdbContext.CsdbProgram" />
    public class CsdbProgramConverter : JsonConverter<CsdbContext.CsdbProgram>
    {
        /// <inheritdoc />
        public override CsdbContext.CsdbProgram Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Read the string value from the JSON
            var value = reader.GetString();

            // Try to parse the string value to the enum value
            if (Enum.TryParse<CsdbContext.CsdbProgram>(value, out var result))
            {
                return result;
            }

            // If parsing fails, throw an exception or return a default value
            throw new JsonException($"Invalid value for CsdbProgram: {value}");
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, CsdbContext.CsdbProgram value, JsonSerializerOptions options)
        {
            // Write the enum value as a string to the JSON
            writer.WriteStringValue(value.ToString());
        }

        // Override the WriteAsPropertyName method
        /// <inheritdoc />
        public override void WriteAsPropertyName(Utf8JsonWriter writer, CsdbContext.CsdbProgram value, JsonSerializerOptions options)
        {
            // Write the enum value as a string property name to the JSON
            writer.WritePropertyName(value.ToString());
        }

        // Override the ReadAsPropertyName method
        /// <inheritdoc />
        public override CsdbContext.CsdbProgram ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Read the string property name from the JSON
            var value = reader.GetString();

            // Try to parse the string value to the enum value
            if (Enum.TryParse<CsdbContext.CsdbProgram>(value, out var result))
            {
                return result;
            }

            // If parsing fails, throw an exception or return a default value
            throw new JsonException($"Invalid value for CsdbProgram: {value}");
        }
    }
}