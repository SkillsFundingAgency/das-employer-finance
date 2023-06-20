using AutoMapper;
using AutoMapper.QueryableExtensions;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Dtos;

namespace SFA.DAS.EmployerFinance.Queries.GetHealthCheck;

public class GetHealthCheckQueryHandler : IRequestHandler<GetHealthCheckQuery, GetHealthCheckQueryResponse>
{
    private readonly EmployerFinanceDbContext _db;
    private readonly IConfigurationProvider _configurationProvider;

    public GetHealthCheckQueryHandler(EmployerFinanceDbContext db, IConfigurationProvider configurationProvider)
    {
        _db = db;
        _configurationProvider = configurationProvider;
    }

    public async Task<GetHealthCheckQueryResponse> Handle(GetHealthCheckQuery message,CancellationToken cancellationToken)
    {
        var healthCheck = await _db.HealthChecks
            .OrderByDescending(h => h.Id)
            .ProjectTo<HealthCheckDto>(_configurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return new GetHealthCheckQueryResponse
        {
            HealthCheck = healthCheck
        };
    }
}