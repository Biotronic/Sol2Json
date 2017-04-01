using System.Collections.Generic;

namespace SolJson.Amf0
{
    [Amf0(Amf0BlockType.TypedObject)]
    public class TypedObjectBlock : Amf0Block<Dictionary<string, AmfBlock>>
    {
        public string ClassName { get; set; }
        
        public TypedObjectBlock()
        {
        }

        public TypedObjectBlock(Dictionary<string, AmfBlock> value, string className) : base(value)
        {
            ClassName = className;
        }
    }
}