using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace SolJson.Amf3
{
    public class Amf3Writer : IDisposable
    {
        public Amf3Writer(Stream s, string name)
        {
            Stream = s;
            Writer = new BinaryWriter(s);
            StringPool = new OrderedHashSet<string>();
            ObjectPool = new OrderedHashSet<Amf3Block>();
            ObjectTraitsPool = new OrderedHashSet<ObjectBlock>();
            WriteHeader(name);
        }

        private void WriteHeader(string name)
        {
            Writer.Write((ushort)0xBF00);
            Writer.Write(0);
            Writer.Write(0x4F534354);
            Writer.Write(new byte[] { 0, 4, 0, 0, 0, 0 });
            Writer.Write(((ushort)name.Length).ChangeEndianness());
            Writer.Write(name.ToCharArray());
            Writer.Write(new byte[] { 0, 0, 0, 3 });
        }

        public void Close()
        {
            Stream.Seek(2, SeekOrigin.Begin);
            var size = (uint)(Stream.Length - 6);
            Writer.Write(size.ChangeEndianness());
            Writer.Write(0x4F534354);
            Closed = true;
        }

        private bool Closed { get; set; }
        private Stream Stream { get; }
        private BinaryWriter Writer { get; }
        public OrderedHashSet<string> StringPool { get; }
        public OrderedHashSet<Amf3Block> ObjectPool { get; }
        public OrderedHashSet<ObjectBlock> ObjectTraitsPool { get; }

        public void WriteString(string value)
        {

            if (!string.IsNullOrEmpty(value))
            {
                var refIndex = StringPool.IndexOf(value);
                if (refIndex != -1)
                {
                    WriteInt(refIndex, false);
                    return;
                }
                StringPool.Add(value);
            }
            value = value ?? "";

            var val = Encoding.UTF8.GetBytes(value);
            WriteInt(val.Length, true);
            Writer.Write(val);
        }

        public void WriteInt(int value, params bool[] flags)
        {

            if (value >= 1 << (28 - flags.Length)) throw new ArgumentOutOfRangeException(nameof(value));
            if (-value > 1 << (28 - flags.Length)) throw new ArgumentOutOfRangeException(nameof(value));
            var v = (uint)value & ((1u << (29 - flags.Length)) - 1);
            WriteUInt(v, flags);
        }

        public void WriteUInt(uint value, params bool[] flags)
        {
            if (value > 1u << (29 - flags.Length)) throw new ArgumentOutOfRangeException(nameof(value));

            foreach (var flag in flags.Reverse())
            {
                value <<= 1;
                value |= flag ? 1u : 0u;
            }

            if (value < 0x7F)
            {
                Writer.Write((byte)value);
            }
            else if (value < 0x3FFFF)
            {
                Writer.Write((byte)(0x80 | value >> 7));
                Writer.Write((byte)(0x7F & value));
            }
            else if (value < 0x1FFFFF)
            {
                Writer.Write((byte)(0x80 | value >> 14));
                Writer.Write((byte)(0x80 | value >> 7));
                Writer.Write((byte)(0x7F & value));
            }
            else if (value < 0x3FFFFFFF)
            {
                Writer.Write((byte)(0x80 | value >> 22));
                Writer.Write((byte)(0x80 | value >> 15));
                Writer.Write((byte)(0x80 | value >> 8));
                Writer.Write((byte)(0xFF & value));
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        public void WriteInt32(int value)
        {
            Writer.Write(value);
        }

        public void WriteUInt32(uint value)
        {
            Writer.Write(value);
        }

        public void WriteByte(byte value)
        {
            Writer.Write(value);
        }

        public void WriteDouble(double value)
        {
            var data = (ulong)BitConverter.DoubleToInt64Bits(value);
            Writer.Write(data.ChangeEndianness());
        }

        public void WriteBytes(List<byte> value)
        {
            Writer.Write(value.ToArray());
        }

        public void WriteBlockType(Amf3BlockType value)
        {
            Writer.Write((byte)value);
        }

        public void Dispose()
        {
            Debug.Assert(Closed, "Need to close before disposing.");
            Writer?.Dispose();
        }

        public int FindReference<T>(Amf3Block<T> block)
        {
            return ObjectPool.Select(Tuple.Create<Amf3Block, int>)
                       .FirstOrDefault(a => a.Item1 == block)?.Item2 ?? -1;
        }

        public int FindTraits(ObjectBlock block)
        {
            return ObjectTraitsPool.Select(Tuple.Create<ObjectBlock, int>)
                       .FirstOrDefault(a => a.Item1.Traits.SequenceEqual(block.Traits))?.Item2 ?? -1;
        }

        public void Write(Amf3Block amf3Block)
        {
            this.WriteBlockType(amf3Block.Type);
            amf3Block.WriteValue(this);
        }
    }
}
