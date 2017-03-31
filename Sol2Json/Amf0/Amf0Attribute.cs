using System;

namespace SolJson.Amf0
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class Amf0Attribute : Attribute
    {
        public Amf0BlockType Type { get; }

        public Amf0Attribute(Amf0BlockType type)
        {
            Type = type;
        }
    }
}
