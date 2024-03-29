﻿using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Client
{
    public interface ISecureHttpClient
    {
        Task<string> GetAsync(string url, CancellationToken cancellationToken = new CancellationToken());
    }
}
