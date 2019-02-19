using Microsoft.ServiceFabric.Services.Remoting;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IManagement : IService
    {asdfasdf
        Task<bool> ExaminedDocumentsAsync(int count, int targetCounter, string userEmail, string documentName);

        Task<bool> DocumentHashReceivedAsync(string docHash, string docUser);
    }
}asdasdasds



EIMAI O NIKOLAS