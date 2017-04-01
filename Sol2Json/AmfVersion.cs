using Newtonsoft.Json;

namespace SolJson
{
    [JsonConverter(typeof(ExtendedStringEnumConverter))]
    public enum AmfVersion
    {
        Amf0 = 0,
        Amf3 = 3
    }
}