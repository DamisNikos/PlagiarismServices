using Common.DataModels;
using Microsoft.ServiceFabric.Services.Remoting;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IRawProcessing : IService
    {
        Task<bool> DocumentReceivedAsync(Document document);
    }
}