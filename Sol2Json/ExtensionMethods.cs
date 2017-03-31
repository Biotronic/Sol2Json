using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SolJson
{
    public static class ExtensionMethods
    {
        public static IEnumerable<T> Recurse<T>(this T root, Func<T, T> fn)
        {
            while (true)
            {
                yield return root;
                root = fn(root);
            }
        }

        public static ushort ChangeEndianness(this ushort value)
        {
            return (ushort)((ushort)((value & 0xff) << 8) | ((value >> 8) & 0xff));
        }

        public static uint ChangeEndianness(this uint value)
        {
            return ((value & 0x000000ff) << 24) +
                   ((value & 0x0000ff00) << 8) +
                   ((value & 0x00ff0000) >> 8) +
                   ((value & 0xff000000) >> 24);
        }

        public static ulong ChangeEndianness(this ulong value)
        {
            value = (value >> 32) | (value << 32);
            value = ((value & 0xFFFF0000FFFF0000) >> 16) | ((value & 0x0000FFFF0000FFFF) << 16);
            return ((value & 0xFF00FF00FF00FF00) >> 8) | ((value & 0x00FF00FF00FF00FF) << 8);
        }

        public static SerializableDictionary<TKey, TValue> ToSerializableDictionary<T, TKey, TValue>(
            this IEnumerable<T> source, Func<T, TKey> keySelector, Func<T, TValue> valueSelector)
        {
            var result= new SerializableDictionary<TKey,TValue>();

            foreach (var v in source)
            {
                result[keySelector(v)] = valueSelector(v);
            }

            return result;
        }
    }

    [DebuggerDisplay("{Values[0]}/{Values[1]}/{Flags[0]}")]
    public struct FlaggedInt
    {
        public FlaggedInt(int value)
        {
            RawValue = value;
        }
        
        private int RawValue { get; }
        public bool[] Flags
        {
            get
            {
                var tmp = RawValue;
                var result = new bool[29];

                var i = 0;
                while (tmp > 0)
                {
                    result[i] = (tmp & 1) > 0;
                    i++;
                    tmp >>= 1;
                }

                return result;
            }
        }
        public int[] Values
        {
            get
            {
                var tmp = RawValue;
                var result = new int[29];

                var i = 0;
                while (i < result.Length)
                {
                    result[i] = tmp;
                    i++;
                    tmp >>= 1;
                }

                return result;
            }
        }
    }
}