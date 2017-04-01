using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            var typeString = jsonObject["Type"].Value<string>();
            var value = jsonObject["Value"];

            var parts = typeString.Split('.');

            var enumType = objectType.Assembly.GetTypes().First(a => a.Name == parts[0] && a.IsEnum);
            var type = Enum.Parse(enumType, parts[1]);
            if (type is Amf3.Amf3BlockType)
            {
                var result = AmfBlock.CreateBlock((Amf3.Amf3BlockType)type, name);

                var generic = result.GetType()
                    .Recurse(a => a.BaseType)
                    .TakeWhile(a => a != null)
                    .FirstOrDefault(a => a.IsGenericType && a.GetGenericTypeDefinition() == typeof(Amf3.Amf3Block<>));

                if (generic == null) return result;

                var blockValue = serializer.Deserialize(value.CreateReader(), generic.GenericTypeArguments[0]);

                result.GetType().GetProperty("Value").SetValue(result, blockValue);

                if (!(result is Amf3.ObjectBlock)) return result;

                ((Amf3.ObjectBlock)result).ClassName = jsonObject["ClassName"].Value<string>();
                ((Amf3.ObjectBlock)result).Traits =
                    (List<string>)serializer.Deserialize(jsonObject["Traits"].CreateReader(), typeof(List<string>));

                return result;
            }
            if (type is Amf0.Amf0BlockType)
            {
                var result = AmfBlock.CreateBlock((Amf0.Amf0BlockType) type, name);

                var generic = result.GetType()
                    .Recurse(a => a.BaseType)
                    .TakeWhile(a => a != null)
                    .FirstOrDefault(a => a.IsGenericType && a.GetGenericTypeDefinition() == typeof(Amf0.Amf0Block<>));

                if (generic == null) return result;

                var blockValue = serializer.Deserialize(value.CreateReader(), generic.GenericTypeArguments[0]);

                result.GetType().GetProperty("Value").SetValue(result, blockValue);

                if (!(result is Amf0.TypedObjectBlock)) return result;

                ((Amf0.TypedObjectBlock)result).ClassName = jsonObject["ClassName"].Value<string>();

                return result;
            }
            throw new ArgumentOutOfRangeException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsSubclassOf(typeof(AmfBlock)) || objectType == typeof(AmfBlock);
        }
    }
}