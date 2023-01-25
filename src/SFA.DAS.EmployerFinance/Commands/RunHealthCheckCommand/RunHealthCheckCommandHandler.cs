using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;
using SFA.DAS.EmployerFinance.Api.Client;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Models;

namespace SFA.DAS.EmployerFinance.Commands.RunHealthCheckCommand
{
    public class RunHealthCheckCommandHandler : IRequestHandler<RunHealthCheckCommand,Unit>
    {
        private readonly Lazy<EmployerFinanceDbContext> _db;
        private readonly IEmployerFinanceApiClient _employerFinanceApiClient;        

        public RunHealthCheckCommandHandler(Lazy<EmployerFinanceDbContext> db, IEmployerFinanceApiClient employerFinanceApiClient)
        {
            _db = db;
            _employerFinanceApiClient = employerFinanceApiClient;
        }

        public async Task<Unit> Handle(RunHealthCheckCommand request,CancellationToken cancellationToken)
        {
            var healthCheck = new HealthCheck(request.UserRef.Value);

            await healthCheck.Run(_employerFinanceApiClient.HealthCheck);

            _db.Value.HealthChecks.Add(healthCheck);

            return Unit.Value;
        }
    }
}