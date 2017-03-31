using System;
using System.Collections.Generic;
using System.Linq;

namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.VectorObject)]
    public class VectorObjectBlock : Amf3Block<List<Amf3Block>>
    {
        protected VectorObjectBlock()
        {
        }

        public string TypeName { get; set; }

        protected VectorObjectBlock(string name, Amf3BlockType type, Amf3Reader reader) : base(name, type, reader)
        {
        }

        protected override void ReadValue(Amf3Reader reader)
        {
            var val = reader.ReadInt();
            if (!val.Flags[0])
            {
                Value = ((VectorObjectBlock)reader.ObjectPool[val.Values[1]]).Value.ToList();
                return;
            }
            var fixedVector = reader.ReadByte();
            TypeName = reader.ReadString();
            Value = new List<Amf3Block>();
            for (var i = 0; i < val.Values[1]; ++i)
            {
                Value.Add(Amf3Reader.Read(reader, i.ToString()));
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
                writer.WriteString("");
                foreach (var v in Value)
                {
                    writer.Write(v);
                }
                writer.ObjectPool.Add(this);
            }
        }
    }
}