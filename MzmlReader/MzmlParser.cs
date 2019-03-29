using NLog;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Linq;


namespace MzmlParser
{
    public class MzmlParser
    {
       
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string lastScanRead = String.Empty;

        public void LoadMzml(string path)
        {
            logger.Info("Loading file: {0}", path);
            using (XmlReader reader = XmlReader.Create(path))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.LocalName)
                        {
                            case "Protein":
                                //do summat
                                break;
                        }
                    }
                }
            }
        }
    }
}
