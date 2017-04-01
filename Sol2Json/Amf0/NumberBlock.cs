namespace SolJson.Amf0
{
    [Amf0(Amf0BlockType.Number)]
    public class NumberBlock : Amf0Block<double>
    {
        public NumberBlock()
        {
        }

        public NumberBlock(double value) : base(value)
        {
        }
    }
}