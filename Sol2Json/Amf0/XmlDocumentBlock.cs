namespace SolJson.Amf0
{
    [Amf0(Amf0BlockType.XmlDocument)]
    public class XmlDocumentBlock : Amf0Block<string>
    {
        public XmlDocumentBlock()
        {
        }

        public XmlDocumentBlock(string value) : base(value)
        {
        }
    }
}