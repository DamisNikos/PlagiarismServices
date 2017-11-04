using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using System.Data.Entity;
using PlagiarismAlgorithmService.Interfaces;
using Common.DataModels;
using Common.ResultsModel;
using Common.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Client;
using System.Fabric;

namespace PlagiarismAlgorithmService
{

    [StatePersistence(StatePersistence.Persisted)]
    internal class PlagiarismAlgorithmService : Actor, IPlagiarismAlgorithmService
    {
        public PlagiarismAlgorithmService(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        public async Task<bool> CompareDocuments(Document inputDocument, List<Document> suspiciousDocuments)
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");


            Parallel.ForEach(suspiciousDocuments, databaseDocument =>
            {
                Comparison comparison = new Comparison
                {
                    ComparisonUser = inputDocument.DocUser,
                    OriginalDocumentName = inputDocument.DocName,
                    SuspiciousDocumentName = databaseDocument.DocName,
                    CommonPassages = new List<CommonPassage>()
                };

                //Restore StopWords List for documents retrieved from database
                foreach (ProfileStopWord profile in databaseDocument.profiles)
                {
                    foreach (StopNGram ngram in profile.ngrams)
                    {
                        string[] stopWords = ngram.stopWordsInString.Split(',');
                        List<StopWord> stopwordsList = new List<StopWord>();
                        for (int i = 0; i < stopWords.Length; i++)
                        {
                            stopwordsList.Add(new StopWord { word = stopWords[i] });
                        }
                        ngram.stopWords = stopwordsList;
                    }
                }

                //=======================++==========Step-1 Canditate Retrieval=========================================
                //Step-1.1
                //Get the intersected(common ngrams) profile (n = 11) of the 2 documents
                ProfileStopWord canditateIntersection = ProfileIntersection.IntersectProfiles(
                     inputDocument.profiles.Where(n => n.profileType == canditateOrboundary.Canditate).Single(),
                     databaseDocument.profiles.Where(n => n.profileType == canditateOrboundary.Canditate).Single());

                //Step-1.1.1
                //Check to see if any common ngrams found
                if (canditateIntersection.ngrams.Count != 0)
                {
                    //Step-1.2
                    //Apply criterion (1) to the canditate intersection to filter out false positives
                    ProfileStopWord canditateIntersectionResults = Criteria.ApplyCanditateRetrievalCriterion(canditateIntersection);

                    //Step-1.1.1
                    //Check to see if any common ngrams found after the applying criterion(1)
                    if (canditateIntersectionResults.ngrams.Count != 0)
                    {
                        //===============++++++=============Step-2 Passage Boundary Detection========++==========================

                        //Step-2.2 REDO Step-1.1 to intersect the profiles (n = 8 )
                        ProfileStopWord boundaryIntersection = ProfileIntersection.IntersectProfiles(
                             inputDocument.profiles.Where(n => n.profileType == canditateOrboundary.Boundary).Single(),
                             databaseDocument.profiles.Where(n => n.profileType == canditateOrboundary.Boundary).Single());
                        //Step-2.4
                        //Apply criterion (2) to avoid noise of coincidental matches
                        ProfileStopWord boundaryIntersectionResults = Criteria.ApplyMatchCriterion(boundaryIntersection);
                        //Step-2.5
                        //Get a list M of matched Ngrams
                        //where members of M are ordered according to the first appearance of a match in the suspicious document
                        List<int[]> M = Criteria.MatchedNgramSet(
                             databaseDocument.profiles.Where(n => n.profileType == canditateOrboundary.Boundary).Single(),
                             inputDocument.profiles.Where(n => n.profileType == canditateOrboundary.Boundary).Single(),
                             boundaryIntersectionResults);
                        //Step-2.6 Apply criterion (3)
                        List<List<IndexedBoundary>> boundaries = BoundaryDetection.DetectInitialSet(M, 100);
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
                            databaseDocument.profiles.Where(n => n.profileType == canditateOrboundary.Boundary).Single());

                        List<IndexedBoundary> passageBoundariesOriginal = BoundaryConverter.StopWordToWord(boundariesOriginal,
                            inputDocument.profiles.Where(n => n.profileType == canditateOrboundary.Boundary).Single());

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
                                                    databaseDocument.words, passageBoundariesSuspicious[i], 3);
                            ProfileCharacter passageOriginal = ProfileCharacterBuilder.GetProfileCharacter(
                                                   inputDocument.words, passageBoundariesOriginal[i], 3);

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
                                suspiciousPassage += $"{databaseDocument.words[j].word} ";
                            }
                            for (int j = passageBoundariesOriginal[i].lower; j <= passageBoundariesOriginal[i].upper; j++)                                               
                            {
                                originalPassage += $"{inputDocument.words[j].word} ";
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
                    using (var context = new ResultsContext())
                    {

                        foreach (CommonPassage passage in comparison.CommonPassages)
                        {
                            context.Passages.Add(passage);
                        }
                        context.Comparisons.Add(comparison);


                        context.SaveChanges();
                    }
                }

            }
            );

            //IManagement managementClient = ServiceProxy.Create<IManagement>
            //                (new Uri("fabric:/PlagiarismServices/ManagementService"), new ServicePartitionKey(1));
            //var result = await managementClient.ExaminedDocumentsAsync(suspiciousDocuments.Count);
            return true;
        }

        public async Task<bool> CompareDocumentsByHash(string inputDocumentHash, List<string> suspiciousDocumentHashes, int targetCounter)
        {

            ActorEventSource.Current.ActorMessage(this, "Targets received");

            //Document inputDocument, List< Document > suspiciousDocuments
            Document inputDocument;
            List<Document> suspiciousDocuments = new List<Document>();
            using (var originalcontext = new DocumentContext())
            {
                inputDocument = originalcontext.Documents
                    .AsNoTracking()
                    .Include(n => n.words)
                    .Include(n => n.profiles.Select(x => x.ngrams))
                    .Where(n => n.DocHash.Equals(inputDocumentHash))
                    .OrderByDescending(n => n.documentID).FirstOrDefault();
            }

            foreach (ProfileStopWord profile in inputDocument.profiles)
            {
                foreach (StopNGram ngram in profile.ngrams)
                {
                    string[] stopWords = ngram.stopWordsInString.Split(',');
                    List<StopWord> stopwordsList = new List<StopWord>();
                    for (int i = 0; i < stopWords.Length; i++)
                    {
                        stopwordsList.Add(new StopWord { word = stopWords[i] });
                    }
                    ngram.stopWords = stopwordsList;
                }
            }

            ActorEventSource.Current.ActorMessage(this, $"Input document: {inputDocument.DocName} retrieved");

            foreach (string suspiciousHash in suspiciousDocumentHashes)
            {
                using (var suspiciouscontext = new DocumentContext())
                {
                    suspiciousDocuments.Add(suspiciouscontext.Documents
                                .AsNoTracking()
                                .Include(n => n.words)
                                .Include(n => n.profiles.Select(x => x.ngrams))
                                .Where(n => n.DocHash.Equals(suspiciousHash))
                                .OrderByDescending(n => n.documentID).FirstOrDefault());
                }
            }

            ActorEventSource.Current.ActorMessage(this, $"{suspiciousDocuments.Count} documents retrieved from database");

            Parallel.ForEach(suspiciousDocuments, databaseDocument =>
            {
                Comparison comparison = new Comparison
                {
                    ComparisonUser = inputDocument.DocUser,
                    OriginalDocumentName = inputDocument.DocName,
                    SuspiciousDocumentName = databaseDocument.DocName,
                    CommonPassages = new List<CommonPassage>()
                };

                //Restore StopWords List for documents retrieved from database
                foreach (ProfileStopWord profile in databaseDocument.profiles)
                {
                    foreach (StopNGram ngram in profile.ngrams)
                    {
                        string[] stopWords = ngram.stopWordsInString.Split(',');
                        List<StopWord> stopwordsList = new List<StopWord>();
                        for (int i = 0; i < stopWords.Length; i++)
                        {
                            stopwordsList.Add(new StopWord { word = stopWords[i] });
                        }
                        ngram.stopWords = stopwordsList;
                    }
                }

                //=======================++==========Step-1 Canditate Retrieval=========================================
                //Step-1.1
                //Get the intersected(common ngrams) profile (n = 11) of the 2 documents
                ProfileStopWord canditateIntersection = ProfileIntersection.IntersectProfiles(
                     inputDocument.profiles.Where(n => n.profileType == canditateOrboundary.Canditate).Single(),
                     databaseDocument.profiles.Where(n => n.profileType == canditateOrboundary.Canditate).Single());

                //Step-1.1.1
                //Check to see if any common ngrams found
                if (canditateIntersection.ngrams.Count != 0)
                {
                    //Step-1.2
                    //Apply criterion (1) to the canditate intersection to filter out false positives
                    ProfileStopWord canditateIntersectionResults = Criteria.ApplyCanditateRetrievalCriterion(canditateIntersection);

                    //Step-1.1.1
                    //Check to see if any common ngrams found after the applying criterion(1)
                    if (canditateIntersectionResults.ngrams.Count != 0)
                    {
                        //===============++++++=============Step-2 Passage Boundary Detection========++==========================

                        //Step-2.2 REDO Step-1.1 to intersect the profiles (n = 8 )
                        ProfileStopWord boundaryIntersection = ProfileIntersection.IntersectProfiles(
                             inputDocument.profiles.Where(n => n.profileType == canditateOrboundary.Boundary).Single(),
                             databaseDocument.profiles.Where(n => n.profileType == canditateOrboundary.Boundary).Single());
                        //Step-2.4
                        //Apply criterion (2) to avoid noise of coincidental matches
                        ProfileStopWord boundaryIntersectionResults = Criteria.ApplyMatchCriterion(boundaryIntersection);
                        //Step-2.5
                        //Get a list M of matched Ngrams
                        //where members of M are ordered according to the first appearance of a match in the suspicious document
                        List<int[]> M = Criteria.MatchedNgramSet(
                             databaseDocument.profiles.Where(n => n.profileType == canditateOrboundary.Boundary).Single(),
                             inputDocument.profiles.Where(n => n.profileType == canditateOrboundary.Boundary).Single(),
                             boundaryIntersectionResults);
                        //Step-2.6 Apply criterion (3)
                        List<List<IndexedBoundary>> boundaries = BoundaryDetection.DetectInitialSet(M, 100);
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
                            databaseDocument.profiles.Where(n => n.profileType == canditateOrboundary.Boundary).Single());

                        List<IndexedBoundary> passageBoundariesOriginal = BoundaryConverter.StopWordToWord(boundariesOriginal,
                            inputDocument.profiles.Where(n => n.profileType == canditateOrboundary.Boundary).Single());

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
                                                    databaseDocument.words, passageBoundariesSuspicious[i], 3);
                            ProfileCharacter passageOriginal = ProfileCharacterBuilder.GetProfileCharacter(
                                                   inputDocument.words, passageBoundariesOriginal[i], 3);

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
                                suspiciousPassage += $"{databaseDocument.words[j].word} ";
                            }
                            for (int j = passageBoundariesOriginal[i].lower; j <= passageBoundariesOriginal[i].upper; j++)
                            {
                                originalPassage += $"{inputDocument.words[j].word} ";
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
                    using (var context = new ResultsContext())
                    {

                        foreach (CommonPassage passage in comparison.CommonPassages)
                        {
                            context.Passages.Add(passage);
                        }
                        context.Comparisons.Add(comparison);


                        context.SaveChanges();
                    }
                }
            });

            ActorEventSource.Current.ActorMessage(this, "Updating counter in management service");

            var serviceName = new Uri("fabric:/PlagiarismServices/ManagerService");
            using (var client = new FabricClient())
            {
                var partitions = await client.QueryManager.GetPartitionListAsync(serviceName);

                var partitionInformation = (Int64RangePartitionInformation)partitions.FirstOrDefault().PartitionInformation;
                IManagement managementClient = ServiceProxy.Create<IManagement>(serviceName, new ServicePartitionKey(partitionInformation.LowKey));

                await managementClient.ExaminedDocumentsAsync(suspiciousDocumentHashes.Count, targetCounter, inputDocument.DocUser, inputDocument.DocName);

            }









            //IManagement managementClient = ServiceProxy.Create<IManagement>
            //    (new Uri("fabric:/PlagiarismServices/ManagementService"), new ServicePartitionKey(1));
            //var result = await managementClient.ExaminedDocumentsAsync(suspiciousDocumentHashes.Count, targetCounter, inputDocument.DocUser, inputDocument.DocName);

            return true;

        }
    }
}

