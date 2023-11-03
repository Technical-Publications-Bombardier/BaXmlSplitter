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
        int? StateValue { get; set; }
        /// <summary>
        /// Gets or sets the name of the state.
        /// </summary>
        /// <value>
        /// The name of the state.
        /// </value>
        string? StateName { get; set; }
        /// <summary>
        /// Gets or sets the remark.
        /// </summary>
        /// <value>
        /// The remark.
        /// </value>
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

    public class StatesPerProgramConverter : JsonConverter<Dictionary<CsdbProgram, Dictionary<int, UowState>>>
    {
        public override Dictionary<CsdbProgram, Dictionary<int, UowState>> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Create an empty dictionary to store the result
            var result = new Dictionary<CsdbProgram, Dictionary<int, UowState>>();

            // Check that the JSON token is a start object
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            // Read the next token
            reader.Read();

            // Loop until the end object token is reached
            while (reader.TokenType != JsonTokenType.EndObject)
            {
                // Check that the JSON token is a property name
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                // Parse the property name as a CsdbProgram enum value
                var program = Enum.Parse<CsdbProgram>(reader.GetString() ?? throw new InvalidOperationException(nameof(reader) + " was null"));

                // Read the next token
                reader.Read();

                // Check that the JSON token is a start object
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                // Read the next token
                reader.Read();

                // Create an empty dictionary to store the inner values
                var innerDict = new Dictionary<int, UowState>();

                // Loop until the end object token is reached
                while (reader.TokenType != JsonTokenType.EndObject)
                {
                    // Check that the JSON token is a property name
                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException();
                    }

                    // Parse the property name as an int value
                    var stateValue = int.Parse(reader.GetString() ?? throw new InvalidOperationException(nameof(reader) + " was null"));

                    // Read the next token
                    reader.Read();

                    // Deserialize the property value as a UowState object
                    var state = JsonSerializer.Deserialize<UowState>(ref reader, options);

                    // Set the StateValue property of the UowState object
                    Debug.Assert(state != null, nameof(state) + " != null");
                    state.StateValue = stateValue;

                    // Add the key-value pair to the inner dictionary
                    innerDict.Add(stateValue, state);

                    // Read the next token
                    reader.Read();
                }

                // Add the key-value pair to the result dictionary
                result.Add(program, innerDict);

                // Read the next token
                reader.Read();
            }

            // Return the result dictionary
            return result;
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<CsdbProgram, Dictionary<int, UowState>> value, JsonSerializerOptions options)
        {
            // This method is not implemented as it is not needed for deserialization
            throw new NotImplementedException();
        }
    }

}
