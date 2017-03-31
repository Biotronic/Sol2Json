namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.Integer)]
    public class IntegerBlock : Amf3Block<int>
    {
        public IntegerBlock()
        {
        }

        public IntegerBlock(int value) : base(value)
        {
        }
    }
}