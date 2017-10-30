using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DataModels;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Common.Interfaces
{
    public interface IRawProcessing : IService
    {
        Task<bool> DocumentReceivedAsync(Document document);

    }
}
