using System.Collections.Generic;

namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.Null)]
    public class NullBlock : Amf3Block
    {
        protected NullBlock()
        {
        }

        protected NullBlock(string name, Amf3BlockType type, Amf3Reader reader) : base(name, type, reader)
        {
        }

        protected override void ReadValue(Amf3Reader reader)
        {
        }

        public override void WriteValue(Amf3Writer writer)
        {
        }
    }
}