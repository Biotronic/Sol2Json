namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.False)]
    [Amf3(Amf3BlockType.True)]
    public class BooleanBlock : Amf3Block<bool>
    {
        public BooleanBlock()
        {
        }

        public BooleanBlock(bool value) : base(value)
        {
        }

        public override Amf3BlockType Type => Value ? Amf3BlockType.True : Amf3BlockType.False;
    }
}