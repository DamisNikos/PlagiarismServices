using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Common.DataModels;
using System.Linq;

namespace PlagiarismAlgorithmService.Interfaces
{
    public interface IPlagiarismAlgorithmService : IActor
    {
        Task<bool> CompareDocuments(Document inputDocument, List<Document> suspiciousDocuments);
    }
}
