using System.Collections.Generic;

namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.Double)]
    public class DoubleBlock : Amf3Block<double>
    {
        protected DoubleBlock()
        {
        }

        protected DoubleBlock(string name, Amf3BlockType type, Amf3Reader reader) : base(name, type, reader)
        {
        }

        protected override void ReadValue(Amf3Reader reader)
        {
            Value = reader.ReadDouble();
        }

        public override void WriteValue(Amf3Writer writer)
        {
            writer.WriteDouble(Value);
        }
    }
}