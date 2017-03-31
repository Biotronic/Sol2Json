using System.Collections.Generic;

namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.False)]
    [Amf3(Amf3BlockType.True)]
    public class BooleanBlock : Amf3Block<bool>
    {
        protected BooleanBlock()
        {
        }

        public override Amf3BlockType Type => Value ? Amf3BlockType.True : Amf3BlockType.False;

        protected BooleanBlock(string name, Amf3BlockType type, Amf3Reader reader) : base(name, type, reader)
        {
            Value = type == Amf3BlockType.True;
        }

        protected override void ReadValue(Amf3Reader reader)
        {
        }

        public override void WriteValue(Amf3Writer writer)
        {
        }
    }
}