using System.Collections.Generic;

namespace SolJson.Amf0
{
    [Amf0(Amf0BlockType.StrictArray)]
    public class StrictArrayBlock : Amf0Block<List<AmfBlock>>
    {
        public StrictArrayBlock()
        {
        }

        public StrictArrayBlock(List<AmfBlock> value) : base(value)
        {
        }
    }
}