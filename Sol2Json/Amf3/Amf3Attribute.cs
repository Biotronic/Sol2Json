using System;

namespace SolJson.Amf3
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class Amf3Attribute : Attribute
    {
        public Amf3BlockType Type { get; }

        public Amf3Attribute(Amf3BlockType type)
        {
            Type = type;
        }
    }
}