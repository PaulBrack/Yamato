using NLog;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using System.Linq;

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
                for (int iii = 1; iii < Lines.Count(); iii++)
                {
                    string temp = Lines.ElementAt(iii)[0];
                    string[] line;
                    if (temp.Contains(";"))
                    {
                        line = temp.Split(";");
                        if (lastPeptideRead != Convert.ToDouble(line[1]))
                        {
                            lastPeptideRead = Convert.ToDouble(line[1]);
                            AddPeptide(library, line);
                            AddTransition(library, line, lastPeptideRead);
                        }
                        else
                        {
                            AddTransition(library, line, lastPeptideRead);
                        }
                    }
                    else if (temp.Contains(","))
                    { line = temp.Split(",");
                        if (lastPeptideRead != Convert.ToDouble(line[1]))
                        {
                            lastPeptideRead = Convert.ToDouble(line[1]);
                            AddPeptide(library, line);
                            AddTransition(library, line, lastPeptideRead);
                        }
                        else
                        {
                            AddTransition(library, line, lastPeptideRead);
                        }
                    }
                   
                        
                }
            }
            else
            {
                var Lines = File.ReadLines(path).Select(a => a.Split("//n"));

                for (int iii = 1; iii < Lines.Count(); iii++)
                {
                    string temp = Lines.ElementAt(iii)[0];
                    string[] line = temp.Split("\t");
                    if (lastPeptideRead != Convert.ToDouble(line[1]))
                    {
                        lastPeptideRead = Convert.ToDouble(line[1]);
                        AddPeptide(library, line);
                        AddTransition(library, line, lastPeptideRead);
                    }
                    else
                    {
                        AddTransition(library, line, lastPeptideRead);
                    }
                }
            }


            return library;
        }

     

        private void AddPeptide(Library library, string[] line)
        {
            var peptide = new Library.Peptide();
            peptide.Id = line[1];
            peptide.Sequence = line[0];
            peptide.AssociatedTransitions = new List<Library.Transition>();
            library.PeptideList.Add(peptide.Id, peptide);
        }

        private void AddTransition(Library library, string[] line, double lastPeptideRead)
        {
            var transition = new Library.Transition();
            transition.Id = line[2];
            transition.PrecursorMz = Convert.ToDouble(line[1]);
            if (line[2].Length > 1)
            {
                transition.ProductMz = Convert.ToDouble(line[2]);
            } else transition.ProductMz = lastPeptideRead;
            transition.ProductIonIntensity = Convert.ToDouble(line[3]);
            if (library.TransitionList.Contains(transition.Id))
            {
                Convert.ToDouble(transition.Id);
                transition.Id += 0.000001;
                Convert.ToString(transition.Id);
            }
            library.TransitionList.Add(transition.Id, transition);
            var correspondingPeptide = (Library.Peptide)(library.PeptideList[key: line[1]]);
            correspondingPeptide.AssociatedTransitions.Add(transition);
        }
    }
}
