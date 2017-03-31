using System.Collections.Generic;

namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.Integer)]
    public class IntegerBlock : Amf3Block<int>
    {
        protected IntegerBlock()
        {
        }

        protected IntegerBlock(string name, Amf3BlockType type, Amf3Reader reader) : base(name, type, reader)
        {
        }

        protected override void ReadValue(Amf3Reader reader)
        {
            Value = reader.ReadInt().Values[0];
        }

        public override void WriteValue(Amf3Writer writer)
        {
            writer.WriteInt(Value);
        }
    }
}