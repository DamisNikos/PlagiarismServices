﻿using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IManagement : IService
    {
        Task<bool> ExaminedDocumentsAsync(int count);

        Task<bool> DocumentReceivedAsync(string docHash);

    }
}
