using Common.DataModels;
using Common.Interfaces;
using Common.ResultsModel;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Newtonsoft.Json;
using PlagiarismAlgorithmService.Algorithm;
using PlagiarismAlgorithmService.Interfaces;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;

namespace PlagiarismAlgorithmService
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class PlagiarismAlgorithmService : Actor, IPlagiarismAlgorithmService
    {
        public PlagiarismAlgorithmService(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        public async Task<bool> CompareDocumentsByHash(string inputDocumentHash, List<string> suspiciousDocumentHashes, int targetCounter)
        {
            ActorEventSource.Current.ActorMessage(this, "PLS: Targets received");
            const int thetaG = 100;
            Document inputDocument;
            List<Profile> inputDocumentProfiles = new List<Profile>();
            List<string> inputDocumentWords = new List<string>();
            using (var originalcontext = new DocumentContext())
            {
                inputDocument = originalcontext.Documents
                    .AsNoTracking()
                    .Where(n => n.DocHash.Equals(inputDocumentHash))
                    .OrderByDescending(n => n.DocumentID).FirstOrDefault();

                inputDocumentProfiles = JsonConvert.DeserializeObject<List<Profile>>(inputDocument.profiles);
                inputDocumentWords = JsonConvert.DeserializeObject<List<string>>(inputDocument.words);
                foreach (Profile profile in inputDocumentProfiles)
                {
                    StopWordListGenerator.GenerateStopWordList(profile);
                }
            }

            ActorEventSource.Current.ActorMessage(this, $"PLS: Input document: {inputDocument.DocName} retrieved");

            List<Document> suspiciousDocuments = new List<Document>();
            foreach (string suspiciousHash in suspiciousDocumentHashes)
            {
                using (var suspiciouscontext = new DocumentContext())
                {
                    suspiciousDocuments.Add(suspiciouscontext.Documents
                                .AsNoTracking()
                                .Where(n => n.DocHash.Equals(suspiciousHash))
                                .OrderByDescending(n => n.DocumentID).FirstOrDefault());
                }
            }

            ActorEventSource.Current.ActorMessage(this, $"PLS: {suspiciousDocuments.Count} documents retrieved from database");

            Parallel.ForEach(suspiciousDocuments, databaseDocument =>
            {
                Comparison comparison = new Comparison
                {
                    ComparisonUser = inputDocument.DocUser,
                    OriginalDocumentName = inputDocument.DocName,
                    SuspiciousDocumentName = databaseDocument.DocName,
                    CommonPassages = new List<CommonPassage>()
                };

                List<Profile> databaseDocumentProfiles = new List<Profile>();
                List<string> databaseDocumentWords = new List<string>();
                databaseDocumentProfiles = JsonConvert.DeserializeObject<List<Profile>>(databaseDocument.profiles);
                databaseDocumentWords = JsonConvert.DeserializeObject<List<string>>(databaseDocument.words);
                foreach (Profile profile in databaseDocumentProfiles)
                {
                    StopWordListGenerator.GenerateStopWordList(profile);
                }

                //=======================++==========Step-1 Canditate Retrieval=========================================
                //Step-1.1
                //Get the intersected(common ngrams) profile (n = 11) of the 2 documents
                Profile canditateIntersection = ProfileIntersection.IntersectProfiles(
                     inputDocumentProfiles.Where(n => n.profileType == canditateOrboundary.Canditate).Single(),
                     databaseDocumentProfiles.Where(n => n.profileType == canditateOrboundary.Canditate).Single());

                //Step - 1.1.1
                //Check to see if any common ngrams found
                if (canditateIntersection.ngrams.Count != 0)
                {
                    //Step-1.2
                    //Apply criterion (1) to the canditate intersection to filter out false positives
                    Profile canditateIntersectionResults = Criteria.ApplyCanditateRetrievalCriterion(canditateIntersection);

                    //Step-1.2.1
                    //Check to see if any common ngrams found after the applying criterion(1)
                    if (canditateIntersectionResults.ngrams.Count != 0)
                    {
                        //===============++++++=============Step-2 Passage Boundary Detection========++==========================

                        //Step-2.2 REDO Step-1.1 to intersect the profiles (n = 8 )
                        Profile boundaryIntersection = ProfileIntersection.IntersectProfiles(
                             inputDocumentProfiles.Where(n => n.profileType == canditateOrboundary.Boundary).Single(),
                             databaseDocumentProfiles.Where(n => n.profileType == canditateOrboundary.Boundary).Single());
                        //Step-2.4
                        //Apply criterion (2) to avoid noise of coincidental matches
                        Profile boundaryIntersectionResults = Criteria.ApplyMatchCriterion(boundaryIntersection);
                        //Step-2.5
                        //Get a list M of matched Ngrams
                        //where members of M are ordered according to the first appearance of a match in the suspicious document
                        List<int[]> M = Criteria.MatchedNgramSet(
                             databaseDocumentProfiles.Where(n => n.profileType == canditateOrboundary.Boundary).Single(),
                             inputDocumentProfiles.Where(n => n.profileType == canditateOrboundary.Boundary).Single(),
                             boundaryIntersectionResults);
                        //Step-2.6 Apply criterion (3)
                        List<List<IndexedBoundary>> boundaries = BoundaryDetection.DetectInitialSet(M, thetaG);
                        //Step-2.7 Apply criterion (4)
                        List<IndexedBoundary> boundariesSuspicious = new List<IndexedBoundary>();
                        List<IndexedBoundary> boundariesOriginal = new List<IndexedBoundary>();
                        foreach (List<IndexedBoundary> mBoundary in boundaries)
                        {
                            boundariesSuspicious.Add(mBoundary[0]);
                            boundariesOriginal.Add(mBoundary[1]);
                        }
                        //Step-2.8
                        //Convert the indexes of the stopWords to their corresponding words
                        //in order to retrieve the passages
                        List<IndexedBoundary> passageBoundariesSuspicious = BoundaryConverter.StopWordToWord(boundariesSuspicious,
                            databaseDocumentProfiles.Where(n => n.profileType == canditateOrboundary.Boundary).Single());

                        List<IndexedBoundary> passageBoundariesOriginal = BoundaryConverter.StopWordToWord(boundariesOriginal,
                            inputDocumentProfiles.Where(n => n.profileType == canditateOrboundary.Boundary).Single());
                        //Examine each set of passages
                        for (int i = 0; i < passageBoundariesOriginal.Count; i++)
                        {
                            //string resultMessage = "";
                            string originalPassage = "";
                            string suspiciousPassage = "";
                            //=================Step-3 Creating the profiles of letter ngrams=================================
                            //Step-3.1
                            //Get the document's profile in letters ngrams
                            ProfileCharacter passageSuspicious = ProfileCharacterBuilder.GetProfileCharacter(
                                                    databaseDocumentWords, passageBoundariesSuspicious[i], 3);
                            ProfileCharacter passageOriginal = ProfileCharacterBuilder.GetProfileCharacter(
                                                   inputDocumentWords, passageBoundariesOriginal[i], 3);
                            //Step-3.1.1
                            //Remove duplicate entries
                            passageSuspicious = ProfileCharacterBuilder.RemoveDuplicates(passageSuspicious);
                            passageOriginal = ProfileCharacterBuilder.RemoveDuplicates(passageOriginal);
                            //============================Step-4 Post-processing=============================================
                            //Step-4.1  (overloading method used on step-1.1 and step-2.2)
                            //Get the intersected(common ngrams) profile of the 2 passages
                            ProfileCharacter passageIntersection = ProfileIntersection.IntersectProfiles(passageSuspicious, passageOriginal);
                            //Step-4.2
                            //Apply criterion (5) to find the similarity score between the 2 profiles
                            float similarityScore = Criteria.SimilarityScore(passageSuspicious, passageOriginal, passageIntersection);
                            //Retrieve the exact passages
                            for (int j = passageBoundariesSuspicious[i].lower;
                                j <= passageBoundariesSuspicious[i].upper; j++)
                            {
                                suspiciousPassage += $"{databaseDocumentWords[j]} ";
                            }
                            for (int j = passageBoundariesOriginal[i].lower; j <= passageBoundariesOriginal[i].upper; j++)
                            {
                                originalPassage += $"{inputDocumentWords[j]} ";
                            }

                            CommonPassage commonPassage = new CommonPassage
                            {
                                OriginalDocumentPassage = originalPassage,
                                SuspiciousDocumentPassage = suspiciousPassage,
                                SimilarityScore = similarityScore
                            };
                            comparison.CommonPassages.Add(commonPassage);
                        }
                    }
                }
                if (comparison.CommonPassages.Count != 0)
                {
                    using (var context = new DocumentContext())
                    {
                        foreach (CommonPassage passage in comparison.CommonPassages)
                        {
                            context.CommonPassages.Add(passage);
                        }
                        context.Comparisons.Add(comparison);

                        context.SaveChanges();
                    }
                }
            });

            ActorEventSource.Current.ActorMessage(this, "PLS: Updating counter in management service");

            var serviceName = new Uri("fabric:/PlagiarismServices/ManagerService");
            using (var client = new FabricClient())
            {
                var partitions = await client.QueryManager.GetPartitionListAsync(serviceName);

                var partitionInformation = (Int64RangePartitionInformation)partitions.FirstOrDefault().PartitionInformation;
                IManagement managementClient = ServiceProxy.Create<IManagement>(serviceName, new ServicePartitionKey(partitionInformation.LowKey));

                await managementClient.ExaminedDocumentsAsync(suspiciousDocumentHashes.Count, targetCounter, inputDocument.DocUser, inputDocument.DocName);
            }

            return true;
        }
    }
}