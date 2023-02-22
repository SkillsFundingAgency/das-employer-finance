using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Queries.FindAccountProviderPayments;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Encoding;
namespace SFA.DAS.EmployerFinance.UnitTests.Queries.FindEmployerAccountPaymentTransactionDetailsTests
{
    public class WhenIGetEmployerPaymentTransactionDetails : QueryBaseTest<FindAccountProviderPaymentsHandler, FindAccountProviderPaymentsQuery, FindAccountProviderPaymentsResponse>
    {
        private const string ProviderName = "Test Provider";

        private Mock<IDasLevyService> _dasLevyService;
        private Mock<IEncodingService> _encodingService;
        private DateTime _fromDate;
        private DateTime _toDate;
        private long _accountId;
        private long _ukprn;
        private string _hashedAccountId;
        private string _externalUserId;

        public override FindAccountProviderPaymentsQuery Query { get; set; }
        public override FindAccountProviderPaymentsHandler RequestHandler { get; set; }
        public override Mock<IValidator<FindAccountProviderPaymentsQuery>> RequestValidator { get; set; }
       
        [SetUp]
        public void Arrange()
        {
            SetUp();

            _fromDate = DateTime.Now.AddDays(-10);
            _toDate = DateTime.Now.AddDays(-2);
            _accountId = 1;
            _ukprn = 10;
            _hashedAccountId = "123ABC";
            _externalUserId = "test";

            _encodingService = new Mock<IEncodingService>();
            _encodingService.Setup(x => x.Decode(It.IsAny<string>(), EncodingType.AccountId)).Returns(_accountId);

            _dasLevyService = new Mock<IDasLevyService>();
            _dasLevyService.Setup(x => x.GetAccountProviderPaymentsByDateRange<PaymentTransactionLine>
                                            (It.IsAny<long>(), It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                           .ReturnsAsync(new []
                {
                    new PaymentTransactionLine { ProviderName = ProviderName }
                });

            Query = new FindAccountProviderPaymentsQuery
            {
                HashedAccountId = _hashedAccountId,
                UkPrn = _ukprn,
                FromDate = _fromDate,
                ToDate = _toDate
            };

            RequestHandler = new FindAccountProviderPaymentsHandler(
                RequestValidator.Object, 
                _dasLevyService.Object,
                _encodingService.Object);
        }

        [Test]
        public override async Task ThenIfTheMessageIsValidTheRepositoryIsCalled()
        {
            //Act
            await RequestHandler.Handle(Query, CancellationToken.None);

            //Assert
            _encodingService.Verify(x => x.Decode(_hashedAccountId,EncodingType.AccountId), Times.Once);
            _dasLevyService.Verify(x=>x.GetAccountProviderPaymentsByDateRange<PaymentTransactionLine>
                                            (_accountId, _ukprn, _fromDate, _toDate));
        }

        [Test]
        public override async Task ThenIfTheMessageIsValidTheValueIsReturnedInTheResponse()
        {
            //Act
            var actual = await RequestHandler.Handle(Query, CancellationToken.None);

            //Assert
            Assert.IsNotNull(actual);
            Assert.IsNotEmpty(actual.Transactions);
        }

        [Test]
        public void ThenAnUnauhtorizedExceptionIsThrownIfTheValidationResultReturnsUnauthorized()
        {
            //Arrange
            RequestValidator.Setup(x => x.ValidateAsync(It.IsAny<FindAccountProviderPaymentsQuery>()))
                            .ReturnsAsync(new ValidationResult {IsUnauthorized = true});

            //Act Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await RequestHandler.Handle(new FindAccountProviderPaymentsQuery(), CancellationToken.None));
        }

        [Test]
        public async Task ThenTheProviderNameShouldBeAddedToTheResponse()
        {
            //Act
            var actual = await RequestHandler.Handle(Query, CancellationToken.None);

            //Assert
            Assert.AreEqual(ProviderName, actual.ProviderName);
        }

        [Test]
        public async Task ThenTheTransactionDateShouldBeAddedToTheResponse()
        {
            //Arrange
            var transactionDate = DateTime.Now.AddDays(-2);
            _dasLevyService.Setup(x => x.GetAccountProviderPaymentsByDateRange<PaymentTransactionLine>
                                           (It.IsAny<long>(), It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                           .ReturnsAsync(new []
               {
                    new PaymentTransactionLine {TransactionDate = transactionDate}
               });

            //Act
            var actual = await RequestHandler.Handle(Query, CancellationToken.None);

            //Assert
            Assert.AreEqual(transactionDate, actual.TransactionDate);
        }

        [Test]
        public async Task ThenANotFoundExceptionShouldBeThrowIfNoTransactionsAreFound()
        {
            //Arrange
            _dasLevyService.Setup(x => x.GetAccountProviderPaymentsByDateRange<PaymentTransactionLine>
                    (It.IsAny<long>(), It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new PaymentTransactionLine[0]);

            //Act
            var actual = await RequestHandler.Handle(Query, CancellationToken.None);
            
            //Assert
            Assert.IsNull(actual);
        }
    }
}
