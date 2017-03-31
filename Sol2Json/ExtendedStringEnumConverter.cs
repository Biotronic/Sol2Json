using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace SolJson
{
    public class ExtendedStringEnumConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteToken(JsonToken.String, $"{value.GetType().Name}.{value}");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Debug.Assert(reader.TokenType == JsonToken.String);
            Debug.Assert(reader.Value is string);
            var parts = ((string) reader.Value).Split('.');
            

            var type = Assembly.GetExecutingAssembly().GetTypes().First(a => a.Name == parts[0] && a.IsEnum);
            return Enum.Parse(type, parts[1]);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsEnum;
        }
    }
}
