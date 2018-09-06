using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PlagiarismAlgorithmService
{
    internal class ProfileCharacterBuilder
    {
        public static ProfileCharacter GetProfileCharacter(List<string> docWords, IndexedBoundary boundary, int nGramSize)
        {
            string allWords = "";
            for (int i = 0; i <= boundary.upper; i++)
            {
                if (i >= boundary.lower)
                {
                    try
                    {
                        allWords += docWords[i];
                    }
                    catch
                    {
                        Debugger.Break();
                    }
                }
            }
            int targetIndex = allWords.Length + 1 - nGramSize;
            List<List<char>> passageProfile = new List<List<char>>();
            for (int i = 0; i < targetIndex; i++)
            {
                List<char> ngram = new List<char>();
                for (int j = 0; j < nGramSize; j++)
                {
                    ngram.Add(allWords[i + j]);
                }
                passageProfile.Add(ngram);
            }

            ProfileCharacter profile = new ProfileCharacter() { ngrams = passageProfile };

            return profile;
        }

        public static ProfileCharacter RemoveDuplicates(ProfileCharacter profile)
        {
            List<List<char>> profileWithoutDuplicates = new List<List<char>>
            {
                profile.ngrams[0]
            };
            for (int i = 1; i < profile.ngrams.Count; i++)
            {
                bool foundInNew = false;
                foreach (List<char> ngram in profileWithoutDuplicates)
                {
                    bool isEqual = true;
                    for (int j = 0; j < ngram.Count; j++)
                    {
                        if (!profile.ngrams[i][j].Equals(ngram[j]))
                        {
                            isEqual = false;
                            break;
                        }
                    }
                    if (isEqual)
                    {
                        foundInNew = true;
                        break;
                    }
                }
                if (!foundInNew)
                {
                    profileWithoutDuplicates.Add(profile.ngrams[i]);
                }
            }
            ProfileCharacter newProfile = new ProfileCharacter() { ngrams = profileWithoutDuplicates };
            var DoubleValues = profile.ngrams.Except(newProfile.ngrams).ToList<List<char>>();

            return newProfile;
        }
    }
}