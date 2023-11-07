using System.Collections;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using static MauiXmlSplitter.Models.CsdbContext;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("MauiXmlSplitter.Tests")]

namespace MauiXmlSplitter.Models
{
    /// <summary>
    /// Unit-of-work states from ETPS. They are relative to the <see cref="CsdbContext.CsdbProgram"/>.
    /// </summary>
    /// <seealso cref="IUowState" />
    /// <seealso cref="IUowState" />
    /// <seealso cref="IUowState" />
    /// <seealso cref="UowState" />
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public partial record UowState(int? StateValue = null, string? StateName = null, string? Remark = null,
            string? XPath = null, string? TagName = null, string? Key = null, string? Resource = null,
            string? Title = null, string? Level = null)
        : IUowState
    {


        /// <summary>The UOW states file parsing regular expression pattern.</summary>
        [GeneratedRegex("""(?<tabs>\t*)(?:Front Matter: )?(?<tag>\S+)(?: (?<key>\S+))?(?: (?<rs>RS-\d+))?(?: - (?<title>.+?))?(?: (?<lvl>[A-Z0-9 =]+?))? +-- .*?\(state = "(?<state>[^"]*)"\)$""")]
        private static partial Regex UowPattern();

        /// <summary>
        /// Gets or sets the x path.
        /// </summary>
        /// <value>
        /// The x path.
        /// </value>
        public string? XPath { get; set; } = XPath;
        /// <summary>
        /// Gets or sets the name of the tag.
        /// </summary>
        /// <value>
        /// The name of the tag.
        /// </value>
        public string? TagName { get; set; } = TagName;
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string? Key { get; set; } = Key;
        /// <summary>
        /// Gets or sets the resource.
        /// </summary>
        /// <value>
        /// The resource.
        /// </value>
        public string? Resource { get; set; } = Resource;
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string? Title { get; set; } = Title;
        /// <summary>
        /// Gets or sets the level.
        /// </summary>
        /// <value>
        /// The level.
        /// </value>
        public string? Level { get; set; } = Level;
        /// <summary>
        /// Gets or sets the name of the state.
        /// </summary>
        /// <value>
        /// The name of the state.
        /// </value>
        // ReSharper disable once StringLiteralTypo
        [JsonPropertyName("statename")]
        public string? StateName { get; set; } = StateName;
        /// <summary>
        /// Gets or sets the state value.
        /// </summary>
        /// <value>
        /// The state value.
        /// </value>
        public int? StateValue { get; set; } = StateValue;
        /// <summary>
        /// Gets or sets the remark.
        /// </summary>
        /// <value>
        /// The remark.
        /// </value>
        [JsonPropertyName("remark")]
        public string? Remark { get; set; } = Remark;

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Join(", ", new ArrayList() { StateValue is null ? null : $"StateValue={StateValue}", string.IsNullOrEmpty(StateName) ? null : $"StateName='{StateName}'", string.IsNullOrEmpty(Remark) ? null : $"Remark='{Remark}'", string.IsNullOrEmpty(XPath) ? null : $"XPath='{XPath}'", string.IsNullOrEmpty(TagName) ? null : $"TagName='{TagName}'", string.IsNullOrEmpty(Key) ? null : $"Key='{Key}'", string.IsNullOrEmpty(Resource) ? null : $"Resource='{Resource}'", string.IsNullOrEmpty(Title) ? null : $"Title='{Title}'", string.IsNullOrEmpty(Level) ? null : $"Level='{Level}'" }.Cast<string>().Where(field => !string.IsNullOrEmpty(field)).ToArray());
        }

        /// <summary>
        /// Gets the debugger display.
        /// </summary>
        /// <returns>The string representation of the unit-of-work state.</returns>
        private string GetDebuggerDisplay()
        {
            return ToString();
        }

        /// <summary>
        /// Two unit-of-work states are equivalent if they have the same <c>StateValue</c>.
        /// </summary>
        /// <param name="other">The other unit-of-work state.</param>
        /// <returns><c>true</c> if the <paramref name="other"/> unit-of-work state value (<see cref="Int32"/>) is the same as this unit-of-work's state value.</returns>
        public bool Equals(IUowState? other)
        {
            return other is not null && StateValue == other.StateValue;
        }

        /// <summary>
        /// Two unit-of-work states are equivalent if they have the same <c>StateValue</c>.
        /// </summary>
        /// <param name="x">The first unit-of-work state.</param>
        /// <param name="y">The second unit-of-work state.</param>
        /// <returns><c>true</c> if the <paramref name="x"/> unit-of-work state value (<see cref="Int32"/>) is the same as <paramref name="y"/>'s unit-of-work state value.</returns>
        public bool Equals(IUowState? x, IUowState? y)
        {
            return x is not null && y is not null && x.StateValue == y.StateValue;
        }

        /// <summary>
        /// Gets the hash code for the unit-of-work state by way of its <c>StateValue</c> hash code.
        /// </summary>
        /// <param name="obj">The unit-of-work state object.</param>
        /// <returns>The hash code of the <c>StateValue</c>.</returns>
        public int GetHashCode(IUowState obj)
        {
            return obj.StateValue.GetHashCode();
        }

    }
    /// <summary>
    /// Defines the structure of the unit-of-work state.
    /// </summary>
    /// <seealso cref="IEquatable&lt;IUowState&gt;" />
    /// <seealso cref="IEqualityComparer&lt;IUowState&gt;" />
    public interface IUowState : IEquatable<IUowState>, IEqualityComparer<IUowState>
    {
        /// <summary>
        /// Gets or sets the state value.
        /// </summary>
        /// <value>
        /// The state value.
        /// </value>
        [JsonPropertyName("value")]
        int? StateValue { get; set; }
        /// <summary>
        /// Gets or sets the name of the state.
        /// </summary>
        /// <value>
        /// The name of the state.
        /// </value>
        [JsonPropertyName("stateName")]
        string? StateName { get; set; }
        /// <summary>
        /// Gets or sets the remark.
        /// </summary>
        /// <value>
        /// The remark.
        /// </value>
        [JsonPropertyName("remark")]
        string? Remark { get; set; }
        /// <summary>
        /// Gets or sets the x path.
        /// </summary>
        /// <value>
        /// The x path.
        /// </value>
        string? XPath { get; set; }
        /// <summary>
        /// Gets or sets the name of the tag.
        /// </summary>
        /// <value>
        /// The name of the tag.
        /// </value>
        string? TagName { get; set; }
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        string? Key { get; set; }
        /// <summary>
        /// Gets or sets the resource.
        /// </summary>
        /// <value>
        /// The resource.
        /// </value>
        string? Resource { get; set; }
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        string? Title { get; set; }
        /// <summary>
        /// Gets or sets the level.
        /// </summary>
        /// <value>
        /// The level.
        /// </value>
        string? Level { get; set; }
    }
    public class UowStateConverter : JsonConverter<UowState>
    {
        public override UowState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Read the JSON object
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected start object token");
            }

            // Create a new UowState instance
            var uowState = new UowState();

            // Read the properties of the JSON object
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("Expected property name token");
                }

                // Get the property name
                var propertyName = reader.GetString();

                // Read the property value
                reader.Read();

                // Set the corresponding property of the UowState instance
                switch (propertyName)
                {
                    case "statename":
                        uowState.StateName = reader.GetString();
                        break;
                    case "value":
                        uowState.StateValue = reader.GetInt32();
                        break;
                    case "remark":
                        uowState.Remark = reader.GetString();
                        break;
                    case "xpath":
                        uowState.XPath = reader.GetString();
                        break;
                    case "tagname":
                        uowState.TagName = reader.GetString();
                        break;
                    case "key":
                        uowState.Key = reader.GetString();
                        break;
                    case "resource":
                        uowState.Resource = reader.GetString();
                        break;
                    case "title":
                        uowState.Title = reader.GetString();
                        break;
                    case "level":
                        uowState.Level = reader.GetString();
                        break;
                    default:
                        throw new JsonException($"Unknown property: {propertyName}");
                }
            }

            // Return the UowState instance
            return uowState;
        }

        public override void Write(Utf8JsonWriter writer, UowState value, JsonSerializerOptions options)
        {
            // Write the start object token
            writer.WriteStartObject();

            // Write the properties of the UowState instance
            writer.WriteString("statename", value.StateName);
            writer.WriteNumber("value", value.StateValue ?? 0);
            writer.WriteString("remark", value.Remark);
            writer.WriteString("xpath", value.XPath);
            writer.WriteString("tagname", value.TagName);
            writer.WriteString("key", value.Key);
            writer.WriteString("resource", value.Resource);
            writer.WriteString("title", value.Title);
            writer.WriteString("level", value.Level);

            // Write the end object token
            writer.WriteEndObject();
        }
    }

}
