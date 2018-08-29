using System.Collections.Generic;

namespace Common.DataModels
{
    public class Profile
    {
        public canditateOrboundary profileType { get; set; }

        public List<StopNGram> ngrams { get; set; }
    }
}