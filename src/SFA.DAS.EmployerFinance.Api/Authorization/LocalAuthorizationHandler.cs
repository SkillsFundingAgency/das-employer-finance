﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Authorization.Context;
using SFA.DAS.Authorization.Handlers;
using SFA.DAS.Authorization.Results;

namespace SFA.DAS.EmployerFinance.Api.Authorization
{
    public class LocalAuthorizationHandler : IAuthorizationHandler
    {
        public string Prefix => "SFA.DAS.EmployerFinance";

        public Task<AuthorizationResult> GetAuthorizationResult(IReadOnlyCollection<string> options, IAuthorizationContext authorizationContext)
        {
            var result = new AuthorizationResult();

            return Task.FromResult(result);
        }
    }
}