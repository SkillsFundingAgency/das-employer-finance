﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Interfaces
{
    public interface IClientContentApiClient
    {
        Task<string> Get(string type, string applicationId);
    }
}
