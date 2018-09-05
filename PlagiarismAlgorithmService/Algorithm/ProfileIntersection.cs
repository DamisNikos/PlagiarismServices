﻿using Common.DataModels;
using System.Collections.Generic;
using System.Linq;

namespace PlagiarismAlgorithmService
{
    internal class ProfileIntersection
    {
        public static Profile IntersectProfiles(Profile profile1, Profile profile2)
        {
            List<StopNGram> intersection = new List<StopNGram>();

            int ngramLength = profile1.ngrams[0].stopWordsList.Count;

            for (int i = 0; i < profile1.ngrams.Count; i++)
            {
                int countEquals = 0;

                for (int j = 0; j < profile2.ngrams.Count; j++)
                {
                    int countEqualWords = 0;

                    for (int k = 0; k < ngramLength; k++)
                    {
                        if (profile1.ngrams[i].stopWordsList[k].Equals(profile2.ngrams[j].stopWordsList[k]))
                        {
                            countEqualWords++;
                        }
                    }

                    if (countEqualWords == ngramLength)
                    {
                        countEquals++;
                    }
                }

                if (countEquals > 0)
                {
                    intersection.Add(new StopNGram()
                    {
                        stopWordsList = profile1.ngrams[i].stopWordsInString.Split(',').ToList(),
                        stopWordsInString = profile1.ngrams[i].stopWordsInString,
                        lower = -1,
                        upper = -1
                    });
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
    }
}