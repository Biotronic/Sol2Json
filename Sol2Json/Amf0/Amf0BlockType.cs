using Newtonsoft.Json;

namespace SolJson.Amf0
{
    [JsonConverter(typeof(ExtendedStringEnumConverter))]
    public enum Amf0BlockType
    {
    }
}