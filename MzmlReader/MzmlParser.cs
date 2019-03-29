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

        //public Library LoadLibrary(string path)
        //{
        //    logger.Info("Loading file: {0}", path);

        //    Library library = new Library();
        //    using (XmlReader reader = XmlReader.Create(path))
        //    {
        //        while (reader.Read())
        //        {
        //            if (reader.IsStartElement())
        //            {
        //                switch (reader.LocalName)
        //                {
        //                    case "Protein":
        //                        AddProtein(library, reader);
        //                        break;
        //                    case "ProteinRef":
        //                        AddPeptideReference(library, reader);
        //                        break;
        //                    case "Peptide":
        //                        lastPeptideRead = reader.GetAttribute("id");
        //                        AddPeptide(library, reader);
        //                        break;
        //                    case "Transition":
        //                        AddTransition(library, reader);
        //                        break;
        //                }
        //            }
        //        }
        //        logger.Debug("{0} proteins loaded", library.ProteinList.Count);
        //        logger.Debug("{0} unique uniprot IDs", library.UniprotIdList.Count);
        //        logger.Debug("{0} decoy proteins loaded", library.ProteinDecoyList.Count);
        //        logger.Debug("{0} peptides loaded", library.PeptideList.Count);
        //        logger.Debug("{0} IRT peptides loaded", library.RtList.Count);
        //        logger.Debug("{0} transitions loaded", library.TransitionList.Count);
        //    }
        //    System.IO.File.WriteAllLines("proteins.txt", library.UniprotIdList.Values.Cast<string>());
        //    return library;
        //}
    }
}
