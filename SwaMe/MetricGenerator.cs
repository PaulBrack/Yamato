using MzmlParser;
using System.IO;


namespace SwaMe
{
    public class MetricGenerator
    {
        public void GenerateMetrics(Run run)
        {
            //Put a breakpoint somewhere in this method to explore the run object
            StreamWriter sw = new StreamWriter("C:\\Users\\pauwmarina\\Desktop\\HannoSwath\\Text.txt");

            //Write a line of text
            sw.WriteLine("Hello World!!");

            //Write a second line of text
            sw.WriteLine("From the StreamWriter class");

            //Close the file
            sw.Close();
        }
    }
}
