using Common.DataModels;
using System.Collections.Generic;

namespace PlagiarismAlgorithmService
{
    internal class Criteria
    {
        public static ProfileStopWord ApplyCanditateRetrievalCriterion(ProfileStopWord profile)
        {
            List<string> mostCommon6 = new List<string> { "the", "of", "and", "a", "in", "to" };
            List<StopNGram> nGramCollection = new List<StopNGram>();
            //iterate through all ngrams that belongs to the interesection of the two profiles
            foreach (var ngram in profile.ngrams)
            {
                int maxseq = 0;
                int membersofC = 0;
                int currentsq = 0;
                //iterate through all members of the ngram
                foreach (var word in ngram.stopWords)
                {
                    bool found = false;
                    //iterate through all words in C
                    for (int i = 0; i < mostCommon6.Count; i++)
                    {
                        //increase sequence and member if match is found and stop iterating C
                        if (word.Equals(mostCommon6[i]))
                        {
                            membersofC++;
                            currentsq++;
                            found = true;
                            break;
                        }
                    }
                    //if match is not found compare this sequence against previous maximal and update accordingly
                    if (!found)
                    {
                        if (maxseq < currentsq) maxseq = currentsq;
                        currentsq = 0;
                    }
                }
                //if criterion (1) is satisfied add this ngram to the collection
                if ((membersofC < ngram.stopWords.Count - 1) && (maxseq < ngram.stopWords.Count - 2))
                {
                    nGramCollection.Add(ngram);
                }
            }
            ProfileStopWord profileStop = new ProfileStopWord() { ngrams = new List<StopNGram>() };
            profileStop.ngrams = nGramCollection;
            return profileStop;
        }

        public static ProfileStopWord ApplyMatchCriterion(ProfileStopWord profile)
        {
            List<string> mostCommon6 = new List<string> { "the", "of", "and", "a", "in", "to" };
            List<StopNGram> nGramCollection = new List<StopNGram>();
            //iterate through each n-gram
            foreach (var ngram in profile.ngrams)
            {
                int membersofC = 0;
                //iterate through each word in the n-gram
                foreach (var word in ngram.stopWords)
                {
                    //iterate through each word in C
                    for (int i = 0; i < mostCommon6.Count; i++)
                    {
                        //if match is found increase members
                        if (word.Equals(mostCommon6[i]))
                        {
                            membersofC++;
                            break;
                        }
                    }
                }
                //if criterion (2) is satisfied add this ngram to the collection
                if ((membersofC < ngram.stopWords.Count))
                {
                    nGramCollection.Add(ngram);
                }
            }
            ProfileStopWord profileStop = new ProfileStopWord() { ngrams = new List<StopNGram>() };
            profileStop.ngrams = nGramCollection;
            return profileStop;
        }

        public static List<int[]> MatchedNgramSet(ProfileStopWord suspiciousProfile, ProfileStopWord originalProfile, ProfileStopWord commonProfile)
        {
            List<int[]> setOfMatched = new List<int[]>();
            for (int i = 0; i < commonProfile.ngrams.Count; i++)
            {
                int locationSuspicious = 0;
                for (int j = 0; j < suspiciousProfile.ngrams.Count; j++)
                {
                    int matches = 0;
                    for (int k = 0; k < suspiciousProfile.ngrams[j].stopWords.Count; k++)
                    {
                        if (suspiciousProfile.ngrams[j].stopWords[k].word.Equals(commonProfile.ngrams[i].stopWords[k].word))
                        {
                            matches++;
                        }
                    }
                    if (matches == suspiciousProfile.ngrams[j].stopWords.Count)
                    {
                        locationSuspicious = j;
                    }
                }

                int locationOriginal = 0;
                for (int j = 0; j < originalProfile.ngrams.Count; j++)
                {
                    int matches = 0;
                    for (int k = 0; k < originalProfile.ngrams[j].stopWords.Count; k++)
                    {
                        if (originalProfile.ngrams[j].stopWords[k].word.Equals(commonProfile.ngrams[i].stopWords[k].word))
                        {
                            matches++;
                        }
                    }
                    if (matches == originalProfile.ngrams[j].stopWords.Count)
                    {
                        locationOriginal = j;
                    }
                }
                setOfMatched.Add(new int[] { locationSuspicious, locationOriginal });
            }

            return setOfMatched;
        }

        public static float SimilarityScore(ProfileCharacter suspicious, ProfileCharacter original, ProfileCharacter intersection)
        {
            float similarity;
            float sizeOfProfile1 = suspicious.ngrams.Count;
            float sizeOfProfile2 = original.ngrams.Count;
            float sizeOfIntersection = intersection.ngrams.Count;
            float maxSize;
            maxSize = sizeOfProfile1 > sizeOfProfile2 ? sizeOfProfile1 : sizeOfProfile2;

            similarity = sizeOfIntersection / maxSize;
            return similarity;
        }
    }
}