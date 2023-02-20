﻿using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Commands.CreateEnglishFractionCalculationDate;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Validation;
using ValidationResult = SFA.DAS.EmployerFinance.Validation.ValidationResult;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.CreateEnglishFractionCalculationDateCommandTests;

public class WhenIHandleTheCommand
{
    private CreateEnglishFractionCalculationDateCommandHandler _handler;
    private Mock<IValidator<CreateEnglishFractionCalculationDateCommand>> _validator;
    private Mock<IEnglishFractionRepository> _englishFractionRepository;
    private Mock<ILogger<CreateEnglishFractionCalculationDateCommandHandler>> _logger;
    private DateTime _expectedDate;

    [SetUp]
    public void Arrange()
    {
        _expectedDate = new DateTime(2016, 10, 30);

        _validator = new Mock<IValidator<CreateEnglishFractionCalculationDateCommand>>();
        _validator.Setup(x => x.Validate(It.IsAny<CreateEnglishFractionCalculationDateCommand>())).Returns(new ValidationResult {ValidationDictionary = new Dictionary<string, string>()});

        _englishFractionRepository = new Mock<IEnglishFractionRepository>();

        _logger = new Mock<ILogger<CreateEnglishFractionCalculationDateCommandHandler>>();

        _handler = new CreateEnglishFractionCalculationDateCommandHandler(_validator.Object, _englishFractionRepository.Object, _logger.Object);
    }

    [Test]
    public void ThenTheValidatorIsCalledAndAInvalidRequestExceptionIsThrownIfNotValid()
    {
        //Arrange
        _validator.Setup(x => x.Validate(It.IsAny<CreateEnglishFractionCalculationDateCommand>())).Returns(new ValidationResult {ValidationDictionary = new Dictionary<string, string> {{"", ""}}});

        //Act
        Assert.ThrowsAsync<ValidationException>(async ()=> await _handler.Handle(new CreateEnglishFractionCalculationDateCommand(), CancellationToken.None));

        //Assert
        _validator.Verify(x=>x.Validate(It.IsAny<CreateEnglishFractionCalculationDateCommand>()),Times.Once);
    }

    [Test]
    public async Task ThenTheRepositoryIsUpdatedWhenTheCommandIsValid()
    {
        //Act
        await _handler.Handle(new CreateEnglishFractionCalculationDateCommand {DateCalculated = _expectedDate}, CancellationToken.None);

        //Assert
        _englishFractionRepository.Verify(x=>x.SetLastUpdateDate(_expectedDate));
    }

    [Test]
    public async Task ThenAnInfoLevelMessageIsLoggedWhenItHasBeenUpdated()
    {
        //Act
        await _handler.Handle(new CreateEnglishFractionCalculationDateCommand { DateCalculated = _expectedDate }, CancellationToken.None);

        //Assert
        _logger.Verify(x => x.LogInformation($"English Fraction CalculationDate updated to {_expectedDate.ToString("dd MMM yyyy")}"), Times.Once());
    }
}