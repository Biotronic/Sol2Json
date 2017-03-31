using System.Collections.Generic;

namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.VectorUInt)]
    public class VectorUIntBlock : Amf3Block<List<uint>>
    {
        public VectorUIntBlock()
        {
        }

        public VectorUIntBlock(List<uint> value, bool fixedLength) : base(value)
        {
            FixedLength = fixedLength;
        }

        public bool FixedLength { get; set; }
    }
}