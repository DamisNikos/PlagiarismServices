using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
