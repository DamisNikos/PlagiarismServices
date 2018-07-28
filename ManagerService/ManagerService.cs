using Common.DataModels;
using Common.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using PlagiarismAlgorithmService.Interfaces;
using System.Collections.Generic;
using System.Data.Entity;
using System.Fabric;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ManagerService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class ManagerService : StatefulService, IManagement
    {
        public ManagerService(StatefulServiceContext context)
            : base(context)
        { }

        public async Task<bool> DocumentHashReceivedAsync(string docHash, string docUser)
        {
            int targetCounter;

            ServiceEventSource.Current.ServiceMessage(this.Context, $"PLS: Received document {docHash} at ManagementService");
            List<Document> documentList = new List<Document>();

            var counter = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, int>>("myDictionary");
            using (var tx = this.StateManager.CreateTransaction())
            {
                await counter.AddOrUpdateAsync(tx, "Counter", 0, (k, v) => 0);
                await tx.CommitAsync();
            }

            using (var documentsToCompare = new DocumentContext())
            {
                int skip = 0;
                int take = 3;

                targetCounter = documentsToCompare.Documents.Where(n => n.DocUser.Equals(docUser)).Count() - 1;

                var batchOfDocuments = documentsToCompare.Documents
                    .AsNoTracking()
                    .Where(n => n.DocUser.Equals(docUser))
                    .Where(n => !n.DocHash.Equals(docHash))
                    .OrderByDescending(n => n.documentID)
                    .Select(n => n.DocHash)
                    .Skip(skip).Take(take)
                    .ToList();

                ServiceEventSource.Current.ServiceMessage(this.Context, $"PLS: Sending {docHash} to algorithm service");

                ActorId actorid = ActorId.CreateRandom();
                while (batchOfDocuments.Any())
                {
                    var actor = ActorProxy.Create<IPlagiarismAlgorithmService>(actorid, "fabric:/PlagiarismServices");
                    var flag = actor.CompareDocumentsByHash(docHash, batchOfDocuments, targetCounter);

                    skip += take;
                    batchOfDocuments = documentsToCompare.Documents
                                                         .AsNoTracking()
                                                         .Where(n => n.DocUser.Equals(docUser))
                                                         .Where(n => !n.DocHash.Equals(docHash))
                                                         .OrderByDescending(n => n.documentID)
                                                         .Select(n => n.DocHash)
                                                         .Skip(skip).Take(take)
                                                         .ToList();
                }
            }

            return true;
        }

        public async Task<bool> ExaminedDocumentsAsync(int count, int targetCounter, string userEmail, string documentName)
        {
            var counter = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, int>>("myDictionary");

            using (var tx = this.StateManager.CreateTransaction())
            {
                var result = await counter.TryGetValueAsync(tx, "Counter");
                await counter.AddOrUpdateAsync(tx, "Counter", result.Value, (key, value) => value += count);

                await tx.CommitAsync();
            }

            using (var tx = this.StateManager.CreateTransaction())
            {
                var result = await counter.TryGetValueAsync(tx, "Counter");
                if (result.Value == targetCounter)
                {
                    ServiceEventSource.Current.ServiceMessage(this.Context, $"PLS: Sending mail about {documentName} to {userEmail}");

                    MailMessage mail = new MailMessage();
                    SmtpClient SmtpServer = new SmtpClient("mail.ceid.upatras.gr");

                    mail.From = new MailAddress("damis@ceid.upatras.gr");
                    mail.To.Add("nikosdamis@gmail.com");
                    mail.Subject = "Plagiarism Services Update";
                    mail.Body = $"Your document {documentName} analysis has been completed. Visit our site to check out the results.";

                    SmtpServer.Port = 587;
                    SmtpServer.Credentials = new System.Net.NetworkCredential("damis", "pebkc$5202l");
                    SmtpServer.EnableSsl = true;

                    SmtpServer.Send(mail);
                }

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