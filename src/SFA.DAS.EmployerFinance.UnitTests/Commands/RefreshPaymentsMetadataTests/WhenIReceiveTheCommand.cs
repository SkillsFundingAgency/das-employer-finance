using System.Security.Cryptography.X509Certificates;
using AutoFixture.NUnit3;
using Microsoft.EntityFrameworkCore.Internal;
using SFA.DAS.EmployerFinance.Commands.RefreshPaymentMetadata;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.TestCommon.DatabaseMock;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.RefreshPaymentsMetadataTests;

public class WhenIReceiveTheCommand
{
    // [Test, MoqAutoData]
    // public async Task ThenTheCommandIsValidated(
    //     [Frozen]Mock<IValidator<RefreshPaymentMetadataCommand>> validator,
    //     RefreshPaymentMetadataCommand command,
    //     RefreshPaymentMetadataCommandHandler handler,
    //     [Frozen]Mock<EmployerFinanceDbContext> dbContext,
    //     IList<Payment> payments)
    // {
    //     validator.Setup(x =>
    //         x.Validate(It.Is<RefreshPaymentMetadataCommand>(c => 
    //             c.PaymentId == command.PaymentId 
    //             && c.AccountId == command.AccountId
    //             && c.PeriodEndRef == command.PeriodEndRef)))
    //         .Returns(new ValidationResult { ValidationDictionary = new Dictionary<string, string>() })
    //         .Verifiable();
    //
    //     dbContext.Setup(x => x.Payments).ReturnsDbSet(payments);
    //
    //     await handler.Handle(command, CancellationToken.None);
    //
    //     validator.Verify();
    // }
}