using Common.DataModels;
using System.Collections.Generic;
using System.Linq;

namespace PlagiarismAlgorithmService
{
    internal class ProfileIntersection
    {
        public static Profile IntersectProfiles(Profile profile1, Profile profile2)
        {
            List<StopNGram> intersection = new List<StopNGram>();

            for (int i = 0; i < profile1.ngrams.Count; i++)
            {
                for (int j = 0; j < profile2.ngrams.Count; j++)
                {
                    if (CheckNgramEquality(profile1.ngrams[i], profile2.ngrams[j]))
                    {
                        intersection.Add(new StopNGram()
                        {
                            stopWordsList = profile1.ngrams[i].stopWordsInString.Split(',').ToList(),
                            stopWordsInString = profile1.ngrams[i].stopWordsInString,
                            lower = -1,
                            upper = -1
                        });
                        break;
                    }
                }
            }

            return new Profile() { ngrams = intersection };
        }

        public static ProfileCharacter IntersectProfiles(ProfileCharacter profile1, ProfileCharacter profile2)
        {
            List<List<char>> ngramsCollection = new List<List<char>>();
            foreach (var ngram1 in profile1.ngrams)
            {
                int countEquals = 0;
                foreach (var ngram2 in profile2.ngrams)
                {
                    int countEqualLetters = 0;
                    for (int i = 0; i < ngram1.Count; i++)
                    {
                        if (ngram1[i].Equals(ngram2[i]))
                        {
                            countEqualLetters++;
                        }
                    }
                    if (countEqualLetters == ngram1.Count)
                    {
                        countEquals++;
                    }
                }
                if (countEquals > 0)
                {
                    ngramsCollection.Add(ngram1);
                }
            }
            ProfileCharacter profile = new ProfileCharacter() { ngrams = ngramsCollection };
            return profile;
        }

        public static bool CheckNgramEquality(StopNGram ngram1, StopNGram ngram2)
        {
            for (int i = 0; i < ngram1.stopWordsList.Count; i++)
            {
                if (!(ngram1.stopWordsList[i].Equals(ngram2.stopWordsList[i]))) return false;
            }
            return true;
        }
    }
}