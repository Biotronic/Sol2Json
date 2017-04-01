namespace SolJson.Amf0
{
    [Amf0(Amf0BlockType.LongString)]
    public class LongStringBlock : Amf0Block<string>
    {
        public LongStringBlock()
        {
        }

        public LongStringBlock(string value) : base(value)
        {
        }
    }
}