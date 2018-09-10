using Common.DataModels;
using Common.ResultsModel;
using System.Collections.Generic;

namespace PlagiarismAlgorithmService.Algorithm
{
    internal class PassageDetectionStep
    {
        public static List<List<IndexedBoundary>> GetInitialSetBoundaries(Profile inputProfile, Profile databaseProfile, int thetaG)
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
            //Step-2.6 Apply criterion (3)
            List<List<IndexedBoundary>> boundaries = BoundaryDetection.DetectInitialSet(M, thetaG);

            return boundaries;
        }

        public static List<CommonPassage> RetrieveExaminedPassages(
            List<IndexedBoundary> boundariesInput,
            List<IndexedBoundary> boundariesDatabase,
            Profile inputProfile,
            Profile databaseProfile,
            List<string> inputDocumentWords,
            List<string> databaseDocumentWords)
        {
            //Step-2.8
            //Convert the indexes of the stopWords to their corresponding words
            //in order to retrieve the passages
            List<IndexedBoundary> passageBoundariesInput = BoundaryConverter.StopWordToWord(boundariesInput, inputProfile);
            List<IndexedBoundary> passageBoundariesDatabase = BoundaryConverter.StopWordToWord(boundariesDatabase, databaseProfile);

            List<CommonPassage> examinedPassages = new List<CommonPassage>();
            //Examine each set of passages
            for (int i = 0; i < passageBoundariesInput.Count; i++)
            {
                examinedPassages.Add(examinedPassage(inputDocumentWords, databaseDocumentWords, passageBoundariesInput[i], passageBoundariesDatabase[i]));
            }

            return examinedPassages;
        }

        private static CommonPassage examinedPassage(List<string> inputDocumentWords,
            List<string> databaseDocumentWords, IndexedBoundary boundaryInput,
            IndexedBoundary boundaryDatabase)
        {
            string inputPassage = "";
            string databasePassage = "";

            //=================Step-3 Creating the profiles of letter ngrams=================================
            //Step-3.1
            //Get the document's profile in letters ngrams
            ProfileCharacter passageInput = ProfileCharacterBuilder.GetProfileCharacter(
                                    inputDocumentWords, boundaryInput, 3);
            ProfileCharacter passageDatabase = ProfileCharacterBuilder.GetProfileCharacter(
                                   databaseDocumentWords, boundaryDatabase, 3);
            //Step-3.1.1
            //Remove duplicate entries
            passageInput = ProfileCharacterBuilder.RemoveDuplicates(passageInput);
            passageDatabase = ProfileCharacterBuilder.RemoveDuplicates(passageDatabase);
            //============================Step-4 Post-processing=============================================
            //Step-4.1  (overloading method used on step-1.1 and step-2.2)
            //Get the intersected(common ngrams) profile of the 2 passages
            ProfileCharacter passageIntersection = ProfileIntersection.IntersectProfiles(passageInput, passageDatabase);
            //Step-4.2
            //Apply criterion (5) to find the similarity score between the 2 profiles
            float similarityScore = Criteria.SimilarityScore(passageDatabase, passageDatabase, passageIntersection);
            //Retrieve the exact passages
            for (int j = boundaryInput.lower;
                          j <= boundaryInput.upper; j++)
            {
                inputPassage += $"{inputDocumentWords[j]} ";
            }

            for (int j = boundaryDatabase.lower; j <= boundaryDatabase.upper; j++)
            {
                databasePassage += $"{databaseDocumentWords[j]} ";
            }

            CommonPassage commonPassage = new CommonPassage
            {
                OriginalDocumentPassage = databasePassage,
                SuspiciousDocumentPassage = inputPassage,
                SimilarityScore = similarityScore
            };

            return commonPassage;
        }
    }
}