﻿using SFA.DAS.EAS.Portal.Database.Models;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EAS.Portal.Application.Services
{
    public interface IAccountsService
    {
        Task<Account> Get(long id, CancellationToken cancellationToken = new CancellationToken());
        Task Save(string messageId, Account account, CancellationToken cancellationToken = new CancellationToken()); // TODO: move messageId from here
    }
}
