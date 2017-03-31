using System;
using System.Collections.Generic;
using System.Linq;

namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.Array)]
    public class ArrayBlock : Amf3Block<List<Amf3Block>>
    {
        protected ArrayBlock()
        {
        }

        private SerializableDictionary<string, Amf3Block> Associative { get; set; }

        protected ArrayBlock(string name, Amf3BlockType type, Amf3Reader reader) : base(name, type, reader)
        {
        }

        protected override void ReadValue(Amf3Reader reader)
        {
            var val = reader.ReadInt();

            if (!val.Flags[0])
            {
                Value = ((ArrayBlock)reader.ObjectPool[val.Values[1]]).Value;
                return;
            }
            var key = reader.ReadString();
            Associative = new SerializableDictionary<string, Amf3Block>();
            while (!string.IsNullOrEmpty(key))
            {
                Associative[key] = Amf3Reader.Read(reader, key);
                key = reader.ReadString();
            }
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
                if (Associative != null)
                {
                    foreach (var v in Associative)
                    {
                        writer.WriteString(v.Key);
                        writer.Write(v.Value);
                    }
                }
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