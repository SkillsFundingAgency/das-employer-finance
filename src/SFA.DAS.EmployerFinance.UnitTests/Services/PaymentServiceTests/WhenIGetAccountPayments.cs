using AutoFixture.NUnit3;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.Provider.Events.Api.Client;
using SFA.DAS.Provider.Events.Api.Types;
using SFA.DAS.Testing.AutoFixture;
using Payment = SFA.DAS.Provider.Events.Api.Types.Payment;

namespace SFA.DAS.EmployerFinance.UnitTests.Services.PaymentServiceTests
{
    internal class WhenIGetAccountPayments
    {
        [Test, MoqAutoData]
        public async Task GetPayments_GetsSinglePage_Payments(
            string periodId,
            long accountId,
            Guid correlationId,
            [Frozen] Mock<IPaymentsEventsApiClient> paymentApiClientMock,
            PaymentService sut)
        {
            // Arrange
            paymentApiClientMock
                .Setup(x => x.GetPayments(periodId, accountId.ToString(), 1, null))
                .ReturnsAsync(new PageOfResults<Payment>
                {
                    PageNumber = 1,
                    TotalNumberOfPages = 1,
                    Items = new List<Payment>
                    {
                        new Payment
                        {
                            Ukprn = 100000050,
                            Amount = 20m,
                            EmployerAccountId = accountId.ToString()
                        }
                    }.ToArray()
                })
                .Verifiable();
            
            // Act
            var result = await sut.GetAccountPayments(periodId, accountId, correlationId);
            
            // Assert
            paymentApiClientMock.Verify();
            paymentApiClientMock.VerifyNoOtherCalls();
        }
    }
}