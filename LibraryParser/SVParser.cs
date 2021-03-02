#nullable enable

using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;

/* This class is meant to read separated value files such as tsv/csv that contain iRT peptide information. A template will be provided with the correct 
 headings (same naming convention as OpenMS, tutorial handout 2016 - section 6.4, page 51 {https://www.openms.de/wp-content/uploads/2016/02/handout1.pdf}). 
 It is populated with Biognosys 11 irt peptides to illustrate the concept.
 
 The purpose is to accomodate users that may have a different file format than TraML, for example the Biognosys iRT Kit reference sheet or another format that they
 would prefer to copy to csv rather than TraML. Support is also built in for the Microsoft Excel South African language issue, where Excel uses commas
 as decimal points if your country is set to South African. It then separates the columns using a semicolon rather than a comma.

 Please note that a TraML file with a more precise indication of the expected retention time will give closer approximation to the retention times picked up for each peptide by Skyline.
 
 Marina - 13092019*/

namespace LibraryParser
{
    public class SVReader : LibraryReader
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public Library LoadLibrary(string path)
        {
            logger.Info("Loading file: {0}", path);
            Library library = new Library();
            List<string> allLines = File.ReadLines(path).ToList();
            string heading = allLines[0];
            int sequenceIndex = 100;
            int pepMzIndex = 100;
            int transMzIndex = 100;
            int intensityIndex = 100;

            char? columnSeparator = DetectColumnSeparator(heading);
            if (!columnSeparator.HasValue)
                throw new SVFileFormatException("Separated value file provided for iRT peptides, however the separator could not be established. Please rerun and ensure columns are separated with commas, semicolons or tabs", path);

            string[] line = heading.Split(columnSeparator.Value);

            for (int position = 0; position < line.Count(); position++)
            {
                string lowerValue = line[position].ToLower();
                switch (lowerValue)
                {
                    case "peptidesequence":
                    case "nominal sequence":
                        sequenceIndex = position;
                        break;
                    case "precursormz":
                    case "q1 monoisotopic":
                        pepMzIndex = position;
                        break;
                    case "productmz":
                    case "q3":
                        transMzIndex = position;
                        break;
                    case "relative intensity":
                    case "libraryintensity":
                        intensityIndex = position;
                        break;
                    default:
                        // Do nothing
                        break;
                }
            }
            if (sequenceIndex == 100 || pepMzIndex == 100 || transMzIndex == 100 || intensityIndex == 100)
            {
                throw new SVFileFormatException("iRT peptides file was provided, but the correct headings could not be found and column could not be distinguished. Please rename your headings as illustrated in the template file", path);
            }

            double mzLastPeptide = 0;
            for (int lineNumber = 1; lineNumber < allLines.Count; lineNumber++)
            {
                string temp = allLines.ElementAt(lineNumber);
                line = temp.Split(columnSeparator.Value);

                double precursorMz = ParseLocalisedDouble(line[pepMzIndex]);

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

        private static char? DetectColumnSeparator(string heading)
        {
            // Figure out which separator is used.  TODO: This is not bombproof if values in the header row include characters confusable with separators.
            if (heading.Contains(';'))
                return ';';
            if (heading.Contains(","))
                return ',';
            if (heading.Contains("\t") || heading.Contains(" "))
                return '\t';
            return default;
        }

        private void AddPeptide(Library library, string[] line, int sequenceIndex, int pepMzIndex)
        {
            string id = line[pepMzIndex].Replace(",", ".");
            var peptide = new Library.Peptide(id, line[sequenceIndex]);
            library.Peptides.Add(peptide.Id, peptide);
        }

        private void AddTransition(Library library, string[] line, double mzLastPeptide, int transMzIndex, int intensityIndex, double precursorMz)
        {
            Library.Transition transition = new Library.Transition(line[transMzIndex], precursorMz.ToString(CultureInfo.InvariantCulture))
            {
                PrecursorMz = precursorMz,
                ProductMz = line[2].Length > 1
                    ? ParseLocalisedDouble(line[transMzIndex])
                    : mzLastPeptide,
                ProductIonIntensity = ParseLocalisedDouble(line[intensityIndex])
            };
            string keystring = $"{transition.Id}-{transition.PrecursorMz}";
            if (library.TransitionList.Contains(keystring))
            {
                logger.Warn("Two of the same peptide - transition combinations were detected. The second entry was not added as a valid transition. Please check your file for duplication.");
            }
            else
            {
                library.TransitionList.Add(keystring, transition);
                var correspondingPeptide = library.Peptides[precursorMz.ToString(CultureInfo.InvariantCulture)];
                correspondingPeptide.AssociatedTransitions.Add(transition);
            }
        }

        public IList<double> CollectTransitions(string path)
        {
            List<double> allTransitions = new List<double>();
            List<string> allLines = File.ReadLines(path).ToList();
            string heading = allLines.ElementAt(0);
            int transMzIndex = 100;
            char? columnSeparator = DetectColumnSeparator(heading);
            if (!columnSeparator.HasValue)
                throw new SVFileFormatException("Separated value file provided for iRT peptides, however the separator could not be established. Please rerun and ensure columns are separated with commas, semicolons or tabs", path);

            string[] line = heading.Split(columnSeparator.Value);

            for (int position = 0; position < line.Count(); position++)
            {
                if (line[position].ToLower().Contains("productmz") || line[position].ToLower().Contains("q3"))
                {
                    transMzIndex = position;
                }
            }
            if (transMzIndex == 100)
            {
                throw new SVFileFormatException("iRT peptides file was provided, but the correct headings could not be found and column could not be distinguished. Please rename your headings as illustrated in the template file", path);
            }

            for (int lineNumber = 1; lineNumber < allLines.Count(); lineNumber++)
            {
                string temp = allLines.ElementAt(lineNumber);
                line = temp.Split(columnSeparator.Value);
                if (!double.TryParse(line[transMzIndex].Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double transitionMz))
                    throw new SVFileFormatException($"Couldn't parse {line[transMzIndex]} (column {transMzIndex}) as a double", path);

                allTransitions.Add(transitionMz);
            }
            return allTransitions;
        }

        private static double ParseLocalisedDouble(string candidate) => double.Parse(candidate.Replace(",", "."), CultureInfo.InvariantCulture);
    }
}
