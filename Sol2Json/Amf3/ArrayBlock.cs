using System;
using System.Collections.Generic;

namespace SolJson.Amf3
{
    [Amf3(Amf3BlockType.Array)]
    public class ArrayBlock : Amf3Block<List<Amf3Block>>, IEquatable<ArrayBlock>
    {
        public ArrayBlock()
        {
        }

        public ArrayBlock(List<Amf3Block> value, Dictionary<string, Amf3Block> assoc) : base(value)
        {
            Associative = assoc;
        }

        public Dictionary<string, Amf3Block> Associative { get; set; }

        public bool Equals(ArrayBlock other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(Associative, other.Associative);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ArrayBlock) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (Associative?.GetHashCode() ?? 0);
            }
        }

        public static bool operator ==(ArrayBlock left, ArrayBlock right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ArrayBlock left, ArrayBlock right)
        {
            return !Equals(left, right);
        }
    }
}