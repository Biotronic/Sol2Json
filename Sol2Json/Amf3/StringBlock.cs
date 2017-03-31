using System.Collections.Generic;

namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.String)]
    public class StringBlock : Amf3Block<string>
    {
        protected StringBlock()
        {
        }

        protected StringBlock(string name, Amf3BlockType type, Amf3Reader reader) : base(name, type, reader)
        {
        }

        protected override void ReadValue(Amf3Reader reader)
        {
            Value = reader.ReadString();
        }

        public override void WriteValue(Amf3Writer writer)
        {
            writer.WriteString(Value);
        }
    }
}