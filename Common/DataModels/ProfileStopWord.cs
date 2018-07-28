using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Common.DataModels
{
    public class ProfileStopWord
    {
        public int ProfileStopWordID { get; set; }
        public canditateOrboundary profileType { get; set; }

        [Required]
        public List<StopNGram> ngrams { get; set; }

        public int documentID { get; set; }

        [IgnoreDataMember]
        public Document document { get; set; }
    }
}