using NLog;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
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
                            case "protein":
                                AddProtein(library, reader);
                                break;
                            case "peptide":
                                lastPeptideRead = reader.GetAttribute("id");
                                AddPeptide(library, reader);
                                break;
                            case "transition":
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
            bool peptideRead = false;

            while (reader.Read() && !peptideRead)
            {
                if (reader.IsStartElement())
                {

                    peptide.AssociatedTransitionIds = new List<string>();
                    if (reader.LocalName == "precursor")
                    {
                        peptide.ChargeState = Convert.ToInt32(reader.GetAttribute("charge"));
                        peptide.Id = reader.GetAttribute("precursor_mz");
                    }
                    else if (reader.LocalName == "peptide")
                    {
                        peptide.Sequence = reader.GetAttribute("sequence");

                        peptide.RetentionTime = Convert.ToInt32(reader.GetAttribute("ave_retention_time"));
                    }
                    else if (reader.LocalName == "collision_energy")
                    {

                        peptide.CollisionEnergy = Convert.ToDouble(reader.GetAttribute("collision_energy"));
                    }

                    }
                else if ( reader.LocalName == "transition")
                {
                    peptideRead = true;
                }
            }

            library.PeptideList.Add(peptide.Id, peptide);
        }

        private void AddPeptideReference(Library library, XmlReader xmlReader)
        {
            string proteinRef = xmlReader.GetAttribute("ref");
            if (!proteinRef.StartsWith("DECOY"))
            {
                Library.Protein correspondingProtein = (Library.Protein)(library.ProteinList[proteinRef]);
                correspondingProtein.AssociatedPeptideIds.Add(lastPeptideRead);
            }
            else
            {
                Library.Protein correspondingProtein = (Library.Protein)(library.ProteinDecoyList[proteinRef]);
                correspondingProtein.AssociatedPeptideIds.Add(lastPeptideRead);
            }
        }

        private void AddTransition(Library library, XmlReader reader)
        {
            var transition = new Library.Transition();
            bool transRead = false;
            while (reader.Read() && !transRead)
            {
                if (reader.IsStartElement())
                {
                    if (reader.LocalName == "precursor_mz")
                    {
                        transition.PeptideId = reader.GetAttribute("precursor_mz");
                        transition.PrecursorMz = Convert.ToDouble(reader.GetAttribute("precursor_mz"));
                    }
                    else if (reader.LocalName == "transition")
                    {
                        transition.Id = String.Concat(reader.GetAttribute("fragment_type") , reader.GetAttribute("fragment_ordinal"));
                        transition.IonType = reader.GetAttribute("fragment_type");
                        transition.ProductIonSeriesOrdinal = Convert.ToInt32(reader.GetAttribute("fragment_ordinal"));
                        transition.ProductIonChargeState = Convert.ToInt32(reader.GetAttribute("product_charge"));
                    }
                    
                    else if (reader.LocalName == "product_mz")
                    {
                        transition.ProductMz = Convert.ToDouble(reader.GetAttribute("product_mz"));
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "transition")
                {
                    transRead = true;
                }
            }

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
