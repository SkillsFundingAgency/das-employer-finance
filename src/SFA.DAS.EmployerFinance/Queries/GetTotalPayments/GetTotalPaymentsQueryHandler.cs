using SFA.DAS.EmployerFinance.Data;

namespace SFA.DAS.EmployerFinance.Queries.GetTotalPayments;

public class GetTotalPaymentsQueryHandler : IRequestHandler<GetTotalPaymentsQuery, GetTotalPaymentsResponse>
{
    private readonly EmployerFinanceDbContext _financeDb;

    public GetTotalPaymentsQueryHandler(EmployerFinanceDbContext financeDb)
    {
        _financeDb = financeDb;
    }

    public async Task<GetTotalPaymentsResponse> Handle(GetTotalPaymentsQuery message,CancellationToken cancellationToken)
    {
        var paymentsCount = await _financeDb.Payments.CountAsync(cancellationToken);

        return new GetTotalPaymentsResponse
        {
            TotalPayments = paymentsCount
        };
    }
}