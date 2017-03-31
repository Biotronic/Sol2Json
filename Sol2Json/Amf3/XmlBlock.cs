namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.Xml)]
    public class XmlBlock : Amf3Block<string>
    {
        public XmlBlock()
        {
        }

        public XmlBlock(string value) : base(value)
        {
        }
    }
}