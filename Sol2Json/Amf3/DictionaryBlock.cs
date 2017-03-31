using System.Collections.Generic;

namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.Dictionary)]
    public class DictionaryBlock : Amf3Block<Dictionary<AmfBlock, AmfBlock>>
    {
        public DictionaryBlock()
        {
        }

        public DictionaryBlock(Dictionary<AmfBlock,AmfBlock> value, bool weakReferences) : base(value)
        {
            WeakReferences = weakReferences;
        }

        public bool WeakReferences { get; set; }
    }
}