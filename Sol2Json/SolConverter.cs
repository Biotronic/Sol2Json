using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SolJson.Amf3;

namespace SolJson
{
    public class SolConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JObject.FromObject(value).WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var name = jsonObject["Name"].Value<string>();
            var type = (Amf3BlockType)jsonObject["Type"].Value<long>();
            var value = jsonObject["Value"];
            
            var result = (Amf3Block)Amf3Block.Blocks[type].Invoke(new object[] { name, type, null });

            var generic = result.GetType()
                .Recurse(a => a.BaseType)
                .TakeWhile(a => a != null)
                .FirstOrDefault(a => a.IsGenericType && a.GetGenericTypeDefinition() == typeof(Amf3Block<>));

            if (generic == null) return result;
            
            generic.GetProperty("Value").SetValue(result,
                serializer.Deserialize(value.CreateReader(), generic.GenericTypeArguments[0]));

            if (!(result is ObjectBlock)) return result;

            ((ObjectBlock) result).ClassName = jsonObject["ClassName"].Value<string>();
            ((ObjectBlock) result).Traits = (List<string>)serializer.Deserialize(jsonObject["Traits"].CreateReader(), typeof(List<string>));

            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Amf3Block);
        }
    }
}