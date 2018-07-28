using Common.DataModels;
using System.Collections.Generic;

namespace PlagiarismAlgorithmService
{
    internal class ProfileIntersection
    {
        public static ProfileStopWord IntersectProfiles(ProfileStopWord profile1, ProfileStopWord profile2)
        {
            List<StopNGram> intersection = new List<StopNGram>();

            //foreach (StopNGram ngram in profile2.ngrams)
            //{
            //    string[] stopWords = ngram.stopWordsInString.Split(',');
            //    List<StopWord> stopwordsList = new List<StopWord>();
            //    for (int i = 0; i < stopWords.Length; i++)
            //    {
            //        stopwordsList.Add(new StopWord { word = stopWords[i] });
            //    }
            //    ngram.stopWords = stopwordsList;
            //}

            int ngramLength = profile1.ngrams[0].stopWords.Count;
            for (int i = 0; i < profile1.ngrams.Count; i++)
            {
                int countEquals = 0;
                for (int j = 0; j < profile2.ngrams.Count; j++)
                {
                    int countEqualWords = 0;
                    for (int k = 0; k < ngramLength; k++)
                    {
                        if (profile1.ngrams[i].stopWords[k].word.Equals(profile2.ngrams[j].stopWords[k].word))
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
                        stopWords = profile1.ngrams[i].stopWords,
                        lower = -1,
                        upper = -1
                    });
                }
            }
            ProfileStopWord profile = new ProfileStopWord() { ngrams = new List<StopNGram>() };
            profile.ngrams = intersection;
            return profile;
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