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
                    OriginalDocumentName = databaseDocument.DocName,
                    SuspiciousDocumentName = inputDocument.DocName,
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

                //=======================++==========Step-1 Candidate Retrieval=========================================
                if (CandidateRetrieval.Retrieve(
                    inputDocumentProfiles.Where(n => n.profileType == canditateOrboundary.Canditate).Single(),
                     databaseDocumentProfiles.Where(n => n.profileType == canditateOrboundary.Canditate).Single()))
                {
                    //===============++++++=============Step-2 Passage Boundary Detection========++==========================
                    List<int[]> M = PassageDetectionStep.GetMatchedNgramSet(
                        inputDocumentProfiles.Where(n => n.profileType == canditateOrboundary.Boundary).Single(),
                         databaseDocumentProfiles.Where(n => n.profileType == canditateOrboundary.Boundary).Single());

                    //Step-2.6 Apply criterion (3)
                    List<List<IndexedBoundary>> boundaries = BoundaryDetection.DetectInitialSet(M, thetaG);
                    //Step-2.7 Apply criterion (4)
                    List<IndexedBoundary> boundariesInput = new List<IndexedBoundary>();
                    List<IndexedBoundary> boundariesDatabase = new List<IndexedBoundary>();
                    foreach (List<IndexedBoundary> mBoundary in boundaries)
                    {
                        boundariesInput.Add(mBoundary[0]);
                        boundariesDatabase.Add(mBoundary[1]);
                    }
                    //Step-2.8
                    //Convert the indexes of the stopWords to their corresponding words
                    //in order to retrieve the passages
                    List<IndexedBoundary> passageBoundariesInput = BoundaryConverter.StopWordToWord(boundariesInput,
                        inputDocumentProfiles.Where(n => n.profileType == canditateOrboundary.Boundary).Single());

                    List<IndexedBoundary> passageBoundariesDatabase = BoundaryConverter.StopWordToWord(boundariesDatabase,
                        databaseDocumentProfiles.Where(n => n.profileType == canditateOrboundary.Boundary).Single());
                    //Examine each set of passages
                    for (int i = 0; i < passageBoundariesDatabase.Count; i++)
                    {
                        //string resultMessage = "";
                        string inputPassage = "";
                        string databasePassage = "";
                        //=================Step-3 Creating the profiles of letter ngrams=================================
                        //Step-3.1
                        //Get the document's profile in letters ngrams
                        ProfileCharacter passageInput = ProfileCharacterBuilder.GetProfileCharacter(
                                                inputDocumentWords, passageBoundariesInput[i], 3);
                        ProfileCharacter passageDatabase = ProfileCharacterBuilder.GetProfileCharacter(
                                               databaseDocumentWords, passageBoundariesDatabase[i], 3);
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
                        for (int j = passageBoundariesInput[i].lower;
                            j <= passageBoundariesInput[i].upper; j++)
                        {
                            inputPassage += $"{inputDocumentWords[j]} ";
                        }

                        for (int j = passageBoundariesDatabase[i].lower; j <= passageBoundariesDatabase[i].upper; j++)
                        {
                            databasePassage += $"{databaseDocumentWords[j]} ";
                        }

                        CommonPassage commonPassage = new CommonPassage
                        {
                            OriginalDocumentPassage = databasePassage,
                            SuspiciousDocumentPassage = inputPassage,
                            SimilarityScore = similarityScore
                        };
                        comparison.CommonPassages.Add(commonPassage);
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