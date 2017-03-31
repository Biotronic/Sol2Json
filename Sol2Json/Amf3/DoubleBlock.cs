namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.Double)]
    public class DoubleBlock : Amf3Block<double>
    {
        public DoubleBlock()
        {
        }

        public DoubleBlock(double value) : base(value)
        {
        }
    }
}