using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Common.DataModels;
using Common.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using System.Data.Entity;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using PlagiarismAlgorithmService.Interfaces;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace ManagementService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class ManagementService : StatefulService, IRawProcessing, IManagement
    {
        public ManagementService(StatefulServiceContext context)
            : base(context)
        { }


        public async Task<bool> DocumentReceivedAsync(Document document)
        {
            ServiceEventSource.Current.ServiceMessage(this.Context, $"Received document {document.DocName} at ManagementService");
            List<Document> documentList = new List<Document>();
            ////////////////////////////
            var counter = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, int>>("myDictionary");
            using (var tx = this.StateManager.CreateTransaction())
            {
                await counter.AddOrUpdateAsync(tx, "Counter", 0, (k, v) => 0);
                await tx.CommitAsync();
            }
            ///////////////////////////////

            //                                       Reading from database
            using (var context = new DocumentContext())
            {
                int count = context.Documents.Count();
                ActorId actorid = ActorId.CreateRandom();

                ServiceEventSource.Current.ServiceMessage(this.Context, $"{count} documents found in base.");
                if (count == 0)
                {
                    ServiceEventSource.Current.ServiceMessage(this.Context, $"Should return to homepage");
                }
                else
                {
                    ServiceEventSource.Current.ServiceMessage(this.Context, $"Documents will be compared against input");

                    int skip = 0;
                    int take = 10;
                    int batchcounter = 0;

                    var batch = context.Documents.AsNoTracking().Include(n => n.words)
                           .Include(n => n.profiles.Select(x => x.ngrams))
                           .OrderBy(n => n.documentID).Skip(skip).Take(take).ToList();

                    while (batch.Any())
                    {
                        
                        var actor = ActorProxy.Create<IPlagiarismAlgorithmService>(actorid, "fabric:/PlagiarismServices");
                        var flag = actor.CompareDocuments(document, batch);

                        batchcounter += batch.Count;
                        ServiceEventSource.Current.ServiceMessage(this.Context, $"Current Batch Counter value:{batchcounter}");

                        

                        skip += take;
                        batch = context.Documents.AsNoTracking().Include(n => n.words)
                                               .Include(n => n.profiles.Select(x => x.ngrams))
                                               .OrderBy(n => n.documentID).Skip(skip).Take(take).ToList();
                    }

                }

            }

            using (var context = new DocumentContext())
            {
                context.Profiles.Add(document.profiles[0]);

                foreach (StopNGram ngram in document.profiles[0].ngrams)
                {
                    context.StopNGrams.Add(ngram);
                }
                foreach (Word word in document.words)
                {
                    context.Word.Add(word);
                }
                context.Documents.Add(document);

                context.SaveChanges();
            }


            using (var tx = this.StateManager.CreateTransaction())
            {
                var result = await counter.TryGetValueAsync(tx, "Counter");
                ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                            result.HasValue ? result.Value.ToString() : "Value does not exist.");

                await tx.CommitAsync();
            }

            return true;
        }

        public async Task<bool> ExaminedDocumentsAsync(int count)
        {
            var counter = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, int>>("myDictionary");
            using (var tx = this.StateManager.CreateTransaction())
            {
                var result = await counter.TryGetValueAsync(tx, "Counter");
                await counter.AddOrUpdateAsync(tx, "Counter", result.Value, (key, value) => value+=count);
                await tx.CommitAsync();
            }
            return true;
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[] { new ServiceReplicaListener(context => this.CreateServiceRemotingListener(context)) };
        }

    }
}
