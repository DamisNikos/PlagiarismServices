using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.ResultsModel
{
    public class CommonPassage
    {
        public int CommonPassageID { get; set; }

        public string OriginalDocumentPassage { get; set; }

        public string SuspiciousDocumentPassage { get; set; }

        public float SimilarityScore { get; set; }


        [ForeignKey("comparison")]
        public int comparisonID { get; set; }
        [IgnoreDataMember]
        public Comparison comparison { get; set; }
    }
}
