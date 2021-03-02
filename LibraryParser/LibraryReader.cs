using NLog;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LibraryParser
{
    public class LibraryReader
    {
        public IList<string> ParseUniprotIds(string proteinId)
        {
            // Regular expression from https://www.uniprot.org/help/accession_numbers
            MatchCollection matches = Regex.Matches(proteinId, "[OPQ][0-9][A-Z0-9]{3}[0-9]|[A-NR-Z][0-9]([A-Z][A-Z0-9]{2}[0-9]){1,2}");
            return matches.Select(match => match.Value).ToArray();
        }

        public void LogResults(Library library, Logger logger, string path)
        {
            logger.Debug("Loading iRT library {0}", path);
            logger.Debug("{0} peptides loaded", library.Peptides.Count);
            logger.Debug("{0} transitions loaded", library.TransitionList.Count);
        }
    }
}
