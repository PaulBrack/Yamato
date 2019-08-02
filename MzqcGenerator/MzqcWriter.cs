using Newtonsoft.Json;
using System.IO;

namespace MzqcGenerator
{
    public class MzqcWriter
    {
        public void WriteMzqc (string path, JsonClasses.MzQC metrics)
        {
            using (StreamWriter file = File.CreateText(path))
            {
                file.Write("mzQC:");
                Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Ignore;
                serializer.Serialize(file, metrics);
            }
        }
    }
}
