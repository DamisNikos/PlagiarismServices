using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DataModels;
namespace PlagiarismAlgorithmService
{
    class BoundaryConverter
    {
        public static List<IndexedBoundary> StopWordToWord(List<IndexedBoundary> boundaries, ProfileStopWord wordsOfDocument)
        {
            List<IndexedBoundary> newBoundaries = new List<IndexedBoundary>();
            foreach (IndexedBoundary boundary in boundaries)
            {
                IndexedBoundary newBoundary = new IndexedBoundary()
                {
                    lower = wordsOfDocument.ngrams[boundary.lower].lower,
                    upper = wordsOfDocument.ngrams[boundary.upper].upper
                };
                newBoundaries.Add(newBoundary);
            }
            return newBoundaries;
        }

    }
}
