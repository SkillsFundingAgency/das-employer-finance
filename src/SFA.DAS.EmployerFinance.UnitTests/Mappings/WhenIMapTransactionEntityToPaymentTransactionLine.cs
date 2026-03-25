using AutoMapper;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerFinance.Mappings;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Models.Transaction;

namespace SFA.DAS.EmployerFinance.UnitTests.Mappings;

public class WhenIMapTransactionEntityToPaymentTransactionLine
{
    private IMapper _mapper;

    [SetUp]
    public void Arrange()
    {
        var config = new MapperConfiguration(c => c.AddProfile<TransactionMappings>());
        _mapper = config.CreateMapper();
    }

    [TestCase("Apprenticeship", LearningType.Apprenticeship)]
    [TestCase("ApprenticeshipUnit", LearningType.ApprenticeshipUnit)]
    [TestCase(null, LearningType.Apprenticeship)]
    public void ThenLearningTypeShouldBeMappedFromString(string? learningTypeString, LearningType expectedLearningType)
    {
        // Arrange
        var entity = new TransactionEntity { LearningType = learningTypeString };

        // Act
        var result = _mapper.Map<PaymentTransactionLine>(entity);

        // Assert
        result.LearningType.Should().Be(expectedLearningType);
    }
}
