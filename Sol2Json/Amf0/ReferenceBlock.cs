namespace SolJson.Amf0
{
    [Amf0(Amf0BlockType.Reference)]
    public class ReferenceBlock : Amf0Block<AmfBlock>
    {
        public ReferenceBlock()
        {
        }

        public ReferenceBlock(AmfBlock value) : base(value)
        {
        }
    }
}