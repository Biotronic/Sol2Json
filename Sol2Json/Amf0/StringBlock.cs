namespace SolJson.Amf0
{
    [Amf0(Amf0BlockType.String)]
    public class StringBlock : Amf0Block<string>
    {
        public StringBlock()
        {
        }

        public StringBlock(string value) : base(value)
        {
        }
    }
}