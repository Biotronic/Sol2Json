using System.Collections.Generic;

namespace SolJson.Amf0
{
    [Amf0(Amf0BlockType.EcmaArray)]
    public class EcmaArrayBlock : Amf0Block<Dictionary<string, AmfBlock>>
    {
        public EcmaArrayBlock()
        {
        }

        public EcmaArrayBlock(Dictionary<string, AmfBlock> value) : base(value)
        {
        }
    }
}