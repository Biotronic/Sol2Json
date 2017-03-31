namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.XmlDoc)]
    public class XmlDocBlock : Amf3Block<string>
    {
        public XmlDocBlock()
        {
        }

        public XmlDocBlock(string value) : base(value)
        {
        }
    }
}