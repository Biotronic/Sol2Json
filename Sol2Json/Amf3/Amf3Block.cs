using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SolJson.Amf3
{
    public abstract class Amf3Block : AmfBlock
    {
        public virtual Amf3BlockType Type { get; set; }
    }

    [DebuggerDisplay("{Name}: {Value}")]
    public abstract class Amf3Block<T> : Amf3Block, IEquatable<Amf3Block<T>>
    {
        public T Value { get; set; }

        protected Amf3Block()
        {
        }

        protected Amf3Block(T value)
        {
            Value = value;
        }

        public bool Equals(Amf3Block<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            var v1 = Value as IEnumerable;
            var v2 = other.Value as IEnumerable;
            if (v1 != null && v2 != null)
            {
                return v1.OfType<object>().SequenceEqual(v2.OfType<object>());
            }
            return EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Amf3Block<T>) obj);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode(Value);
        }

        public static bool operator ==(Amf3Block<T> left, Amf3Block<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Amf3Block<T> left, Amf3Block<T> right)
        {
            return !Equals(left, right);
        }
    }
}
