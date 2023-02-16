using MediatR;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.Encoding;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Queries.GetAccountTransactionSummary
{
    public class GetAccountTransactionSummaryQueryHandler : IRequestHandler<GetAccountTransactionSummaryRequest, GetAccountTransactionSummaryResponse>
    {
        private readonly IEncodingService _encodingService;
        private readonly ITransactionRepository _transactionRepository;

        public GetAccountTransactionSummaryQueryHandler(IEncodingService encodingService, ITransactionRepository transactionRepository)
        {
            _encodingService = encodingService;
            _transactionRepository = transactionRepository;
        }

        public async Task<GetAccountTransactionSummaryResponse> Handle(GetAccountTransactionSummaryRequest message,CancellationToken cancellationToken)
        {
            var accountId = _encodingService.Decode(message.HashedAccountId, EncodingType.AccountId);
            var result = await _transactionRepository.GetAccountTransactionSummary(accountId);

            return new GetAccountTransactionSummaryResponse { Data = result };
        }
    }
}
