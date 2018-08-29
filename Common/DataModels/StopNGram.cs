using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Common.DataModels
{
    public class OldStopNGram
    {
        public int StopNgramID { get; set; }

        [Required]
        public string stopWordsInString { get; set; }

        [NotMapped]
        public List<OldStopWord> stopWords { get; set; }

        [Required]
        public int lower { get; set; }

        [Required]
        public int upper { get; set; }

        [ForeignKey("profileStopWord")]
        public int profileStopWordID { get; set; }

        [IgnoreDataMember]
        public OldProfileStopWord profileStopWord { get; set; }
    }
}