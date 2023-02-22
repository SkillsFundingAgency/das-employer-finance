using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Queries.FindEmployerAccountLevyDeclarationTransactions;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.FindEmployerAccountLevyDeclarationTransactionDetailsTests
{
    public class WhenIGetEmployerLevyDeclarationTransactionDetails : QueryBaseTest<FindEmployerAccountLevyDeclarationTransactionsHandler, FindEmployerAccountLevyDeclarationTransactionsQuery, FindEmployerAccountLevyDeclarationTransactionsResponse>
    {
        private Mock<IDasLevyService> _dasLevyService;
        private Mock<IEncodingService> _encodingService;
        private DateTime _fromDate;
        private DateTime _toDate;
        private long _accountId;
        private string _hashedAccountId;
        private string _externalUserId;
        public override FindEmployerAccountLevyDeclarationTransactionsQuery Query { get; set; }
        public override FindEmployerAccountLevyDeclarationTransactionsHandler RequestHandler { get; set; }
        public override Mock<IValidator<FindEmployerAccountLevyDeclarationTransactionsQuery>> RequestValidator { get; set; }
       
        [SetUp]
        public void Arrange()
        {
            SetUp();

            _fromDate = DateTime.Now.AddDays(-10);
            _toDate = DateTime.Now.AddDays(-2);
            _accountId = 1;
            _hashedAccountId = "123ABC";
            _externalUserId = "test";

            _encodingService = new Mock<IEncodingService>();
            _encodingService.Setup(x => x.Decode(It.IsAny<string>(), EncodingType.AccountId)).Returns(_accountId);

            _dasLevyService = new Mock<IDasLevyService>();
            _dasLevyService.Setup(x => x.GetAccountLevyTransactionsByDateRange<LevyDeclarationTransactionLine>
                                            (It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                           .ReturnsAsync(new []
                {
                    new LevyDeclarationTransactionLine()
                });

            Query = new FindEmployerAccountLevyDeclarationTransactionsQuery
            {
                HashedAccountId = _hashedAccountId,
                FromDate = _fromDate,
                ToDate = _toDate
            };

            RequestHandler = new FindEmployerAccountLevyDeclarationTransactionsHandler(
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
            _dasLevyService.Verify(x=>x.GetAccountLevyTransactionsByDateRange<LevyDeclarationTransactionLine>
                                            (_accountId, _fromDate, _toDate));
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
        public async Task ThenTheLineItemTotalIsCalculatedFromTheAmountTopupAndPercentageOfFraction()
        {
            //Arrange
            _dasLevyService.Setup(x => x.GetAccountLevyTransactionsByDateRange<LevyDeclarationTransactionLine>
                                            (It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                           .ReturnsAsync(new []
            {
                new LevyDeclarationTransactionLine {LineAmount=10,EnglishFraction = 0.5m,TransactionType = TransactionItemType.Declaration},
                new LevyDeclarationTransactionLine {LineAmount=1,EnglishFraction = 0.5m,TransactionType = TransactionItemType.TopUp}
            });

            //Act
            var actual = await RequestHandler.Handle(Query, CancellationToken.None);

            //Assert
            Assert.AreEqual(11,actual.Total);
        }
    }
}
