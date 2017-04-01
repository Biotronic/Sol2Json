namespace SolJson.Amf0
{
    public class Amf0Block : AmfBlock
    {
        public Amf0BlockType Type { get; set; }
    }

    public class Amf0Block<T> : Amf0Block
    {
        public T Value { get; set; }

        public Amf0Block()
        {
        }

        public Amf0Block(T value)
        {
            Value = value;
        }
    }
}
