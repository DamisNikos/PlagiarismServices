using Common.DataModels;
using Common.Interfaces;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Newtonsoft.Json;
using RawProcessingService.Rawprocessing;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RawProcessingService
{
    internal sealed class RawProcessingService : StatelessService, IRawProcessing
    {
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[] { new ServiceInstanceListener(context => this.CreateServiceRemotingListener(context)) };
        }

        public async Task<bool> DocumentReceivedAsync(Document document)
        {
            ServiceEventSource.Current.ServiceMessage(this.Context, $"PLS: Received document {document.DocName} at RawProcessingService");

            ByteArray2FileConverter.ByteArray2File(document);
            List<string> words = DocumentParser.GetText(Directory.GetCurrentDirectory() + "\\" + document.DocName, this.Context);
            ByteArray2FileConverter.DeleteFile(document);

            List<StopWord> listofStopWords = ProfileStopWordBuilder.GetStopWordPresentation(words, this.Context);

            List<Profile> profiles = new List<Profile>();
            profiles.Add(ProfileStopWordBuilder.GetProfileStopWord(listofStopWords, 11, canditateOrboundary.Canditate));
            profiles.Add(ProfileStopWordBuilder.GetProfileStopWord(listofStopWords, 8, canditateOrboundary.Boundary));

            document.words = JsonConvert.SerializeObject(words);
            document.profiles = JsonConvert.SerializeObject(profiles);

            ServiceEventSource.Current.ServiceMessage(this.Context, $"PLS: {document.DocName} preprocessing completed");

            using (var context = new DocumentContext())
            {
                context.Documents.Add(document);

                try
                {
                    context.SaveChanges();
                    Console.WriteLine($"File Added");
                }
                catch (Exception e)
                {
                    ServiceEventSource.Current.ServiceMessage(this.Context, $"Error upon writing file into the the database:\n{e.InnerException}");
                }
            }

            ServiceEventSource.Current.ServiceMessage(this.Context, $"PLS: {document.DocName} saved in the database");

            var serviceName = new Uri("fabric:/PlagiarismServices/ManagerService");
            using (var client = new FabricClient())
            {
                var partitions = await client.QueryManager.GetPartitionListAsync(serviceName);

                var partitionInformation = (Int64RangePartitionInformation)partitions.FirstOrDefault().PartitionInformation;
                IManagement managementClient = ServiceProxy.Create<IManagement>(serviceName, new ServicePartitionKey(partitionInformation.LowKey));

                managementClient.DocumentHashReceivedAsync(document.DocHash, document.DocUser);
            }

            //IManagement managementClient = ServiceProxy.Create<IManagement>
            //    (new Uri("fabric:/PlagiarismServices/ManagementService"), new ServicePartitionKey(1));
            return true;
        }

        public RawProcessingService(StatelessServiceContext context)
                    : base(context)
        { }
    }
}