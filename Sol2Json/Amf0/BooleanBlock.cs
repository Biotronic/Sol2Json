namespace SolJson.Amf0
{
    [Amf0(Amf0BlockType.Boolean)]
    public class BooleanBlock : Amf0Block<bool>
    {
        public BooleanBlock()
        {
        }

        public BooleanBlock(bool value) : base(value)
        {
        }
    }
}