using System.ComponentModel.DataAnnotations;
using HMRC.ESFA.Levy.Api.Types;
using SFA.DAS.EmployerFinance.Commands.UpdatePayeInformation;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Interfaces.Hmrc;
using SFA.DAS.EmployerFinance.Models.Paye;
using SFA.DAS.EmployerFinance.Validation;
using ValidationResult = SFA.DAS.EmployerFinance.Validation.ValidationResult;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.UpdatePayeInformationTests;

public class WhenUpdatingPayeInformation
{
    private UpdatePayeInformationCommandHandler _handler;
    private Mock<IValidator<UpdatePayeInformationCommand>> _validator;
    private Mock<IPayeRepository> _payeRepository;
    private Mock<IHmrcService> _hmrcService;
    private const string ExpectedEmpRef = "123RFV";
    private const string ExpectedEmpRefName = "Test Scheme Name";
        
    [SetUp]
    public void Arrange()
    {
        _validator = new Mock<IValidator<UpdatePayeInformationCommand>>();
        _validator.Setup(x => x.Validate(It.IsAny<UpdatePayeInformationCommand>())).Returns(new ValidationResult { ValidationDictionary = new Dictionary<string, string>() });

        _payeRepository = new Mock<IPayeRepository>();
        _payeRepository.Setup(x => x.GetPayeSchemeByRef(ExpectedEmpRef)).ReturnsAsync(new Paye { EmpRef = ExpectedEmpRef });

        _hmrcService = new Mock<IHmrcService>();
        _hmrcService.Setup(x => x.GetEmprefInformation(ExpectedEmpRef))
            .ReturnsAsync(new EmpRefLevyInformation
            {
                Employer = new Employer { Name = new Name { EmprefAssociatedName = ExpectedEmpRefName } }
            });

        _handler = new UpdatePayeInformationCommandHandler(_validator.Object, _payeRepository.Object, _hmrcService.Object);
    }

    [Test]
    public void ThenTheCommandIsValidatedAndAnInvalidRequestExceptionIsThrownIfNotValid()
    {
        //Arrange
        _validator.Setup(x => x.Validate(It.IsAny<UpdatePayeInformationCommand>())).Returns(new ValidationResult { ValidationDictionary = new Dictionary<string, string> { { "", "" } } });

        //Act
        Assert.ThrowsAsync<ValidationException>(async () => await _handler.Handle(new UpdatePayeInformationCommand(), CancellationToken.None));

        //Assert
        _validator.Verify(x => x.Validate(It.IsAny<UpdatePayeInformationCommand>()));
    }

    [Test]
    public async Task ThenTheSchemeIsRetrievedFromTheDatabase()
    {
        //Act
        await _handler.Handle(new UpdatePayeInformationCommand { PayeRef = ExpectedEmpRef }, CancellationToken.None);

        //Assert
        _payeRepository.Verify(x => x.GetPayeSchemeByRef(ExpectedEmpRef), Times.Once);
    }

    [Test]
    public async Task ThenIfTheSchemeReturnedHasNoNameAssociatedWithItThenTheHmrcServiceIsCalled()
    {
        //Arrange
        _payeRepository.Setup(x => x.GetPayeSchemeByRef(ExpectedEmpRef)).ReturnsAsync(new Paye {EmpRef = ExpectedEmpRef});

        //Act
        await _handler.Handle(new UpdatePayeInformationCommand { PayeRef = ExpectedEmpRef }, CancellationToken.None);

        //Assert
        _hmrcService.Verify(x => x.GetEmprefInformation(ExpectedEmpRef), Times.Once);
    }

    [Test]
    public async Task ThenIftheScehmeReturnedHasANameThenTheHmrcServiceIsNotCalled()
    {
        //Arrange
        _payeRepository.Setup(x => x.GetPayeSchemeByRef(ExpectedEmpRef)).ReturnsAsync(new Paye { EmpRef = ExpectedEmpRef, Name = "Test" });

        //Act
        await _handler.Handle(new UpdatePayeInformationCommand { PayeRef = ExpectedEmpRef }, CancellationToken.None);

        //Assert
        _hmrcService.Verify(x => x.GetEmprefInformation(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task ThenIfTheSchemeNameIsPopulatedTheRecordIsUpdated()
    {
        //Act
        await _handler.Handle(new UpdatePayeInformationCommand { PayeRef = ExpectedEmpRef }, CancellationToken.None);

        //Assert
        _payeRepository.Verify(x => x.UpdatePayeSchemeName(ExpectedEmpRef, ExpectedEmpRefName), Times.Once);
    }


    [Test]
    public async Task ThenIfTheSchemeNameIsNotPopulatedTheRecordIsNotUpdated()
    {
        //Arrange
        _hmrcService.Setup(x => x.GetEmprefInformation(ExpectedEmpRef))
            .ReturnsAsync(new EmpRefLevyInformation
            {
                Employer = new Employer { Name = new Name() }
            });

        //Act
        await _handler.Handle(new UpdatePayeInformationCommand { PayeRef = ExpectedEmpRef }, CancellationToken.None);

        //Assert
        _payeRepository.Verify(x => x.UpdatePayeSchemeName(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task ThenIfNullIsReturnedFromHmrcThenTheRecordIsNotUpdated()
    {
        //Arrange
        _hmrcService.Setup(x => x.GetEmprefInformation(ExpectedEmpRef)).ReturnsAsync(() => null);

        //Act
        await _handler.Handle(new UpdatePayeInformationCommand { PayeRef = ExpectedEmpRef }, CancellationToken.None);

        //Assert
        _payeRepository.Verify(x => x.UpdatePayeSchemeName(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}