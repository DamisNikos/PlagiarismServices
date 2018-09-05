using Common.DataModels;
using System.Linq;

namespace PlagiarismAlgorithmService.Algorithm
{
    internal class StopWordListGenerator
    {
        public static Profile GenerateStopWordList(Profile profile)
        {
            foreach (StopNGram ngram in profile.ngrams)
            {
                ngram.stopWordsList = ngram.stopWordsInString.Split(',').ToList();
            }

            return profile;
        }
    }
}