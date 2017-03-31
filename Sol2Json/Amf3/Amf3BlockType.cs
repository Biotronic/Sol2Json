using Newtonsoft.Json;

namespace SolJson.Amf3
{
    [JsonConverter(typeof(ExtendedStringEnumConverter))]
    public enum Amf3BlockType
    {
        Undefined = 0x00,
        Null = 0x01,
        False = 0x02,
        True = 0x03,
        Integer = 0x04,
        Double = 0x05,
        String = 0x06,
        XmlDoc = 0x07,
        Date = 0x08,
        Array = 0x09,
        Object = 0x0A,
        Xml = 0x0B,
        ByteArray = 0x0C,
        VectorInt = 0x0D,
        VectorUInt = 0x0E,
        VectorDouble = 0x0F,
        VectorObject = 0x10,
        Dictionary = 0x11
    }
}