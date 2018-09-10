using Common.DataModels;
using System.Collections.Generic;

namespace PlagiarismAlgorithmService.Algorithm
{
    internal class PassageDetectionStep
    {
        public static List<int[]> GetMatchedNgramSet(Profile inputProfile, Profile databaseProfile)
        {
            //Step-2.2 REDO Step-1.1 to intersect the profiles (n = 8)
            Profile intersection = ProfileIntersection.IntersectProfiles(inputProfile, databaseProfile);
            //Step-2.4
            //Apply criterion (2) to avoid noise of coincidental matches
            Profile intersectionResults = Criteria.ApplyMatchCriterion(intersection);
            //Step-2.5
            //Get a list M of matched Ngrams
            //where members of M are ordered according to the first appearance of a match in the suspicious document
            List<int[]> M = Criteria.MatchedNgramSet(inputProfile, databaseProfile, intersectionResults);

            //DEBUG PURPOSES
            List<int> M1 = new List<int>();
            List<int> M2 = new List<int>();
            foreach (int[] pair in M)
            {
                M1.Add(pair[0]);
                M2.Add(pair[1]);
            }
            int max1 = 0;
            int index1 = 0;

            int max2 = 0;
            int index2 = 0;
            for (int i = 0; i < M.Count - 1; i++)
            {
                if (M1[i] > max1)
                {
                    max1 = M1[i];
                    index1 = i;
                }

                if (M2[i] > max2)
                {
                    max2 = M2[i];
                    index2 = i;
                }
            }
            //END OF DEBUG PURPOSES

            return M;
        }
    }
}