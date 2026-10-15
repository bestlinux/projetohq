using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProjetoHQApi.Application.Common.Converters
{
    public class BoolOrIntJsonConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.True) return 1;
            if (reader.TokenType == JsonTokenType.False) return 0;
            if (reader.TokenType == JsonTokenType.Number) return reader.GetInt32();
            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                if (bool.TryParse(str, out var b)) return b ? 1 : 0;
                if (int.TryParse(str, out var i)) return i;
            }
            return 0;
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
