using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace SolJson
{
    public class AmfWriter : IDisposable
    {
        private Stream Stream { get; }
        public AmfVersion Version { get; set; }
        private BinaryWriter Writer { get; }
        private List<string> StringPool { get; }
        private List<AmfBlock> ObjectPool { get; }
        private List<List<string>> TraitsPool { get; }
        private List<AmfBlock> References { get; }
        private bool Closed { get; set; }

        public AmfWriter(Stream stream, AmfVersion version = AmfVersion.Amf3)
        {
            Stream = stream;
            Writer = new BinaryWriter(stream);
            ObjectPool = new List<AmfBlock>();
            StringPool = new List<string>();
            TraitsPool = new List<List<string>>();
            References = new List<AmfBlock>();
            Version = version;
        }


        public void WriteHeader(string name)
        {
            Writer.Write((ushort)0xBF00);
            Writer.Write(0);
            Writer.Write(0x4F534354);
            Writer.Write(new byte[] { 0, 4, 0, 0, 0, 0 });
            Writer.Write(((ushort)name.Length).ChangeEndianness());
            Writer.Write(name.ToCharArray());
            Writer.Write(((uint)Version).ChangeEndianness());
        }

        public void Close()
        {
            Stream.Seek(2, SeekOrigin.Begin);
            var size = (uint)(Stream.Length - 6);
            Writer.Write(size.ChangeEndianness());
            Writer.Write(0x4F534354);
            Closed = true;
        }

        public void WriteString(string value)
        {
            if (Version == AmfVersion.Amf0) WriteAmf0ShortString(value);
            if (Version == AmfVersion.Amf3) WriteAmf3String(value);
        }

        private void WriteAmf3String(string value)
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

        private void WriteAmf0ShortString(string value)
        {
            var tmp = Encoding.UTF8.GetBytes(value);
            Debug.Assert(tmp.Length < 65536);
            WriteInt16((short)tmp.Length);
            Writer.Write(tmp);
        }

        private void WriteAmf0LongString(string value)
        {
            var tmp = Encoding.UTF8.GetBytes(value);
            WriteInt32(tmp.Length);
            Writer.Write(tmp);
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

        public void WriteInt16(short value)
        {
            Writer.Write(value.ChangeEndianness());
        }

        public void WriteInt32(int value)
        {
            Writer.Write(value.ChangeEndianness());
        }

        public void WriteUInt32(uint value)
        {
            Writer.Write(value.ChangeEndianness());
        }

        public void WriteByte(byte value)
        {
            Writer.Write(value);
        }

        public void WriteDouble(double value)
        {
            var data = BitConverter.DoubleToInt64Bits(value);
            Writer.Write(data.ChangeEndianness());
        }

        public void WriteBytes(List<byte> value)
        {
            Writer.Write(value.ToArray());
        }

        public void WriteBlockType(Amf3.Amf3BlockType value)
        {
            Writer.Write((byte)value);
        }

        private void WriteBlockType(Amf0.Amf0BlockType value)
        {
            Writer.Write((byte)value);
        }

        public void Dispose()
        {
            Debug.Assert(Closed, "Need to close before disposing.");
            Writer?.Dispose();
        }

        public int FindReference<T>(Amf3.Amf3Block<T> block)
        {
            return ObjectPool.Select(Tuple.Create<AmfBlock, int>)
                       .FirstOrDefault(a => (a.Item1 as Amf3.Amf3Block<T>) == block)?.Item2 ?? -1;
        }

        private short FindAmf0Reference(AmfBlock value)
        {
            return (short)References.FindIndex(a => a == value);
        }

        public int FindTraits(Amf3.ObjectBlock block)
        {
            return TraitsPool.Select(Tuple.Create<List<string>, int>)
                       .FirstOrDefault(a => a.Item1.SequenceEqual(block.Traits))?.Item2 ?? -1;
        }

        public void Write(AmfBlock value)
        {
            var v = GetType()
                    .GetMethods()
                    .First(a => a.Name == "Write" && a.GetParameters()[0].ParameterType == value.GetType());

            v.Invoke(this, new object[] { value });
        }

        public void Write(Amf0.NumberBlock value)
        {
            WriteBlockType(value.Type);
            WriteDouble(value.Value);
        }

        public void Write(Amf0.BooleanBlock value)
        {
            WriteBlockType(value.Type);
            WriteByte((byte)(value.Value ? 1 : 0));
        }

        public void Write(Amf0.StringBlock value)
        {
            WriteBlockType(value.Type);
            WriteAmf0ShortString(value.Value);
        }

        public void Write(Amf0.ObjectBlock value)
        {
            WriteBlockType(value.Type);
        }

        public void Write(Amf0.NullBlock value)
        {
            WriteBlockType(value.Type);
        }

        public void Write(Amf0.UndefinedBlock value)
        {
            WriteBlockType(value.Type);
        }

        public void Write(Amf0.ReferenceBlock value)
        {
            WriteBlockType(value.Type);
            var tmp = FindAmf0Reference(value.Value);
            WriteInt16(tmp);
        }

        public void Write(Amf0.EcmaArrayBlock value)
        {
            WriteBlockType(value.Type);
            WriteInt32(value.Value.Count);
            foreach (var v in value.Value)
            {
                WriteAmf0ShortString(v.Key);
                Write(v.Value);
            }
        }

        public void Write(Amf0.ObjectEndBlock value)
        {
            WriteBlockType(value.Type);
        }

        public void Write(Amf0.StrictArrayBlock value)
        {
            WriteBlockType(value.Type);
            WriteInt32(value.Value.Count);
            foreach (var v in value.Value)
            {
                Write(v);
            }
        }

        public void Write(Amf0.DateBlock value)
        {
            WriteBlockType(value.Type);
            WriteInt16(0);
            WriteDouble((value.Value - new DateTime(1970,1,1)).TotalMilliseconds);
        }

        public void Write(Amf0.LongStringBlock value)
        {
            WriteBlockType(value.Type);
            WriteAmf0LongString(value.Value);
        }

        public void Write(Amf0.UnsupportedBlock value)
        {
            WriteBlockType(value.Type);
        }

        public void Write(Amf0.XmlDocumentBlock value)
        {
            WriteBlockType(value.Type);
            WriteAmf0LongString(value.Value);
        }

        public void Write(Amf0.TypedObjectBlock value)
        {
            WriteBlockType(value.Type);
            WriteAmf0ShortString(value.ClassName);
            foreach (var v in value.Value)
            {
                WriteAmf0ShortString(v.Key);
                Write(v.Value);
            }
            WriteBlockType(Amf0.Amf0BlockType.ObjectEnd);
        }

        public void Write(Amf3.StringBlock value)
        {
            WriteBlockType(value.Type);
            WriteAmf3String(value.Value);
        }

        public void Write(Amf3.BooleanBlock value)
        {
            WriteBlockType(value.Type);
        }

        public void Write(Amf3.DoubleBlock value)
        {
            WriteBlockType(value.Type);
            WriteDouble(value.Value);
        }

        public void Write(Amf3.IntegerBlock value)
        {
            WriteBlockType(value.Type);
            WriteInt(value.Value);
        }

        public void Write(Amf3.NullBlock value)
        {
            WriteBlockType(value.Type);
        }

        public void Write(Amf3.UndefinedBlock value)
        {
            WriteBlockType(value.Type);
        }

        public void Write(Amf3.XmlBlock value)
        {
            WriteBlockType(value.Type);
            var index = FindReference(value);
            if (index < 0)
            {
                WriteInt(index, false);
                return;
            }
            var val = Encoding.UTF8.GetBytes(value.Value);
            WriteInt(val.Length, true);
            Writer.Write(val);
        }

        public void Write(Amf3.XmlDocBlock value)
        {
            WriteBlockType(value.Type);
            var index = FindReference(value);
            if (index < 0)
            {
                WriteInt(index, false);
                return;
            }
            var val = Encoding.UTF8.GetBytes(value.Value);
            WriteInt(val.Length, true);
            Writer.Write(val);
        }

        public void Write(Amf3.DateBlock value)
        {
            WriteBlockType(value.Type);
            var index = FindReference(value);
            WriteInt(index, index < 0);
            if (index == -1)
            {
                WriteDouble((value.Value - new DateTime(1970, 1, 1)).TotalMilliseconds);
                ObjectPool.Add(value);
            }
        }

        public void Write(Amf3.ObjectBlock value)
        {
            WriteBlockType(value.Type);
            var index = FindReference(value);
            if (index > -1)
            {
                // U29O-ref
                WriteInt(index, false);
                return;
            }
            index = FindTraits(value);
            if (index > -1)
            {
                // U29O-traits-ref
                WriteInt(index, true, false);
            }
            else
            {
                // U29O-traits
                WriteInt(value.Traits.Count, true, true, false, value.Value.Count > value.Traits.Count);
                WriteAmf3String(value.ClassName);
                foreach (var v in value.Traits)
                {
                    WriteAmf3String(v);
                }
                TraitsPool.Add(value.Traits);
            }
            foreach (var v in value.Traits)
            {
                Write(value.Value[v]);
            }
            foreach (var v in value.Value)
            {
                if (value.Traits.Contains(v.Key)) continue;

                WriteAmf3String(v.Key);
                Write(v.Value);
            }
            WriteAmf3String("");
            ObjectPool.Add(value);
        }

        public void Write(Amf3.DictionaryBlock value)
        {
            WriteBlockType(value.Type);
            var index = FindReference(value);
            if (index > -1)
            {
                WriteInt(index, false);
            }
            else
            {
                WriteInt(value.Value.Count, true);
                WriteByte((byte)(value.WeakReferences ? 1 : 0));
                foreach (var block in value.Value)
                {
                    Write(block.Key);
                    Write(block.Value);
                }
                ObjectPool.Add(value);
            }
        }

        public void Write(Amf3.ArrayBlock value)
        {
            WriteBlockType(value.Type);
            var index = FindReference(value);
            if (index > -1)
            {
                WriteInt(index, false);
            }
            else
            {
                WriteInt(value.Value.Count, true);
                if (value.Associative != null)
                {
                    foreach (var v in value.Associative)
                    {
                        WriteAmf3String(v.Key);
                        Write(v.Value);
                    }
                }
                WriteAmf3String("");
                foreach (var v in value.Value)
                {
                    Write(v);
                }
                ObjectPool.Add(value);
            }
        }

        public void Write(Amf3.VectorObjectBlock value)
        {
            WriteBlockType(value.Type);
            var index = FindReference(value);
            if (index > -1)
            {
                WriteInt(index, false);
            }
            else
            {
                WriteInt(value.Value.Count, true);
                WriteByte((byte)(value.FixedLength ? 1 : 0));
                WriteAmf3String("");
                foreach (var v in value.Value)
                {
                    Write(v);
                }
                ObjectPool.Add(value);
            }
        }

        public void Write(Amf3.VectorDoubleBlock value)
        {
            WriteBlockType(value.Type);
            var index = FindReference(value);
            if (index > -1)
            {
                WriteInt(index, false);
            }
            else
            {
                WriteInt(value.Value.Count, true);
                WriteByte((byte)(value.FixedLength ? 1 : 0));
                foreach (var v in value.Value)
                {
                    WriteDouble(v);
                }
                ObjectPool.Add(value);
            }
        }

        public void Write(Amf3.VectorUIntBlock value)
        {
            WriteBlockType(value.Type);
            var index = FindReference(value);
            if (index > -1)
            {
                WriteInt(index, false);
            }
            else
            {
                WriteInt(value.Value.Count, true);
                WriteByte((byte)(value.FixedLength ? 1 : 0));
                foreach (var v in value.Value)
                {
                    WriteUInt32(v);
                }
                ObjectPool.Add(value);
            }
        }

        public void Write(Amf3.VectorIntBlock value)
        {
            WriteBlockType(value.Type);
            var index = FindReference(value);
            if (index > -1)
            {
                WriteInt(index, false);
            }
            else
            {
                WriteInt(value.Value.Count, true);
                WriteByte((byte)(value.FixedLength ? 1 : 0));
                foreach (var v in value.Value)
                {
                    WriteInt32(v);
                }
                ObjectPool.Add(value);
            }
        }

        public void Write(Amf3.ByteArrayBlock value)
        {
            WriteBlockType(value.Type);
            var index = FindReference(value);
            if (index > -1)
            {
                WriteInt(index, false);
            }
            else
            {
                WriteInt(value.Value.Count, true);
                WriteBytes(value.Value);
                ObjectPool.Add(value);
            }
        }
    }
}
