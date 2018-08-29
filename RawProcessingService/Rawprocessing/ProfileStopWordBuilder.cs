using Common.DataModels;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;

namespace RawProcessingService.Rawprocessing
{
    public class ProfileStopWordBuilder
    {
        //Step-1
        //1.Get the normalized(all lowercase, no punctuation) text presentation
        //2.Create the stopNword presentation
        //3.Return the stopNword presentation
        public static List<StopWord> GetStopWordPresentation(List<string> docWords, StatelessServiceContext serviceContext)
        {
            List<StopWord> StopWordPresentation = new List<StopWord>();

            //iterate through all document's words in text presentation
            for (int i = 0; i < docWords.Count; i++)
            {
                foreach (string commonWord in Globals.top50strings)
                {
                    //if match is found add this word in the stopNword presentation
                    if (docWords[i].Equals(commonWord))
                    {
                        StopWordPresentation.Add(new StopWord() { index = i, word = docWords[i] });
                    }
                }
            }
            return StopWordPresentation;
        }

        //Step-2
        //1.Get the stopNword presentation
        //2.Calculate the size of nGram presentation
        //3.Create the document's profile in n-gram stopNword
        public static Profile GetProfileStopWord(List<StopWord> swPresentation, int nGramSize, canditateOrboundary type)
        {
            //calculate the size of nGram presentation
            int targetIndex = swPresentation.Count + 1 - nGramSize;
            List<StopNGram> docProfile = new List<StopNGram>();
            //iterate through each n-gram
            for (int i = 0; i < targetIndex; i++)
            {
                StopNGram ngram = new StopNGram() { stopWordsInString = "" };

                //add words to each n-gram
                for (int j = 0; j < nGramSize; j++)
                {
                    //calculate the first and last index (in document's words) of the ngram
                    if (j == 0)
                    {
                        ngram.lower = swPresentation[i + j].index;
                    }
                    else if (j == nGramSize - 1)
                    {
                        ngram.upper = swPresentation[i + j].index;
                    }

                    ngram.stopWordsInString += swPresentation[i + j].word;
                    if (j < nGramSize - 1) { ngram.stopWordsInString += ","; }
                }

                //add current n-gram to the profile
                docProfile.Add(ngram);
            }
            Profile profile = new Profile() { profileType = type, ngrams = docProfile.ToList() };
            return profile;
        }
    }
}