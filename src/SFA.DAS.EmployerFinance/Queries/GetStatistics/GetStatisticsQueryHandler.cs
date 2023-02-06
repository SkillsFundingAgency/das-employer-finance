//using System;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using MediatR;
//using SFA.DAS.EmployerFinance.Data;
//using SFA.DAS.EmployerFinance.Api.Types;

//namespace SFA.DAS.EmployerFinance.Queries.GetStatistics
//{
//    public class GetStatisticsQueryHandler : IRequestHandler<GetStatisticsQuery, GetStatisticsResponse>
//    {
//        private readonly Lazy<EmployerFinanceDbContext> _financeDb;

//        public GetStatisticsQueryHandler(Lazy<EmployerFinanceDbContext> financeDb)
//        {
//            _financeDb = financeDb;
//        }

//        public async Task<GetStatisticsResponse> Handle(GetStatisticsQuery message, CancellationToken cancellationToken)
//        {
//            var accountsCount = await _financeDb.Value.Accounts.Count(cancellationToken);
//            var legalEntitiesCount = await _financeDb.Value.LegalEntities.CountAsync(cancellationToken);
//            var payeSchemesCount = await _financeDb.Value.Payees.CountAsync(cancellationToken);
//            var agreementsCount = await _accountDb.Value.Agreements.Where(a => a.StatusId == EmployerAgreementStatus.Signed)
//                                                                             .CountAsync(cancellationToken);
//            _financeDb.Value.
//            var statistics = new Statstics
//            {
//                TotalAccounts = accountsCount,
//                TotalLegalEntities = legalEntitiesCount,
//                TotalPayeSchemes = payeSchemesCount,
//                TotalAgreements = agreementsCount
//            };

//            return new GetStatisticsResponse
//            {
//                Statistics = statistics
//            };
//        }
//    }
//}
