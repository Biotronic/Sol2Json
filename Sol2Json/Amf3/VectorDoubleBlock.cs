using System.Collections.Generic;

namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.VectorDouble)]
    public class VectorDoubleBlock : Amf3Block<List<double>>
    {
        public VectorDoubleBlock()
        {
        }

        public VectorDoubleBlock(List<double> value, bool fixedLength) : base(value)
        {
            FixedLength = fixedLength;
        }

        public bool FixedLength { get; set; }
    }
}