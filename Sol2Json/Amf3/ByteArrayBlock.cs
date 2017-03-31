using System.Collections.Generic;

namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.ByteArray)]
    public class ByteArrayBlock : Amf3Block<List<byte>>
    {
        public ByteArrayBlock()
        {
        }

        public ByteArrayBlock(List<byte> value) : base(value)
        {
        }
    }
}