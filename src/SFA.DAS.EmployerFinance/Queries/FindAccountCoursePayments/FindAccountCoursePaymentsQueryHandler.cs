using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Queries.FindAccountCoursePayments;

public class FindAccountCoursePaymentsQueryHandler(IValidator<FindAccountCoursePaymentsQuery> validator,
    IDasLevyService dasLevyService,
    IEncodingService encodingService)
    : IRequestHandler<FindAccountCoursePaymentsQuery,
        FindAccountCoursePaymentsResponse>
{
    public async Task<FindAccountCoursePaymentsResponse> Handle(FindAccountCoursePaymentsQuery message, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(message);

        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        var accountId = encodingService.Decode(message.HashedAccountId, EncodingType.AccountId);
        var transactions = await dasLevyService.GetAccountCoursePaymentsByDateRange<PaymentTransactionLine>
            (accountId, message.UkPrn, message.CourseName, message.CourseLevel, message.PathwayCode, message.FromDate, message.ToDate);

        if (transactions is {Length: 0})
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
            LearningType = firstTransaction.LearningType,
            TransactionDate = firstTransaction.TransactionDate,
            DateCreated = firstTransaction.DateCreated,
            Transactions = transactions.ToList(),
            CohortReference = EncodeCohortReference(firstTransaction.CohortId),
            Total = transactions.Sum(c => c.LineAmount)
        };
    }

    private string EncodeCohortReference(long? cohortId)
    {
        return !cohortId.HasValue
            ? null
            : encodingService.Encode(cohortId.Value, EncodingType.CohortReference);
    }
}