using NLog;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LibraryParser
{
    public class LibraryReader
    {
        public void StoreUniprotIds(Library library, string proteinId)
        {
            //Regular expression from https://www.uniprot.org/help/accession_numbers
            MatchCollection matches = Regex.Matches(proteinId, "[OPQ][0-9][A-Z0-9]{3}[0-9]|[A-NR-Z][0-9]([A-Z][A-Z0-9]{2}[0-9]){1,2}");
            foreach (Match match in matches)
            {
                library.UniprotIdList.Add(new KeyValuePair<string, string>(proteinId, match.Value));
            }
        }

        public void LogResults(Library library, Logger logger, string path)
        {
            logger.Debug("Loading iRT library {0}", path);
            logger.Debug("{0} peptides loaded", library.PeptideList.Count);
            logger.Debug("{0} transitions loaded", library.TransitionList.Count);
            //System.IO.File.WriteAllLines("proteins.txt", library.UniprotIdList.Select(x => x.Value).Distinct());
        }
    }
}
