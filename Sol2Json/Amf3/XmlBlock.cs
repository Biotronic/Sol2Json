using System.Collections.Generic;

namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.Xml)]
    public class XmlBlock : Amf3Block<string>
    {
        protected XmlBlock()
        {
        }

        protected XmlBlock(string name, Amf3BlockType type, Amf3Reader reader) : base(name, type, reader)
        {
        }

        protected override void ReadValue(Amf3Reader reader)
        {
            // TODO: References
            Value = reader.ReadString();
            reader.ObjectPool.Add(this);
        }

        public override void WriteValue(Amf3Writer writer)
        {
            var index = writer.FindReference(this);
            if (index == -1)
            {
                writer.WriteString(Value);
                writer.ObjectPool.Add(this);
            }
            else
            {
                writer.WriteInt(index, false);
            }
        }
    }
}