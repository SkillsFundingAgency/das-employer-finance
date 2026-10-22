using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Queries.GetLevyDeclarationsByAccountAndDateRange;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Testing.AutoFixture;
using System.ComponentModel.DataAnnotations;
using ValidationResult = SFA.DAS.EmployerFinance.Validation.ValidationResult;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetLevyDeclarationsByAccountAndDateRange;

[TestFixture]
internal class WhenIHandleGetLevyDeclarationsByAccountAndDateRangeQuery
{
    private Mock<IDasLevyRepository> _dasLevyRepository;
    private Mock<IValidator<GetLevyDeclarationsByAccountAndDateRangeQuery>> _validator;
    private GetLevyDeclarationsByAccountAndDateRangeQueryHandler _handler;

    [SetUp]
    public void Arrange()
    {
        _dasLevyRepository = new Mock<IDasLevyRepository>();
        _validator = new Mock<IValidator<GetLevyDeclarationsByAccountAndDateRangeQuery>>();
        _handler = new GetLevyDeclarationsByAccountAndDateRangeQueryHandler(
            _dasLevyRepository.Object,
            _validator.Object);
    }

    [Test, MoqAutoData]
    public async Task ThenReturnsDeclarations(
        GetLevyDeclarationsByAccountAndDateRangeQuery query,
        List<LevyDeclarationItem> levyDeclarations)
    {
        // Arrange
        _validator
            .Setup(x => x.ValidateAsync(query))
            .ReturnsAsync(new ValidationResult());

        _dasLevyRepository
            .Setup(x => x.GetAccountLevyDeclarationsByDateRange(query.AccountId, query.FromDate, query.ToDate))
            .ReturnsAsync(levyDeclarations);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result._.Should().BeEquivalentTo(levyDeclarations);
    }

    [Test, MoqAutoData]
    public async Task ThenThrowsValidationExceptionWhenValidationFails(
        GetLevyDeclarationsByAccountAndDateRangeQuery query)
    {
        // Arrange
        var validationResult = new ValidationResult();
        validationResult.AddError("AccountId", "AccountId is invalid");

        _validator
            .Setup(x => x.ValidateAsync(query))
            .ReturnsAsync(validationResult);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Test, MoqAutoData]
    public async Task ThenRepositoryIsNotCalledWhenValidationFails(
        GetLevyDeclarationsByAccountAndDateRangeQuery query)
    {
        // Arrange
        var validationResult = new ValidationResult();
        validationResult.AddError("AccountId", "AccountId is invalid");

        _validator
            .Setup(x => x.ValidateAsync(query))
            .ReturnsAsync(validationResult);

        // Act
        try { await _handler.Handle(query, CancellationToken.None); } catch { }

        // Assert
        _dasLevyRepository.Verify(
            x => x.GetAccountLevyDeclarationsByDateRange(
                It.IsAny<long>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>()),
            Times.Never);
    }
}