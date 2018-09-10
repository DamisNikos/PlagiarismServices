using Common.DataModels;
using Common.Interfaces;
using Common.ResultsModel;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
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

        public async Task<bool> CompareDocumentsByHash(string inputDocumentHash, List<string> databaseDocumentHashes, int targetCounter)
        {
            const int thetaG = 100;
            ActorEventSource.Current.ActorMessage(this, "PLS: Targets received");
            ProcessDocument inputDocument = DataRetriever.RetrieveDocument(inputDocumentHash);
            ActorEventSource.Current.ActorMessage(this, $"PLS: Input document: {inputDocument.document.DocName} retrieved");
            List<ProcessDocument> databaseDocuments = new List<ProcessDocument>();
            foreach (string databaseHash in databaseDocumentHashes)
            {
                databaseDocuments.Add(DataRetriever.RetrieveDocument(databaseHash));
            }
            ActorEventSource.Current.ActorMessage(this, $"PLS: {databaseDocuments.Count} documents retrieved from database");
            Parallel.ForEach(databaseDocuments, databaseDocument =>
            {
                Comparison comparison = new Comparison
                {
                    ComparisonUser = inputDocument.document.DocUser,
                    OriginalDocumentName = databaseDocument.document.DocName,
                    SuspiciousDocumentName = inputDocument.document.DocName,
                    CommonPassages = new List<CommonPassage>()
                };
                //=======================++==========Step-1 Candidate Retrieval=========================================
                if (CandidateRetrieval.Retrieve(
                    inputDocument.profiles.Where(n => n.profileType == canditateOrboundary.Canditate).Single(),
                     databaseDocument.profiles.Where(n => n.profileType == canditateOrboundary.Canditate).Single()))
                {
                    //===============++++++=============Step-2 Passage Boundary Detection========++==========================
                    //Step-2.6 Apply criterion (3)
                    List<List<IndexedBoundary>> boundaries = PassageDetectionStep.GetInitialSetBoundaries(
                        inputDocument.profiles.Where(n => n.profileType == canditateOrboundary.Boundary).Single(),
                         databaseDocument.profiles.Where(n => n.profileType == canditateOrboundary.Boundary).Single(),
                         thetaG);

                    //Step-2.7 Apply criterion (4)
                    List<IndexedBoundary> boundariesInput = new List<IndexedBoundary>();
                    List<IndexedBoundary> boundariesDatabase = new List<IndexedBoundary>();
                    foreach (List<IndexedBoundary> mBoundary in boundaries)
                    {
                        boundariesInput.Add(mBoundary[0]);
                        boundariesDatabase.Add(mBoundary[1]);
                    }

                    comparison.CommonPassages = PassageDetectionStep.RetrieveExaminedPassages(
                        boundariesInput,
                        boundariesDatabase,
                        inputDocument.profiles.Where(n => n.profileType == canditateOrboundary.Boundary).Single(),
                        databaseDocument.profiles.Where(n => n.profileType == canditateOrboundary.Boundary).Single(),
                        inputDocument.words,
                        databaseDocument.words);
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

                await managementClient.ExaminedDocumentsAsync(databaseDocumentHashes.Count, targetCounter, inputDocument.document.DocUser, inputDocument.document.DocName);
            }

            return true;
        }
    }
}