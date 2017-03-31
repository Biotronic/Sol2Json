using System;
using System.Collections.Generic;

namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.Date)]
    public class DateBlock : Amf3Block<DateTime>
    {
        protected DateBlock()
        {
        }

        protected DateBlock(string name, Amf3BlockType type, Amf3Reader reader) : base(name, type, reader)
        {
        }

        protected override void ReadValue(Amf3Reader reader)
        {
            var val = reader.ReadInt();

            if (!val.Flags[0])
            {
                Value = ((DateBlock)reader.ObjectPool[val.Values[0]]).Value;
                return;
            }
            var tmp = reader.ReadDouble();
            Value = new DateTime(1970, 1, 1).AddMilliseconds((ulong)tmp);
            reader.ObjectPool.Add(this);
        }

        public override void WriteValue(Amf3Writer writer)
        {
            var index = writer.FindReference(this);
            writer.WriteInt(0, index < 0);
            if (index == -1)
            {
                writer.WriteDouble((Value - new DateTime(1970, 1, 1)).TotalMilliseconds);
                writer.ObjectPool.Add(this);
            }
        }
    }
}