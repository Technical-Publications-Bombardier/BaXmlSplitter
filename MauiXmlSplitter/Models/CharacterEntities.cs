using System.Text.Json;
using System.Text.Json.Serialization;

namespace MauiXmlSplitter.Models
{
    public record CharacterEntities
    {
        public string Entity { get; set; } = string.Empty;
        private int character;
        public string Hex
        {
            get => character.ToString("X4");
            set => character = int.Parse(value, System.Globalization.NumberStyles.HexNumber);
        }
        public string Description { get; set; } = string.Empty;
    }
    public class CharacterEntitiesConverter : JsonConverter<CharacterEntities>
    {
        public override CharacterEntities Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var characterEntities = new CharacterEntities();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return characterEntities;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read();
                    switch (propertyName)
                    {
                        case "Entity":
                            characterEntities.Entity = reader.GetString() ?? string.Empty;
                            break;
                        case "Hex":
                            characterEntities.Hex = reader.GetString() ?? string.Empty;
                            break;
                        case "Description":
                            characterEntities.Description = reader.GetString() ?? string.Empty;
                            break;
                        default:
                            throw new JsonException();
                    }
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, CharacterEntities value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("Entity", value.Entity);
            writer.WriteString("Hex", value.Hex);
            writer.WriteString("Description", value.Description);
            writer.WriteEndObject();
        }
    }

}
