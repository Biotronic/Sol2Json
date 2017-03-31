using System;
using System.IO;

namespace SolJson
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            SolFile sol;
            string destination;
            if (Path.GetExtension(args[0])?.ToLower() == ".sol")
            {
                sol = SolFile.FromSol(args[0]);
                destination = args.Length > 1 ? args[1] : Path.ChangeExtension(args[0], ".json");
            }
            else if (Path.GetExtension(args[0])?.ToLower() == ".json")
            {
                sol = SolFile.FromJson(args[0]);
                destination = args.Length > 1 ? args[1] : Path.ChangeExtension(args[0], ".sol");
            }
            else
            {
                throw new Exception();
            }

            if (Path.GetExtension(destination)?.ToLower() == ".sol")
            {
                sol.ToSol(destination);
            }
            else if (Path.GetExtension(destination)?.ToLower() == ".json")
            {
                sol.ToJson(destination);
            }
            else
            {
                throw new Exception();
            }
        }
    }
}
