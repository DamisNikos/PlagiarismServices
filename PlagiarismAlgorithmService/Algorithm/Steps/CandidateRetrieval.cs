using Common.DataModels;

namespace PlagiarismAlgorithmService.Algorithm
{
    internal class CandidateRetrieval
    {
        public static bool Retrieve(Profile inputProfile, Profile databaseProfile)
        {
            //Step-1.1
            //Get the intersected(common ngrams) profile (n = 11) of the 2 documents
            Profile intersection = ProfileIntersection.IntersectProfiles(inputProfile, databaseProfile);
            //Step - 1.1.1
            //Check to see if any common ngrams found
            if (intersection.ngrams.Count != 0)
            {
                //Step-1.2
                //Apply criterion (1) to the canditate intersection to filter out false positives
                Profile intersectionResults = Criteria.ApplyCanditateRetrievalCriterion(intersection);
                //Step-1.2.1
                //Check to see if any common ngrams found after the applying criterion(1)
                if (intersectionResults.ngrams.Count != 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}