using NLog;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using System.Linq;

namespace LibraryParser
{
    public class SkyReader
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string lastPeptideRead = String.Empty;

        public Library LoadLibrary(string path)
        {
            logger.Info("Loading file: {0}", path);

            Library library = new Library();
            using (XmlReader reader = XmlReader.Create(path))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.LocalName)
                        {
                            case "Protein":
                                AddProtein(library, reader);
                                break;
                            case "Peptide":
                                lastPeptideRead = reader.GetAttribute("id");
                                AddPeptide(library, reader);
                                break;
                            case "Transition":
                                AddTransition(library, reader);
                                break;
                        }
                    }
                }
                logger.Debug("{0} proteins loaded", library.ProteinList.Count);
                logger.Debug("{0} unique uniprot IDs", library.UniprotIdList.Count);
                logger.Debug("{0} decoy proteins loaded", library.ProteinDecoyList.Count);
                logger.Debug("{0} peptides loaded", library.PeptideList.Count);
                logger.Debug("{0} IRT peptides loaded", library.RtList.Count);
                logger.Debug("{0} transitions loaded", library.TransitionList.Count);
            }
            System.IO.File.WriteAllLines("proteins.txt", library.UniprotIdList.Values.Cast<string>());
            return library;
        }

        private void AddProtein(Library library, XmlReader xmlReader)
        {
            var protein = new Library.Protein();
            protein.Id = xmlReader.GetAttribute("name");
            protein.AssociatedPeptideIds = new List<string>();
            if (protein.Id.StartsWith("DECOY"))
            {
                library.ProteinDecoyList.Add(protein.Id, protein);
            }
            else
            {
                library.ProteinList.Add(protein.Id, protein);
                StoreUniprotIds(library, protein.Id);
            }
        }

        private void AddPeptide(Library library, XmlReader reader)
        {
            var peptide = new Library.Peptide();
            peptide.Id = reader.GetAttribute("name");
            peptide.Sequence = reader.GetAttribute("sequence");
            peptide.AssociatedTransitionIds = new List<string>();
            peptide.ChargeState = Convert.ToInt32(reader.GetAttribute("charge"));
            peptide.RetentionTime = Convert.ToInt32(reader.GetAttribute("ave_retention"));                    

            library.PeptideList.Add(peptide.Id, peptide);
        }
       
        private void AddTransition(Library library, XmlReader reader)
        {
            var transition = new Library.Transition();
            transition.PeptideId = reader.GetAttribute("precursor_mz");
            transition.Id = reader.GetAttribute("product_mz");
            transition.ProductMz = Convert.ToDouble(reader.GetAttribute("product_mz"));
            transition.PrecursorMz = Convert.ToDouble(reader.GetAttribute("precursor_mz"));
            transition.ProductIonChargeState = Convert.ToInt32(reader.GetAttribute("product_charge"));
            transition.ProductIonSeriesOrdinal = Convert.ToInt32(reader.GetAttribute("fragment_ordinal"));
            transition.IonType = reader.GetAttribute("fragment_type");
            transition.ProductIonIntensity = Convert.ToDouble(reader.GetAttribute("height"));
            library.TransitionList.Add(transition.Id, transition);
            var correspondingPeptide = (Library.Peptide)(library.PeptideList[transition.PeptideId]);
            correspondingPeptide.AssociatedTransitionIds.Add(transition.Id);
        }

        private void StoreUniprotIds(Library library, string proteinId)
        {
            //Regular expression from https://www.uniprot.org/help/accession_numbers
            //I don't think this matches all cases - some seem to have shorter strings
            Match matches = Regex.Match(proteinId, "[OPQ][0-9][A-Z0-9]{3}[0-9]|[A-NR-Z][0-9]([A-Z][A-Z0-9]{2}[0-9]){1,2}");
            foreach (Group match in matches.Groups)
            {
                if (match.Success && library.UniprotIdList[proteinId] == null)
                {
                    library.UniprotIdList.Add(proteinId, match.Value);
                }
            }
        }
    }
}
