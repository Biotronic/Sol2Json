using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace SolJson
{
    public class AmfReader : IDisposable
    {
        public AmfVersion Version { get; set; }
        private BinaryReader Reader { get; }
        private FileHeader Header { get; set; }
        public List<string> StringPool { get; set; }
        public List<AmfBlock> ObjectPool { get; set; }
        public List<List<string>> TraitsPool { get; set; }


        public AmfReader(Stream stream)
        {
            Reader = new BinaryReader(stream);
            StringPool = new List<string>();
            ObjectPool = new List<AmfBlock>();
            TraitsPool = new List<List<string>>();
        }

        public void Dispose()
        {
            Reader?.Dispose();
        }

        public void ReadHeader()
        {
            Header = new FileHeader(Reader);

            Debug.Assert(Header.Type == 0x4F534354);
            Debug.Assert(Header.Unknown1 == 0x00000400);

            Version = Header.Version;
        }

        public struct FileHeader
        {
            public ushort Magic { get; }
            public uint Size { get; }
            public uint Type { get; }
            public uint Unknown1 { get; }
            public ushort Unknown2 { get; }
            public string Name { get; }
            public AmfVersion Version { get; }

            internal FileHeader(BinaryReader reader)
            {
                Magic = reader.ReadUInt16();
                Size = reader.ReadUInt32().ChangeEndianness();
                Type = reader.ReadUInt32();
                Unknown1 = reader.ReadUInt32();
                Unknown2 = reader.ReadUInt16();
                var nameSize = reader.ReadUInt16().ChangeEndianness();
                Name = Encoding.UTF8.GetString(reader.ReadBytes(nameSize));
                Version = (AmfVersion)reader.ReadUInt32().ChangeEndianness();
            }
        }

        public Amf3.Amf3BlockType ReadBlockType()
        {
            return (Amf3.Amf3BlockType)Reader.ReadByte();
        }

        public string ReadString()
        {
            var size = ReadInt();
            if (!size.Flags[0]) return StringPool[size.Values[1]];
            var result = Encoding.UTF8.GetString(Reader.ReadBytes(size.Values[1]));
            if (!string.IsNullOrEmpty(result))
            {
                StringPool.Add(result);
            }
            return result;
        }

        public string ReadString(int length)
        {
            return Encoding.UTF8.GetString(Reader.ReadBytes(length));
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
            return BitConverter.Int64BitsToDouble((long)data);
        }

        public List<byte> ReadBytes(int count)
        {
            return Reader.ReadBytes(count).ToList();
        }

        public AmfBlock ReadBlock(string name)
        {
            if (Version == AmfVersion.Amf0)
            {
                throw new NotImplementedException();
            }
            if (Version == AmfVersion.Amf3)
            {
                Amf3.Amf3Block result;
                var type = ReadBlockType();
                switch (type)
                {
                    case Amf3.Amf3BlockType.Undefined:
                        result = new Amf3.UndefinedBlock();
                        break;
                    case Amf3.Amf3BlockType.Null:
                        result = new Amf3.NullBlock();
                        break;
                    case Amf3.Amf3BlockType.False:
                        result = new Amf3.BooleanBlock(false);
                        break;
                    case Amf3.Amf3BlockType.True:
                        result = new Amf3.BooleanBlock(true);
                        break;
                    case Amf3.Amf3BlockType.Integer:
                        result = new Amf3.IntegerBlock(ReadInt().Values[0]);
                        break;
                    case Amf3.Amf3BlockType.Double:
                        result = new Amf3.DoubleBlock(ReadDouble());
                        break;
                    case Amf3.Amf3BlockType.String:
                        result = new Amf3.StringBlock(ReadString());
                        break;
                    case Amf3.Amf3BlockType.XmlDoc:
                        result = ReadXmlDocBlock();
                        break;
                    case Amf3.Amf3BlockType.Date:
                        result = ReadDateBlock();
                        break;
                    case Amf3.Amf3BlockType.Array:
                        result = ReadArrayBlock();
                        break;
                    case Amf3.Amf3BlockType.Object:
                        result = ReadObjectBlock();
                        break;
                    case Amf3.Amf3BlockType.Xml:
                        result = ReadXmlBlock();
                        break;
                    case Amf3.Amf3BlockType.ByteArray:
                        result = ReadByteArray();
                        break;
                    case Amf3.Amf3BlockType.VectorInt:
                        result = ReadVectorInt();
                        break;
                    case Amf3.Amf3BlockType.VectorUInt:
                        result = ReadVectorUint();
                        break;
                    case Amf3.Amf3BlockType.VectorDouble:
                        result = ReadVectorDouble();
                        break;
                    case Amf3.Amf3BlockType.VectorObject:
                        result = ReadVectorObject();
                        break;
                    case Amf3.Amf3BlockType.Dictionary:
                        result = ReadDictionary();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                result.Type = type;
                result.Name = name;
                return result;
            }
            throw new ArgumentOutOfRangeException();
        }

        private Amf3.ObjectBlock ReadObjectBlock()
        {
            Dictionary<string, AmfBlock> fields;
            var className = string.Empty;
            List<string> traits;

            var val = ReadInt();
            if (!val.Flags[0])
            {
                // U29O-ref
                return (Amf3.ObjectBlock)ObjectPool[val.Values[1]];
            }
            if (!val.Flags[1])
            {
                // U29O-traits-ref
                traits = TraitsPool[val.Values[2]];
                //className = TraitsPool[val.Values[2]];
                fields = new Dictionary<string, AmfBlock>();
                foreach (var trait in traits)
                {
                    fields[trait] = ReadBlock(trait);
                }
            }
            else if (val.Flags[2])
            {
                // U29O-traits-ext
                throw new NotImplementedException("Cannot deserialize random data. Sorry. :(");
            }
            else
            {
                // U29O-traits
                //var isDynamic = val.Flags[3];
                var count = val.Values[4];
                className = ReadString();

                fields = new Dictionary<string, AmfBlock>();
                traits = new List<string>();
                for (var i = 0; i < count; ++i)
                {
                    traits.Add(ReadString());
                }
                for (var i = 0; i < count; ++i)
                {
                    fields[traits[i]] = ReadBlock(traits[i]);
                }
                TraitsPool.Add(traits);
            }
            var name = ReadString();
            while (!string.IsNullOrEmpty(name))
            {
                fields[name] = ReadBlock(name);
                name = ReadString();
            }
            var result = new Amf3.ObjectBlock(className,traits,fields);
            ObjectPool.Add(result);
            return result;
        }

        private Amf3.DictionaryBlock ReadDictionary()
        {
            var val = ReadInt();
            if (!val.Flags[0])
            {
                return (Amf3.DictionaryBlock)ObjectPool[val.Values[1]];
            }
            var weakReferences = ReadByte() == 1;
            var dic = new Dictionary<AmfBlock, AmfBlock>();
            for (var i = 0; i < val.Values[1]; ++i)
            {
                var key = ReadBlock($"key {i}");
                var value = ReadBlock($"value {i}");
                dic.Add(key, value);
            }
            var result = new Amf3.DictionaryBlock(dic, weakReferences);
            ObjectPool.Add(result);
            return result;
        }

        private Amf3.VectorObjectBlock ReadVectorObject()
        {
            var val = ReadInt();
            if (!val.Flags[0])
            {
                return ((Amf3.VectorObjectBlock)ObjectPool[val.Values[1]]);
            }
            var fixedLength = ReadByte() == 1;
            var typeName = ReadString();
            var value = new List<AmfBlock>();
            for (var i = 0; i < val.Values[1]; ++i)
            {
                value.Add(ReadBlock(i.ToString()));
            }
            var result = new Amf3.VectorObjectBlock(typeName, value, fixedLength);
            ObjectPool.Add(result);
            return result;
        }

        private Amf3.VectorDoubleBlock ReadVectorDouble()
        {
            var val = ReadInt();
            if (!val.Flags[0])
            {
                return ((Amf3.VectorDoubleBlock)ObjectPool[val.Values[1]]);
            }
            var fixedLength = ReadByte()==1;
            var value = new List<double>();
            var count = val.Values[1];
            while (count > 0)
            {
                count--;
                value.Add(ReadDouble());
            }
            var result = new Amf3.VectorDoubleBlock(value, fixedLength);
            ObjectPool.Add(result);
            return result;
        }

        private Amf3.VectorUIntBlock ReadVectorUint()
        {
            var val = ReadInt();
            if (!val.Flags[0])
            {
                return ((Amf3.VectorUIntBlock)ObjectPool[val.Values[1]]);
            }
            var fixedLength = ReadByte()==1;
            var value = new List<uint>();
            var count = val.Values[1];
            while (count > 0)
            {
                count--;
                value.Add(ReadUInt32());
            }
            var result = new Amf3.VectorUIntBlock(value, fixedLength);
            ObjectPool.Add(result);
            return result;
        }

        private Amf3.VectorIntBlock ReadVectorInt()
        {
            var val = ReadInt();
            if (!val.Flags[0])
            {
                return ((Amf3.VectorIntBlock)ObjectPool[val.Values[1]]);
            }
            var fixedLength = ReadByte()==1;
            var value = new List<int>();
            var count = val.Values[1];
            while (count > 0)
            {
                count--;
                value.Add(ReadInt32());
            }
            var result = new Amf3.VectorIntBlock(value, fixedLength);
            ObjectPool.Add(result);
            return result;
        }

        private Amf3.ByteArrayBlock ReadByteArray()
        {
            var val = ReadInt();
            if (!val.Flags[0])
            {
                return (Amf3.ByteArrayBlock)ObjectPool[val.Values[1]];
            }
            var result = new Amf3.ByteArrayBlock(ReadBytes(val.Values[1]));
            ObjectPool.Add(result);
            return result;
        }

        private Amf3.XmlBlock ReadXmlBlock()
        {
            var v = ReadInt();
            if (!v.Flags[0]) return (Amf3.XmlBlock)ObjectPool[v.Values[1]];
            var result = new Amf3.XmlBlock(ReadString(v.Values[1]));
            ObjectPool.Add(result);

            return result;
        }

        private Amf3.DateBlock ReadDateBlock()
        {
            var val = ReadInt();

            if (!val.Flags[0])
            {
                return ((Amf3.DateBlock)ObjectPool[val.Values[0]]);
            }
            var milliseconds = ReadDouble();
            var value = new DateTime(1970, 1, 1).AddMilliseconds((ulong)milliseconds);
            var result = new Amf3.DateBlock(value);
            ObjectPool.Add(result);
            return result;
        }

        private Amf3.XmlDocBlock ReadXmlDocBlock()
        {
            var v = ReadInt();
            if (!v.Flags[0]) return (Amf3.XmlDocBlock) ObjectPool[v.Values[1]];
            var result = new Amf3.XmlDocBlock(ReadString(v.Values[1]));
            ObjectPool.Add(result);

            return result;
        }

        private Amf3.ArrayBlock ReadArrayBlock()
        {
            var val = ReadInt();

            if (!val.Flags[0])
            {
                return ((Amf3.ArrayBlock)ObjectPool[val.Values[1]]);
            }
            var key = ReadString();
            var assoc = new Dictionary<string, Amf3.Amf3Block>();
            while (!string.IsNullOrEmpty(key))
            {
                assoc[key] = (Amf3.Amf3Block)ReadBlock(key);
                key = ReadString();
            }
            var value = new List<Amf3.Amf3Block>();
            for (var i = 0; i < val.Values[1]; ++i)
            {
                value.Add((Amf3.Amf3Block)ReadBlock(i.ToString()));
            }
            var result = new Amf3.ArrayBlock(value, assoc);
            ObjectPool.Add(result);
            return result;
        }
    }
}
