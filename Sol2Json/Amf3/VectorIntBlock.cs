using System.Collections.Generic;
using System.Linq;

namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.VectorInt)]
    public class VectorIntBlock : Amf3Block<List<int>>
    {
        protected VectorIntBlock()
        {
        }

        protected VectorIntBlock(string name, Amf3BlockType type, Amf3Reader reader) : base(name, type, reader)
        {
        }

        protected override void ReadValue(Amf3Reader reader)
        {
            var val = reader.ReadInt();
            if (!val.Flags[0])
            {
                Value = ((VectorIntBlock)reader.ObjectPool[val.Values[1]]).Value.ToList();
                return;
            }
            var fixedVector = reader.ReadByte();
            Value = new List<int>();
            var count = val.Values[1];
            while (count > 0)
            {
                count--;
                Value.Add(reader.ReadInt32());
            }
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
                writer.WriteByte(0);
                foreach (var v in Value)
                {
                    writer.WriteInt32(v);
                }
                writer.ObjectPool.Add(this);
            }
        }
    }
}