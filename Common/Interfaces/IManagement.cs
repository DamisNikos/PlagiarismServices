using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IManagement : IService
    {
        Task<bool> ExaminedDocumentsAsync(int count, int targetCounter, string userEmail, string documentName);

        Task<bool> DocumentHashReceivedAsync(string docHash, string docUser);

    }
}
