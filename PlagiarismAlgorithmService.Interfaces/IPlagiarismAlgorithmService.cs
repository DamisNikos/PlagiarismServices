using Microsoft.ServiceFabric.Actors;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlagiarismAlgorithmService.Interfaces
{
    public interface IPlagiarismAlgorithmService : IActor
    {
        Task<bool> CompareDocumentsByHash(string inputDocumentHash, List<string> suspiciousDocumentHashes, int targetCounter);
    }
}