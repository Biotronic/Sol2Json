using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace SolJson.Amf3
{
    public class Amf3Reader : IDisposable
    {
        public Amf3Reader(Stream s)
        {
            Reader = new BinaryReader(s);
            StringPool = new List<string>();
            ObjectPool = new List<Amf3Block>();
            ObjectTraitsPool = new List<Amf3Block>();

            ReadHeader();
        }

        public struct Header
        {
            public ushort Magic { get; }
            public uint Size { get; }
            public uint Type { get; }
            public uint Unknown1 { get; }
            public ushort Unknown2 { get; }
            public string Name { get; }
            public uint AmfType { get; }

            internal Header(BinaryReader reader)
            {
                Magic = reader.ReadUInt16();
                Size = reader.ReadUInt32().ChangeEndianness();
                Type = reader.ReadUInt32();
                Unknown1 = reader.ReadUInt32();
                Unknown2 = reader.ReadUInt16();
                var nameSize = reader.ReadUInt16().ChangeEndianness();
                Name = Encoding.UTF8.GetString(reader.ReadBytes(nameSize));
                AmfType = reader.ReadUInt32();
            }
        }

        public Header FileHeader { get; private set; }

        private void ReadHeader()
        {
            FileHeader = new Header(Reader);

            Debug.Assert(FileHeader.Type == 0x4F534354);
            Debug.Assert(FileHeader.Unknown1 == 0x00000400);
        }

        private BinaryReader Reader { get; }
        public List<string> StringPool { get; }
        public List<Amf3Block> ObjectPool { get; }
        public List<Amf3Block> ObjectTraitsPool { get; }

        public Amf3BlockType ReadBlockType()
        {
            return (Amf3BlockType)Reader.ReadByte();
        }

        public string ReadString()
        {
            var size = ReadInt();
            if (!size.Flags[0]) return StringPool[size.Values[1]];
            var result = Encoding.UTF8.GetString(Reader.ReadBytes(size.Values[1]));
            if (!String.IsNullOrEmpty(result))
            {
                StringPool.Add(result);
            }
            return result;
        }

        private uint ReadUInt()
        {
            var b1 = Reader.ReadByte();
            var b2 = b1 <= 127 ? 0 : Reader.ReadByte();
            var b3 = b2 <= 127 ? 0 : Reader.ReadByte();
            var b4 = b3 <= 127 ? 0 : Reader.ReadByte();

            if (b3 > 127)
            {
                return (uint)(
                    b4 |
                    ((b3 & 0x7f) << 8) |
                    ((b2 & 0x7f) << 15) |
                    ((b1 & 0x7f) << 22)
                    );
            }
            if (b2 > 127)
            {
                return (uint)(
                    b3 |
                    ((b2 & 0x7f) << 7) |
                    ((b1 & 0x7f) << 14)
                    );
            }
            if (b1 > 127)
            {
                return (uint)(
                    b2 |
                    ((b1 & 0x7f) << 7)
                    );
            }
            return b1;
        }

        public FlaggedInt ReadInt()
        {
            var uintValue = ReadUInt();
            var sign = uintValue & (1 << 28);
            if (sign != 0)
            {
                sign *= 0xFF;
                uintValue |= sign;
            }

            var result = new FlaggedInt((int)uintValue);
            return result;
        }

        public int ReadInt32()
        {
            return Reader.ReadInt32();
        }

        public uint ReadUInt32()
        {
            return Reader.ReadUInt32();
        }

        public byte ReadByte()
        {
            return Reader.ReadByte();
        }

        public double ReadDouble()
        {
            var data = Reader.ReadUInt64().ChangeEndianness();
            return BitConverter.Int64BitsToDouble((long) data);
        }

        public List<byte> ReadBytes(int count)
        {
            return Reader.ReadBytes(count).ToList();
        }

        public void Dispose()
        {
            Reader?.Dispose();
        }

        public static Amf3Block Read(Amf3Reader reader, string name)
        {
            var type = reader.ReadBlockType();
            return (Amf3Block)Amf3Block.Blocks[type].Invoke(new object[] { name, type, reader });
        }
    }
}
