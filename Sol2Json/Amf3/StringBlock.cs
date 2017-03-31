namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.String)]
    public class StringBlock : Amf3Block<string>
    {
        public StringBlock()
        {
        }

        public StringBlock(string value) : base(value)
        {
        }
    }
}