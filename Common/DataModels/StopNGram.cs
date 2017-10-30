using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DataModels
{
    public class StopNGram
    {
        public int StopNgramID { get; set; }

        [Required]
        public string stopWordsInString { get; set; }

        [NotMapped]
        public List<StopWord> stopWords { get; set; }

        [Required]
        public int lower { get; set; }

        [Required]
        public int upper { get; set; }

        [ForeignKey("profileStopWord")]
        public int profileStopWordID { get; set; }
        [IgnoreDataMember]
        public ProfileStopWord profileStopWord { get; set; }
    }
}
