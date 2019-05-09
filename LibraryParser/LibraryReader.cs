using System.Text.RegularExpressions;

namespace LibraryParser
{
    public class LibraryReader
    {
        protected void StoreUniprotIds(Library library, string proteinId)
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
