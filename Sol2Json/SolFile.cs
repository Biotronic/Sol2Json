using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using SolJson.Amf3;

namespace SolJson
{
    public class SolFile
    {
        public List<Amf3Block> Blocks { get; set; }

        public SolFile()
        {
            Blocks = new List<Amf3Block>();
        }

        public static SolFile FromSol(string filename)
        {
            var result = new SolFile();
            using (var s = File.OpenRead(filename))
            using (var reader = new Amf3Reader(s))
            {
                while (s.Position < s.Length)
                {
                    var name = reader.ReadString();
                    var block = Amf3Reader.Read(reader, name);
                    var footer = reader.ReadByte();
                    Debug.Assert(footer == 0);
                    result.Blocks.Add(block);
                }
            }
            return result;
        }

        public void ToSol(string filename)
        {
            using (var s = File.OpenWrite(filename))
            using (var writer = new Amf3Writer(s, Path.GetFileNameWithoutExtension(filename)))
            {
                foreach (var v in Blocks)
                {
                    writer.WriteString(v.Name);
                    writer.Write(v);
                    writer.WriteByte(0);
                }
                writer.Close();
            }
        }

        public static SolFile FromXml(string filename)
        {
            using (var stream = File.OpenRead(filename))
            {
                var x = new XmlSerializer(typeof(SolFile), Amf3Block.BlockTypes.ToArray());
                return (SolFile)x.Deserialize(stream);
            }
        }

        public void ToXml(string filename)
        {
            using (var stream = File.OpenWrite(filename))
            {
                var x = new XmlSerializer(GetType(), Amf3Block.BlockTypes.ToArray());
                x.Serialize(stream, this);
            }
        }

        public static SolFile FromJson(string filename)
        {
            return JsonConvert.DeserializeObject<SolFile>(File.ReadAllText(filename), new SolConverter());
        }

        public void ToJson(string filename)
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filename, json);
        }
    }
}
