using System;
using System.Collections.Generic;
using System.Linq;

namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.Object)]
    public class ObjectBlock : Amf3Block<Dictionary<string, AmfBlock>>, IEquatable<ObjectBlock>
    {
        public ObjectBlock()
        {
        }
        
        public ObjectBlock(string className, List<string> traits, Dictionary<string, AmfBlock> value) : base(value)
        {
            ClassName = className;
            Traits = traits;
        }

        public string ClassName { get; set; }
        public List<string> Traits { get; set; }

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