using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Queries.FindAccountCoursePayments
{
    public class FindAccountCoursePaymentsQueryHandler : IRequestHandler<FindAccountCoursePaymentsQuery,
        FindAccountCoursePaymentsResponse>
    {
        private readonly IValidator<FindAccountCoursePaymentsQuery> _validator;
        private readonly IDasLevyService _dasLevyService;
        private readonly IEncodingService _encodingService;

        public FindAccountCoursePaymentsQueryHandler(
            IValidator<FindAccountCoursePaymentsQuery> validator,
            IDasLevyService dasLevyService,
            IEncodingService encodingService)
        {
            _validator = validator;
            _dasLevyService = dasLevyService;
            _encodingService = encodingService;
        }

        public async Task<FindAccountCoursePaymentsResponse> Handle(FindAccountCoursePaymentsQuery message, CancellationToken cancellationToken)
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

            var accountId = _encodingService.Decode(message.HashedAccountId, EncodingType.AccountId);
            var transactions = await _dasLevyService.GetAccountCoursePaymentsByDateRange<PaymentTransactionLine>
                (accountId, message.UkPrn, message.CourseName, message.CourseLevel, message.PathwayCode, message.FromDate, message.ToDate);

            if (!transactions.Any())
            {
                return null;
            }

            var firstTransaction = transactions.First();

            return new FindAccountCoursePaymentsResponse
            {
                ProviderName = firstTransaction.ProviderName,
                CourseName = firstTransaction.CourseName,
                CourseLevel = firstTransaction.CourseLevel,
                PathwayName = firstTransaction.PathwayName,
                TransactionDate = firstTransaction.TransactionDate,
                DateCreated = firstTransaction.DateCreated,
                Transactions = transactions.ToList(),
                Total = transactions.Sum(c => c.LineAmount)
            };
        }
    }
}
