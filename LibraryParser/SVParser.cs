using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;

/* This class is meant to read seperated value files such as tsv/csv that contain iRT peptide information. A template will be provided with the correct 
 headings (same naming convention as OpenMS, tutorial handout 2016 - section 6.4, page 51 {https://www.openms.de/wp-content/uploads/2016/02/handout1.pdf}). 
 It is populated with Biognosys 11 irt peptides to illustrate the concept.
 
 The purpose is to accomdate users that may have a different file format than TraML, for example the Biognosys iRT Kit reference sheet or another format that they
 would prefer to copy to csv rather than TraML. Support is also built in for the Microsoft Excel South African language issue, where excel uses commas
 as decimal points if your country is set to South African. It then seperates the columns using a semi-colon rather than a comma.

 Please note that a TraML file with a more precise indication of the expected retention time will give closer proximation to the retention times picked up for each peptide by Skyline.
 
 Marina - 13092019*/

namespace LibraryParser
{
    public class SVReader : LibraryReader
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private double mzLastPeptide = 0;

        public Library LoadLibrary(string path)
        {
            logger.Info("Loading file: {0}", path);
            string sep = "";
            Library library = new Library();
            IEnumerable <string[]> allLines;
            if (File.ReadLines(path).Contains("//r//n"))
                allLines = File.ReadLines(path).Select(a => a.Split("//r//n"));
            else
                allLines = File.ReadLines(path).Select(a => a.Split("//n"));
            string heading = allLines.ElementAt(0)[0];
            string[] line = { };
            int sequenceIndex = 100;
            int pepMzIndex = 100;
            int transMzIndex = 100;
            int intensityIndex = 100;

            //figure out  which separater is used:
            if (heading.Contains(";"))
            {
                sep = "semi-colon";
                line = heading.Split(";");
            }
            else if (heading.Contains(","))
            {
                sep = "comma";
                line = heading.Split(",");
            }
            else if (heading.Contains("\t")|| heading.Contains(" "))
            {
                sep = "tab";
                line = heading.Split("\t");
            }
            else
            {
                logger.Info("Seperated value file provided for iRT peptides, however the separater could not be established. Please rerun and ensure the file is separated with either a comma, semi-colon or tab.");
                Environment.Exit(0);
            }
            

            
            for (int position = 0; position < line.Count(); position++)
               {
               if (line[position].ToLower().Contains("peptidesequence") || line[position].ToLower().Contains("nominal sequence"))
                    {
                        sequenceIndex = position;
                    }
               else if (line[position].ToLower().Contains("precursormz") || line[position].ToLower().Contains("q1 monoisotopic"))
                    {
                        pepMzIndex = position;
                    }
               else if (line[position].ToLower().Contains("productmz") || line[position].ToLower().Contains("q3"))
                    {
                        transMzIndex = position;
                    }
               else if (line[position].ToLower().Contains("relative intensity") || line[position].ToLower().Contains("libraryintensity"))
                    {
                        intensityIndex = position;
                    }
               }
            if (sequenceIndex == 100 || pepMzIndex == 100 || transMzIndex == 100 || intensityIndex == 100)
            {
                logger.Info("iRT peptides file was provided, but the correct headings could not be found and column could not be distinguished. Please rename your headings as illustrated in the template file.");
                logger.Info("Exiting program");
                Environment.Exit(0);
            }

            for (int iii = 1; iii < allLines.Count(); iii++)
                {
                    string temp = allLines.ElementAt(iii)[0];
                    
                    if (sep== "semi-colon")
                    {
                        line = temp.Split(";");
                    }
                    else if (sep == "comma")
                    {
                        line = temp.Split(",");
                    }
                    else if (sep == "tab")
                    {
                        line = temp.Split("\t");
                    }

                    double precursorMz = double.Parse(line[pepMzIndex].Replace(",", "."), CultureInfo.InvariantCulture);

                if (mzLastPeptide != precursorMz)
                    {
                        mzLastPeptide = precursorMz;
                        AddPeptide(library, line, sequenceIndex, pepMzIndex);
                        AddTransition(library, line, mzLastPeptide, transMzIndex, intensityIndex, precursorMz);
                    }
                    else
                    {
                        AddTransition(library, line, mzLastPeptide, transMzIndex, intensityIndex, precursorMz);
                    }
            }
            
            return library;
        }

        private void AddPeptide(Library library, string[] line, int sequenceIndex, int pepMzIndex)
        {
            var peptide = new Library.Peptide();
            peptide.Id = line[pepMzIndex].Replace(",", ".");
            peptide.Sequence = line[sequenceIndex];
            peptide.AssociatedTransitions = new List<Library.Transition>();
            library.PeptideList.Add(peptide.Id, peptide);
        }

        private void AddTransition(Library library, string[] line, double mzLastPeptide, int transMzIndex, int intensityIndex, double precursorMz)
        {
            var transition = new Library.Transition();
            transition.Id = line[transMzIndex].Replace(",", ".");
            transition.PrecursorMz = precursorMz;
            if (line[2].Length > 1)
            {
                transition.ProductMz = double.Parse(line[transMzIndex].Replace(",", "."), CultureInfo.InvariantCulture);
            }
            else transition.ProductMz = mzLastPeptide;
            transition.ProductIonIntensity = double.Parse(line[intensityIndex].Replace(",", "."), CultureInfo.InvariantCulture);
            string keystring = transition.Id +"-"+ transition.PrecursorMz;
            if (library.TransitionList.Contains(keystring))
            {
                logger.Info("Two of the same peptide - transition combinations were detected. The second entry was not added as a valid transition.Please check your file for duplication.");
            }
            else
            {
                library.TransitionList.Add(keystring, transition);
                var correspondingPeptide = (Library.Peptide)(library.PeptideList[key: Convert.ToString(precursorMz).Replace(",", ".")]);
                correspondingPeptide.AssociatedTransitions.Add(transition);
            }
        }
    }
}
