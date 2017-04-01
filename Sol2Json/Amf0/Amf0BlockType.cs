using Newtonsoft.Json;

namespace SolJson.Amf0
{
    [JsonConverter(typeof(ExtendedStringEnumConverter))]
    public enum Amf0BlockType
    {
        Number = 0x00,
        Boolean = 0x01,
        String = 0x02,
        Object = 0x03,
        Movieclip = 0x04,
        Null = 0x05,
        Undefined = 0x06,
        Reference = 0x07,
        EcmaArray = 0x08,
        ObjectEnd = 0x09,
        StrictArray = 0x0a,
        Date = 0x0b,
        LongString = 0x0c,
        Unsupported = 0x0d,
        Recordset = 0x0e,
        XmlDocument = 0x0f,
        TypedObject = 0x10,
        Amf3Transition = 0x11
    }
}