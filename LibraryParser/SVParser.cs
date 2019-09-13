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
        private double lastPeptideRead = 0;

        public Library LoadLibrary(string path)
        {
            logger.Info("Loading file: {0}", path);

            Library library = new Library();
            if (path.Contains("csv"))
            {
                var Lines = File.ReadLines(path).Select(a => a.Split("//n"));
                string sep = "";
                //figure out which column is which and also which separater is used:
                string heading = Lines.ElementAt(0)[0];
                string[] line = { };
                int sequenceIndex = 0;
                int pepMzIndex = 0;
                int transMzIndex = 0;
                int intensityIndex = 0;

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
                for (int iii = 1; iii < Lines.Count(); iii++)
                {
                    string temp = Lines.ElementAt(iii)[0];
                    
                    if (sep== "semi-colon")
                    {
                        line = temp.Split(";");
                    }
                    else if (sep == "comma")
                    {
                        line = temp.Split(",");
                    }
                    double precursorMz = Convert.ToDouble(line[pepMzIndex]);
                    if (lastPeptideRead != precursorMz)
                    {
                        lastPeptideRead = precursorMz;
                        AddPeptide(library, line, sequenceIndex, pepMzIndex);
                        AddTransition(library, line, lastPeptideRead, transMzIndex, intensityIndex, pepMzIndex);
                    }
                    else
                    {
                        AddTransition(library, line, lastPeptideRead, transMzIndex, intensityIndex, pepMzIndex);
                    }
                }
            }
            else 
            {
                var Lines = File.ReadLines(path).Select(a => a.Split("//n"));
                //figure out which column is which:
                string heading = Lines.ElementAt(0)[0];
                string[] line = { };
                int sequenceIndex = 0;
                int pepMzIndex = 0;
                int transMzIndex = 0;
                int intensityIndex = 0;
                line = heading.Split("\t");

                for (int position = 0; position < line.Count(); position++)
                {
                    if (line[position].ToLower().Contains("peptidesequence") && !line[position].ToLower().Contains("sequence id"))
                    {
                        sequenceIndex = position;
                    }
                    else if ( line[position].ToLower().Contains("q1 monoisotopic") || line[position].ToLower().Contains("precursormz"))
                    {
                        pepMzIndex = position;
                    }
                    else if ( line[position].ToLower().Contains("q3") || line[position].ToLower().Contains("productmz"))
                    {
                        transMzIndex = position;
                    }
                    else if (line[position].ToLower().Contains("intensity") || line[position].ToLower().Contains("libraryintensity"))
                    {
                        intensityIndex = position;
                    }
                }
                //Now we use the indexes
                for (int iii = 1; iii < Lines.Count(); iii++)
                {
                    line = Lines.ElementAt(iii)[0].Split("\t");
                    
                    double precursorMz = double.Parse(line[pepMzIndex].Replace(",","."), CultureInfo.InvariantCulture);
                    if (lastPeptideRead != precursorMz)
                    {
                        lastPeptideRead = precursorMz;
                        AddPeptide(library, line, sequenceIndex, pepMzIndex);
                        AddTransition(library, line, lastPeptideRead, transMzIndex, intensityIndex, precursorMz);
                    }
                    else
                    {
                        AddTransition(library, line, lastPeptideRead, transMzIndex, intensityIndex, precursorMz);
                    }
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

        private void AddTransition(Library library, string[] line, double lastPeptideRead, int transMzIndex, int intensityIndex, double precursorMz)
        {
            var transition = new Library.Transition();
            transition.Id = line[transMzIndex].Replace(",", ".");
            transition.PrecursorMz = precursorMz;
            if (line[2].Length > 1)
            {
                transition.ProductMz = double.Parse(line[transMzIndex].Replace(",", "."), CultureInfo.InvariantCulture);
            }
            else transition.ProductMz = lastPeptideRead;
            transition.ProductIonIntensity = double.Parse(line[intensityIndex].Replace(",", "."), CultureInfo.InvariantCulture);
            if (library.TransitionList.Contains(transition.Id))
            {
                double.Parse(transition.Id.Replace(",", "."), CultureInfo.InvariantCulture);
                transition.Id += 0.000001;
                Convert.ToString(transition.Id);
            }
            library.TransitionList.Add(transition.Id, transition);
            var correspondingPeptide = (Library.Peptide)(library.PeptideList[key: Convert.ToString(precursorMz)]);
            correspondingPeptide.AssociatedTransitions.Add(transition);
        }
    }
}
