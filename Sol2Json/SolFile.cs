using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace SolJson
{
    public class SolFile
    {
        public List<AmfBlock> Blocks { get; set; }

        public SolFile()
        {
            Blocks = new List<AmfBlock>();
        }

        public static SolFile FromSol(string filename)
        {
            var result = new SolFile();
            using (var s = File.OpenRead(filename))
            using (var reader = new AmfReader(s))
            {
                reader.ReadHeader();
                while (s.Position < s.Length)
                {
                    var name = reader.ReadString();
                    var block = reader.ReadBlock(name);
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
            using (var writer = new AmfWriter(s, Path.GetFileNameWithoutExtension(filename)))
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
