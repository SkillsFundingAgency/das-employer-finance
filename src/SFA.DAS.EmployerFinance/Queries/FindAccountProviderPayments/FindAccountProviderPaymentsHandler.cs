using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.EmployerFinance.Models.Payments;
using System.Threading;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Queries.FindAccountProviderPayments
{
    public class FindAccountProviderPaymentsHandler : IRequestHandler<FindAccountProviderPaymentsQuery, FindAccountProviderPaymentsResponse>
    {
        private readonly IValidator<FindAccountProviderPaymentsQuery> _validator;
        private readonly IDasLevyService _dasLevyService;
        private readonly IEncodingService _encodingService;

        public FindAccountProviderPaymentsHandler(
            IValidator<FindAccountProviderPaymentsQuery> validator,
            IDasLevyService dasLevyService,
            IEncodingService encodingService)
        {
            _validator = validator;
            _dasLevyService = dasLevyService;
            _encodingService = encodingService;
        }

        public async Task<FindAccountProviderPaymentsResponse> Handle(FindAccountProviderPaymentsQuery message,CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(message);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            if (validationResult.IsUnauthorized)
            {
                throw new UnauthorizedAccessException();
            }

            var accountId = _encodingService.Decode(message.HashedAccountId,EncodingType.AccountId);
            var transactions = await _dasLevyService.GetAccountProviderPaymentsByDateRange<PaymentTransactionLine>
                                    (accountId, message.UkPrn, message.FromDate, message.ToDate);

            if (!transactions.Any())
            {
                return null;//TODO
            }

            var firstTransaction = transactions.First();

            return new FindAccountProviderPaymentsResponse
            {
                ProviderName = firstTransaction.ProviderName,
                TransactionDate = firstTransaction.TransactionDate,
                DateCreated = firstTransaction.DateCreated,
                Transactions = transactions.ToList(),
                Total = transactions.Sum(c => c.LineAmount)
            };
        }
    }
}