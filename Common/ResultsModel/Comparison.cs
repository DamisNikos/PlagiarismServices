using System.Collections.Generic;

namespace Common.ResultsModel
{
    public class Comparison
    {
        public int comparisonID { get; set; }

        public string ComparisonUser { get; set; }

        public string OriginalDocumentName { get; set; }

        public string SuspiciousDocumentName { get; set; }

        public List<CommonPassage> CommonPassages { get; set; }
    }
}