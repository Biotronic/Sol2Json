using System;
using System.Collections.Generic;
using System.Reflection;

namespace SolJson
{
    public class AmfBlock
    {
        public string Name { get; set; }

        private static readonly Dictionary<Amf0.Amf0BlockType, Type> Amf0BlockTypes = new Dictionary<Amf0.Amf0BlockType, Type>();
        private static readonly Dictionary<Amf3.Amf3BlockType, Type> Amf3BlockTypes = new Dictionary<Amf3.Amf3BlockType, Type>();

        static AmfBlock()
        {
            var types = Assembly.GetCallingAssembly().DefinedTypes;
            foreach (var blockType in types)
            {
                if (!blockType.IsSubclassOf(typeof(AmfBlock))) continue;
                var v2 = blockType.GetCustomAttributes<Amf3.Amf3Attribute>();
                foreach (var attr in v2)
                {
                    Amf3BlockTypes[attr.Type] = blockType;
                }
                var v3 = blockType.GetCustomAttributes<Amf0.Amf0Attribute>();
                foreach (var attr in v3)
                {
                    Amf0BlockTypes[attr.Type] = blockType;
                }
            }
        }

        public static Amf3.Amf3Block CreateBlock(Amf3.Amf3BlockType type, string name)
        {
            if (!Amf3BlockTypes.ContainsKey(type))
            {
                throw new ArgumentOutOfRangeException();
            }

            var result = (Amf3.Amf3Block)Activator.CreateInstance(Amf3BlockTypes[type]);
            result.Type = type;
            result.Name = name;
            return result;
        }

        public static Amf0.Amf0Block CreateBlock(Amf0.Amf0BlockType type, string name)
        {
            if (!Amf0BlockTypes.ContainsKey(type))
            {
                throw new ArgumentOutOfRangeException();
            }

            var result = (Amf0.Amf0Block)Activator.CreateInstance(Amf0BlockTypes[type]);
            result.Type = type;
            result.Name = name;
            return result;
        }
    }
}
