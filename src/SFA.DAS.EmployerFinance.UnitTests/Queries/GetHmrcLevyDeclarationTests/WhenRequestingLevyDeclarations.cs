﻿using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Interfaces.Hmrc;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Queries.GetHMRCLevyDeclaration;
using SFA.DAS.EmployerFinance.Queries.GetLastLevyDeclaration;
using SFA.DAS.EmployerFinance.UnitTests.ObjectMothers;
using SFA.DAS.EmployerFinance.Validation;
using ValidationResult = SFA.DAS.EmployerFinance.Validation.ValidationResult;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetHmrcLevyDeclarationTests;

public class WhenRequestingLevyDeclarations
{
    private const string ExpectedEmpRef = "12345";
    private GetHMRCLevyDeclarationQueryHandler _getHMRCLevyDeclarationQueryHandler;
    private Mock<IValidator<GetHMRCLevyDeclarationQuery>> _validator;
    private Mock<IHmrcService> _hmrcService;
    private Mock<IMediator> _mediator;

    [SetUp]
    public void Arrange()
    {
        _validator = new Mock<IValidator<GetHMRCLevyDeclarationQuery>>();
        _validator.Setup(x => x.Validate(It.IsAny<GetHMRCLevyDeclarationQuery>())).Returns(new ValidationResult { ValidationDictionary = new Dictionary<string, string>() });

        _hmrcService = new Mock<IHmrcService>();
        _hmrcService.Setup(x => x.GetLevyDeclarations(ExpectedEmpRef, It.IsAny<DateTime?>())).ReturnsAsync(DeclarationsObjectMother.Create(ExpectedEmpRef));

        _mediator = new Mock<IMediator>();
        _mediator.Setup(x => x.Send(It.IsAny<GetLastLevyDeclarationQuery>(), CancellationToken.None)).ReturnsAsync(new GetLastLevyDeclarationResponse { Transaction = new DasDeclaration() });

        _getHMRCLevyDeclarationQueryHandler = new GetHMRCLevyDeclarationQueryHandler(_validator.Object, _hmrcService.Object, _mediator.Object);
    }

    [Test]
    public async Task ThenTheValidatorIsCalled()
    {
        //Act
        await _getHMRCLevyDeclarationQueryHandler.Handle(new GetHMRCLevyDeclarationQuery(), CancellationToken.None);

        //Assert
        _validator.Verify(x => x.Validate(It.IsAny<GetHMRCLevyDeclarationQuery>()));
    }

    [Test]
    public void ThenAnInvalidRequestExceptionIsThrownIfTheQueryIsNotValid()
    {
        //Arrange
        _validator.Setup(x => x.Validate(It.IsAny<GetHMRCLevyDeclarationQuery>())).Returns(new ValidationResult { ValidationDictionary = new Dictionary<string, string> { { "", "" } } });

        //Act
        Assert.ThrowsAsync<ValidationException>(async () => await _getHMRCLevyDeclarationQueryHandler.Handle(new GetHMRCLevyDeclarationQuery(), CancellationToken.None));
    }

    [Test]
    public async Task ThenTheLevyServiceIsCalledWithThePassedIdToGetTheLevyDeclarations()
    {
        //Act
        await _getHMRCLevyDeclarationQueryHandler.Handle(new GetHMRCLevyDeclarationQuery { EmpRef = ExpectedEmpRef }, CancellationToken.None);

        //Assert
        _hmrcService.Verify(x => x.GetLevyDeclarations(It.Is<string>(c => c.Equals(ExpectedEmpRef)), It.IsAny<DateTime?>()), Times.Once);
    }
        

    [Test]
    public async Task ThenTheResponseIsPopulatedWithDeclarations()
    {
        //Act
        var actual = await _getHMRCLevyDeclarationQueryHandler.Handle(new GetHMRCLevyDeclarationQuery { EmpRef = ExpectedEmpRef }, CancellationToken.None);

        //Assert
        actual.Should().NotBeNull();
        actual.Empref.Should().Be(ExpectedEmpRef);
        actual.LevyDeclarations.Declarations.Any().Should().BeTrue();
    }

    [Test]
    public async Task ThenIfThereAreAlreadyDeclarationsInTheDatabaseThenTheRequestIsLimitedFromThePreviousMonth()
    {
        //Arrange
        var expectedDate = new DateTime(2017, 01, 20);
        _mediator.Setup(x => x.Send(It.Is<GetLastLevyDeclarationQuery>(c => c.EmpRef.Equals(ExpectedEmpRef)), CancellationToken.None)).ReturnsAsync(new GetLastLevyDeclarationResponse { Transaction = new DasDeclaration { SubmissionDate = expectedDate } });

        //Act
        await _getHMRCLevyDeclarationQueryHandler.Handle(new GetHMRCLevyDeclarationQuery { EmpRef = ExpectedEmpRef }, CancellationToken.None);

        //Assert
        _hmrcService.Verify(x => x.GetLevyDeclarations(It.Is<string>(c => c.Equals(ExpectedEmpRef)), It.Is<DateTime>(c => c.Date.Equals(expectedDate.AddDays(-1)))), Times.Once);
    }
}