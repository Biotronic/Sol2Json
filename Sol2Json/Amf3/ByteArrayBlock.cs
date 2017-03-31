using System.Collections.Generic;
using System.Linq;

namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.ByteArray)]
    public class ByteArrayBlock : Amf3Block<List<byte>>
    {
        protected ByteArrayBlock()
        {
        }

        protected ByteArrayBlock(string name, Amf3BlockType type, Amf3Reader reader) : base(name, type, reader)
        {
        }

        protected override void ReadValue(Amf3Reader reader)
        {
            var val = reader.ReadInt();
            if (!val.Flags[0])
            {
                Value = ((ByteArrayBlock)reader.ObjectPool[val.Values[1]]).Value.ToList();
                return;
            }
            Value = reader.ReadBytes(val.Values[1]);
            reader.ObjectPool.Add(this);
        }

        public override void WriteValue(Amf3Writer writer)
        {
            var index = writer.FindReference(this);
            if (index > -1)
            {
                writer.WriteInt(index, false);
            }
            else
            {
                writer.WriteInt(Value.Count, true);
                writer.WriteBytes(Value);
                writer.ObjectPool.Add(this);
            }
        }
    }
}