using System;
using System.Collections.Generic;
using System.Linq;

namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.Object)]
    public class ObjectBlock : Amf3Block<SerializableDictionary<string, Amf3Block>>, IEquatable<ObjectBlock>
    {
        protected ObjectBlock()
        {
            ClassName = string.Empty;
        }

        public string ClassName { get; set; }
        public List<string> Traits { get; set; }

        protected ObjectBlock(string name, Amf3BlockType type, Amf3Reader reader) : base(name, type, reader)
        {
        }

        protected override void ReadValue(Amf3Reader reader)
        {
            var val = reader.ReadInt();
            if (!val.Flags[0])
            {
                // U29O-ref
                Value = ((ObjectBlock)reader.ObjectPool[val.Values[1]]).Value;
                Traits = ((ObjectBlock)reader.ObjectPool[val.Values[1]]).Traits;
                ClassName = ((ObjectBlock)reader.ObjectPool[val.Values[1]]).ClassName;
            }
            else if (!val.Flags[1])
            {
                // U29O-traits-ref
                Traits = ((ObjectBlock)reader.ObjectTraitsPool[val.Values[2]]).Traits;
                ClassName = ((ObjectBlock)reader.ObjectTraitsPool[val.Values[2]]).ClassName;
                Value = new SerializableDictionary<string, Amf3Block>();
                foreach (var trait in Traits)
                {
                    Value[trait] = Amf3Reader.Read(reader, trait);
                }
            }
            else if (val.Flags[2])
            {
                // U29O-traits-ext
                ClassName = reader.ReadString();
                throw new NotImplementedException("Cannot deserialize random data. Sorry. :(");
            }
            else
            {
                // U29O-traits
                var isDynamic = val.Flags[3];
                var count = val.Values[4];
                ClassName = reader.ReadString();

                Value = new SerializableDictionary<string, Amf3Block>();
                Traits = new List<string>();
                for (var i = 0; i < count; ++i)
                {
                    Traits.Add(reader.ReadString());
                }
                for (var i = 0; i < count; ++i)
                {
                    Value[Traits[i]] = Amf3Reader.Read(reader, Traits[i]);
                }
                reader.ObjectPool.Add(this);
                reader.ObjectTraitsPool.Add(this);
            }
            var name = reader.ReadString();
            while (!string.IsNullOrEmpty(name))
            {
                Value[name] = Amf3Reader.Read(reader, name);
                name = reader.ReadString();
            }
        }

        public override void WriteValue(Amf3Writer writer)
        {
            var index = writer.FindReference(this);
            if (index > -1)
            {
                // U29O-ref
                writer.WriteInt(index, false);
                return;
            }
            index = writer.FindTraits(this);
            if (index > -1)
            {
                // U29O-traits-ref
                writer.WriteInt(index, true, false);
            }
            else
            {
                // U29O-traits
                writer.WriteInt(Traits.Count, true, true, false, Value.Count > Traits.Count);
                writer.WriteString(ClassName);
                foreach (var v in Traits)
                {
                    writer.WriteString(v);
                }
                writer.ObjectTraitsPool.Add(this);
            }
            foreach (var v in Traits)
            {
                writer.Write(Value[v]);
            }
            foreach (var v in Value)
            {
                if (Traits.Contains(v.Key)) continue;

                writer.WriteString(v.Key);
                writer.Write(v.Value);
            }
            writer.WriteString("");
            writer.ObjectPool.Add(this);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ObjectBlock)obj);
        }

        public bool Equals(ObjectBlock other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(ClassName, other.ClassName) && Traits.Equals(other.Traits) && Value.SequenceEqual(other.Value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (((ClassName.GetHashCode() * 397) ^ Traits.GetHashCode()) * 397) ^ Value.GetHashCode();
            }
        }

        public static bool operator ==(ObjectBlock left, ObjectBlock right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ObjectBlock left, ObjectBlock right)
        {
            return !Equals(left, right);
        }
    }
}