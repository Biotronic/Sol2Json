using System.Collections.Generic;

namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.VectorInt)]
    public class VectorIntBlock : Amf3Block<List<int>>
    {
        public VectorIntBlock()
        {
        }

        public VectorIntBlock(List<int> value, bool fixedLength) : base(value)
        {
            FixedLength = fixedLength;
        }

        public bool FixedLength { get; set; }
    }
}