using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Common.DataModels
{
    public class OldProfileStopWord
    {
        public int ProfileStopWordID { get; set; }
        public canditateOrboundary profileType { get; set; }

        [Required]
        public List<OldStopNGram> ngrams { get; set; }

        public int documentID { get; set; }

        [IgnoreDataMember]
        public OldDocument document { get; set; }
    }
}