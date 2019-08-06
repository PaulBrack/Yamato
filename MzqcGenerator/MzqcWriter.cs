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
                file.Write("{ mzQC:");
                JsonSerializer serializer = new JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Ignore;
                serializer.Serialize(file, metrics);
                file.Write("}");
            }
        }

    }

}
