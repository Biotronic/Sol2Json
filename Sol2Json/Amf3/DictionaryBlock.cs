using System.Collections.Generic;

namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.Dictionary)]
    public class DictionaryBlock : Amf3Block<SerializableDictionary<Amf3Block, Amf3Block>>
    {
        protected DictionaryBlock()
        {
        }

        protected DictionaryBlock(string name, Amf3BlockType type, Amf3Reader reader) : base(name, type, reader)
        {
        }

        protected override void ReadValue(Amf3Reader reader)
        {
            var val = reader.ReadInt();
            if (!val.Flags[0])
            {
                Value = ((DictionaryBlock)reader.ObjectPool[val.Values[1]]).Value.ToSerializableDictionary(a => a.Key, a => a.Value);
                return;
            }
            var weak = reader.ReadByte();
            Value = new SerializableDictionary<Amf3Block, Amf3Block>();
            for (var i = 0; i < val.Values[1]; ++i)
            {
                var key = Amf3Reader.Read(reader, $"key {i}");
                var value = Amf3Reader.Read(reader, $"value {i}");
                Value.Add(key, value);
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
                foreach (var block in Value)
                {
                    writer.Write(block.Key);
                    writer.Write(block.Value);
                }
                writer.ObjectPool.Add(this);
            }
        }
    }
}