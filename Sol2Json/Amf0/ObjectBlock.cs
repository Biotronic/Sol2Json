using System.Collections.Generic;

namespace SolJson.Amf0
{
    [Amf0(Amf0BlockType.Object)]
    public class ObjectBlock : Amf0Block<Dictionary<string, AmfBlock>>
    {
        public ObjectBlock()
        {
        }

        public ObjectBlock(Dictionary<string, AmfBlock> value) : base(value)
        {
        }
    }
}