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
        public FileHeader Header { get; set; }
        public List<string> StringPool { get; }
        public List<AmfBlock> ObjectPool { get; }
        public List<List<string>> TraitsPool { get; }
        public List<AmfBlock> References { get; }


        public AmfReader(Stream stream)
        {
            Reader = new BinaryReader(stream);
            StringPool = new List<string>();
            ObjectPool = new List<AmfBlock>();
            TraitsPool = new List<List<string>>();
            References = new List<AmfBlock>();
        }

        internal string ReadString()
        {
            if (Version == AmfVersion.Amf0) return ReadAmf0String();
            if (Version == AmfVersion.Amf3) return ReadAmf3String();
            throw new ArgumentOutOfRangeException();
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

        public Amf0.Amf0BlockType ReadBlockType0()
        {
            return (Amf0.Amf0BlockType)Reader.ReadByte();
        }

        public Amf3.Amf3BlockType ReadBlockType3()
        {
            return (Amf3.Amf3BlockType)Reader.ReadByte();
        }

        public string ReadAmf3String()
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

        private string ReadAmf0String()
        {
            return ReadString(ReadInt16());
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

        public short ReadInt16()
        {
            return Reader.ReadInt16().ChangeEndianness();
        }

        public int ReadInt32()
        {
            return Reader.ReadInt32().ChangeEndianness();
        }

        public uint ReadUInt32()
        {
            return Reader.ReadUInt32().ChangeEndianness();
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
                Amf0.Amf0Block result;
                var type = ReadBlockType0();

                switch (type)
                {
                    case Amf0.Amf0BlockType.Number:
                        result = new Amf0.NumberBlock(ReadDouble());
                        break;
                    case Amf0.Amf0BlockType.Boolean:
                        result = new Amf0.BooleanBlock(ReadByte() == 1);
                        break;
                    case Amf0.Amf0BlockType.String:
                        result = new Amf0.StringBlock(ReadString(ReadInt16()));
                        break;
                    case Amf0.Amf0BlockType.Object:
                        result = ReadAmf0ObjectBlock();
                        break;
                    case Amf0.Amf0BlockType.Null:
                        result = new Amf0.NullBlock();
                        break;
                    case Amf0.Amf0BlockType.Undefined:
                        result = new Amf0.UndefinedBlock();
                        break;
                    case Amf0.Amf0BlockType.Reference:
                        result = ReadAmf0ReferenceBlock();
                        break;
                    case Amf0.Amf0BlockType.EcmaArray:
                        result = ReadEcmaArray();
                        break;
                    case Amf0.Amf0BlockType.ObjectEnd:
                        result = new Amf0.ObjectEndBlock();
                        break;
                    case Amf0.Amf0BlockType.StrictArray:
                        result = ReadStrictArrayBlock();
                        break;
                    case Amf0.Amf0BlockType.Date:
                        result = ReadAmf0DateBlock();
                        break;
                    case Amf0.Amf0BlockType.LongString:
                        result = new Amf0.StringBlock(ReadAmf0String());
                        break;
                    case Amf0.Amf0BlockType.Unsupported:
                        result = new Amf0.UnsupportedBlock();
                        break;
                    case Amf0.Amf0BlockType.XmlDocument:
                        result = new Amf0.XmlDocumentBlock(ReadAmf0String());
                        break;
                    case Amf0.Amf0BlockType.TypedObject:
                        result = ReadTypedObjectBlock();
                        break;
                    case Amf0.Amf0BlockType.Amf3Transition:
                        result = new Amf0.Amf3TranstionBlock();
                        Version = AmfVersion.Amf3;
                        break;
                    case Amf0.Amf0BlockType.Movieclip:
                    case Amf0.Amf0BlockType.Recordset:
                        throw new NotImplementedException();
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                result.Type = type;
                result.Name = name;
                return result;
            }
            if (Version == AmfVersion.Amf3)
            {
                Amf3.Amf3Block result;
                var type = ReadBlockType3();
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
                        result = new Amf3.StringBlock(ReadAmf3String());
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

        private Amf0.TypedObjectBlock ReadTypedObjectBlock()
        {
            var assoc = new Dictionary<string, AmfBlock>();
            var className = ReadAmf0String();

            while (true)
            {
                var key = ReadAmf0String();
                var value = ReadBlock(key);
                if (key == string.Empty && value is Amf0.ObjectEndBlock) break;
                assoc[key] = value;
            }

            return new Amf0.TypedObjectBlock(assoc, className);
        }

        private Amf0.ObjectBlock ReadAmf0ObjectBlock()
        {
            var assoc = new Dictionary<string, AmfBlock>();

            while (true)
            {
                var key = ReadAmf0String();
                var value = ReadBlock(key);
                if (key == string.Empty && value is Amf0.ObjectEndBlock) break;
                assoc[key] = value;
            }

            return new Amf0.ObjectBlock(assoc);
        }

        private Amf0.DateBlock ReadAmf0DateBlock()
        {
            ReadInt16(); // Time zone
            var offset = ReadDouble();
            return new Amf0.DateBlock(new DateTime(1970,1,1).AddMilliseconds(offset));
        }

        private Amf0.StrictArrayBlock ReadStrictArrayBlock()
        {
            var count = ReadInt32();
            var value = new List<AmfBlock>();
            for (var i = 0; i < count; ++i)
            {
                value.Add(ReadBlock(i.ToString()));
            }

            return new Amf0.StrictArrayBlock(value);
        }

        private Amf0.EcmaArrayBlock ReadEcmaArray()
        {
            var count = ReadInt32();
            var assoc = new Dictionary<string, AmfBlock>();
            for (var i = 0; i < count; ++i)
            {
                var key = ReadAmf0String();
                var value = ReadBlock(key);
                assoc[key] = value;
            }

            return new Amf0.EcmaArrayBlock(assoc);
        }

        private Amf0.ReferenceBlock ReadAmf0ReferenceBlock()
        {
            var index = ReadInt16();
            return new Amf0.ReferenceBlock(References[index]);
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
                className = ReadAmf3String();

                fields = new Dictionary<string, AmfBlock>();
                traits = new List<string>();
                for (var i = 0; i < count; ++i)
                {
                    traits.Add(ReadAmf3String());
                }
                for (var i = 0; i < count; ++i)
                {
                    fields[traits[i]] = ReadBlock(traits[i]);
                }
                TraitsPool.Add(traits);
            }
            var name = ReadAmf3String();
            while (!string.IsNullOrEmpty(name))
            {
                fields[name] = ReadBlock(name);
                name = ReadAmf3String();
            }
            var result = new Amf3.ObjectBlock(className, traits, fields);
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
            var typeName = ReadAmf3String();
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
            var fixedLength = ReadByte() == 1;
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
            var fixedLength = ReadByte() == 1;
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
            var fixedLength = ReadByte() == 1;
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
            if (!v.Flags[0]) return (Amf3.XmlDocBlock)ObjectPool[v.Values[1]];
            var result = new Amf3.XmlDocBlock(ReadString(v.Values[1]));
            ObjectPool.Add(result);

            return result;
        }

        private Amf3.ArrayBlock ReadArrayBlock()
        {
            var val = ReadInt();

            if (!val.Flags[0])
            {
                return (Amf3.ArrayBlock)ObjectPool[val.Values[1]];
            }
            var key = ReadAmf3String();
            var assoc = new Dictionary<string, Amf3.Amf3Block>();
            while (!string.IsNullOrEmpty(key))
            {
                assoc[key] = (Amf3.Amf3Block)ReadBlock(key);
                key = ReadAmf3String();
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