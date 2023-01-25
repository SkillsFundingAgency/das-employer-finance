using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Dtos;

namespace SFA.DAS.EmployerFinance.Queries.GetHealthCheck
{
    public class GetHealthCheckQueryHandler : IRequestHandler<GetHealthCheckQuery, GetHealthCheckQueryResponse>
    {
        private readonly Lazy<EmployerFinanceDbContext> _db;
        private readonly IConfigurationProvider _configurationProvider;

        public GetHealthCheckQueryHandler(Lazy<EmployerFinanceDbContext> db, IConfigurationProvider configurationProvider)
        {
            _db = db;
            _configurationProvider = configurationProvider;
        }

        public async Task<GetHealthCheckQueryResponse> Handle(GetHealthCheckQuery message,CancellationToken cancellationToken)
        {
            var healthCheck = await _db.Value.HealthChecks
                .OrderByDescending(h => h.Id)
                .ProjectTo<HealthCheckDto>(_configurationProvider)
                .FirstOrDefaultAsync();

            return new GetHealthCheckQueryResponse
            {
                HealthCheck = healthCheck
            };
        }
    }
}