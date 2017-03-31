using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace SolJson.Amf3
{
    public abstract class Amf3Block
    {
        public static List<Type> BlockTypes { get; }
        public static Dictionary<Amf3BlockType, ConstructorInfo> Blocks { get; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public virtual Amf3BlockType Type { get; set; }

        protected Amf3Block()
        {
        }

        [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
        protected Amf3Block(string name, Amf3BlockType type, Amf3Reader reader)
        {
            Name = name;
            Type = type;
            if (reader != null)
            {
                ReadValue(reader);
            }
        }

        protected abstract void ReadValue(Amf3Reader reader);
        public abstract void WriteValue(Amf3Writer writer);

        static Amf3Block()
        {
            Blocks = new Dictionary<Amf3BlockType, ConstructorInfo>();
            BlockTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(IsAmf3BlockType).ToList();
            foreach (var type in BlockTypes)
            {
                foreach (var blockType in GetBlockType(type))
                {
                    Blocks[blockType] = GetConstructor(type);
                }
            }
        }

        private static ConstructorInfo GetConstructor(Type t)
        {
            return t.GetConstructor(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance, null,
                new[] { typeof(string), typeof(Amf3BlockType), typeof(Amf3Reader) }, new ParameterModifier[0]);
        }

        private static bool IsAmf3BlockType(Type t)
        {
            if (!t.IsSubclassOf(typeof(Amf3Block))) return false;

            return GetBlockType(t).Any();
        }

        private static Amf3BlockType[] GetBlockType(MemberInfo t)
        {
            return t.GetCustomAttributes<Amf3Attribute>().Select(a => a.Type).ToArray();
        }
    }

    [DebuggerDisplay("{Name}: {Value}")]
    public abstract class Amf3Block<T> : Amf3Block
    {
        public T Value { get; set; }

        protected Amf3Block()
        {
        }

        protected Amf3Block(string name, Amf3BlockType type, Amf3Reader reader) : base(name, type, reader)
        {
        }
    }
}
