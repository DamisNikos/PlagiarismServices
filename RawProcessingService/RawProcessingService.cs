﻿using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

using Common.DataModels;
using Common.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using RawProcessingService.Rawprocessing;
using System.IO;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Client;
using System.Runtime.Serialization.Formatters.Binary;

namespace RawProcessingService
{
   
    internal sealed class RawProcessingService : StatelessService, IRawProcessing
    {
        public RawProcessingService(StatelessServiceContext context)
            : base(context)
        { }

        public Task<bool> DocumentReceivedAsync(Document document)
        {
            ServiceEventSource.Current.ServiceMessage(this.Context, $"Received document {document.DocName} at RawProcessingService");


            ByteArray2FileConverter.ByteArray2File(document);
            document.words = DocumentParser.GetText(Directory.GetCurrentDirectory()+"\\"+document.DocName, this.Context);
            ByteArray2FileConverter.DeleteFile(document);
            List<StopWord> listofStopWords = ProfileStopWordBuilder.GetStopWordPresentation(document.words, this.Context);
            document.profiles.Add(ProfileStopWordBuilder.GetProfileStopWord(listofStopWords, 11, canditateOrboundary.Canditate));
            document.profiles.Add(ProfileStopWordBuilder.GetProfileStopWord(listofStopWords, 8, canditateOrboundary.Boundary));

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

            IManagement managementClient = ServiceProxy.Create<IManagement>
                (new Uri("fabric:/PlagiarismServices/ManagementService"), new ServicePartitionKey(1));

            managementClient.DocumentReceivedAsync(document.DocHash);

            return Task.FromResult<bool>(true);
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
           
            return new[] { new ServiceInstanceListener(context => this.CreateServiceRemotingListener(context)) };

        }


    }
}
