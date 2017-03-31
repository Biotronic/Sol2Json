using System.Collections.Generic;

namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.VectorObject)]
    public class VectorObjectBlock : Amf3Block<List<AmfBlock>>
    {
        public VectorObjectBlock()
        {
        }

        public VectorObjectBlock(string typeName, List<AmfBlock> value, bool fixedLength) : base(value)
        {
            TypeName = typeName;
            FixedLength = fixedLength;
        }

        public string TypeName { get; set; }
        public bool FixedLength { get; set; }
    }
}