using AutoFixture;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Queries.GetAccountProjectionSummary;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetAccountProjectionSummary
{
    public class WhenIGetAccountProjectionSummary
    {
        private Mock<IDasLevyRepository> _repositoryMock;
        private Mock<ILogger<GetAccountProjectionSummaryHandler>> _loggerMock;
        private IFixture _fixture;
        private Mock<ICurrentDateTime> _currentDateTime;
        private GetAccountProjectionSummaryHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _repositoryMock = new Mock<IDasLevyRepository>();
            _loggerMock = new Mock<ILogger<GetAccountProjectionSummaryHandler>>();
            _currentDateTime = new Mock<ICurrentDateTime>();
            _fixture = new Fixture();
            _handler = new GetAccountProjectionSummaryHandler(_repositoryMock.Object, _currentDateTime.Object, _loggerMock.Object);
        }

        [Test]
        public async Task Handle_ShouldReturnCurrentMonthRecord_WhenCurrentMonthRecordExists()
        {
            // Arrange
            var today = new DateTime(2024, 6, 25);
            var payeScheme = "PA12/YE";
            _currentDateTime.Setup(x => x.Now).Returns(today);

            var query = _fixture.Create<GetAccountProjectionSummaryQuery>();
            var declarations = _fixture
                .Build<LevyDeclarationItem>()
                .With(x=> x.EmpRef, payeScheme)
                .CreateMany(5)
                .ToList();

            var currentMonthRecord = new LevyDeclarationItem
            {
                PayrollYear = today.ToPayrollYearString(),
                PayrollMonth = 3,
                TotalAmount = 1000,
                AccountId = query.AccountId,
                EmpRef = payeScheme
            };

            declarations.Add(currentMonthRecord);

            _repositoryMock.Setup(repo => repo.GetAccountLevyDeclarationsForPreviousMonths(query.AccountId, GetAccountProjectionSummaryHandler.PreviousMonths))
                .ReturnsAsync(declarations);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.AccountId.Should().Be(currentMonthRecord.AccountId);
            result.FundsIn.Should().Be(12000);
        }

        [Test]
        public async Task Handle_ShouldReturnPreviousMonthRecord_WhenCurrentMonthRecordNotExistsButPreviousMonthRecordExists()
        {
            // Arrange
            var today = new DateTime(2024, 6, 25);
            var payeScheme = "PA12/YE";
            _currentDateTime.Setup(x => x.Now).Returns(today);

            var query = _fixture.Create<GetAccountProjectionSummaryQuery>();
            var declarations = _fixture
                .Build<LevyDeclarationItem>()
                .With(x=> x.EmpRef, payeScheme)
                .CreateMany(5)
                .ToList();

            var previousMonthRecord = new LevyDeclarationItem
            {
                PayrollYear = today.ToPayrollYearString(),
                PayrollMonth = 2,
                TotalAmount = 800,
                AccountId = query.AccountId,
                EmpRef = payeScheme
            };
            declarations.Add(previousMonthRecord);

            _repositoryMock.Setup(repo => repo.GetAccountLevyDeclarationsForPreviousMonths(query.AccountId, GetAccountProjectionSummaryHandler.PreviousMonths))
                .ReturnsAsync(declarations);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.AccountId.Should().Be(previousMonthRecord.AccountId);
            result.FundsIn.Should().Be(9600);
        }
          
        [Test]
        public async Task Handle_Straddle_PayrollYear_Should_return()
        {
            // Arrange
            var today = new DateTime(2024, 4, 25);
            var payeScheme = "PA12/YE";
            _currentDateTime.Setup(x => x.Now).Returns(today);

            var query = _fixture.Create<GetAccountProjectionSummaryQuery>();
            var declarations = _fixture
                .Build<LevyDeclarationItem>()
                .With(x=> x.EmpRef, payeScheme)
                .CreateMany(5)
                .ToList();

            var previousMonthRecord = new LevyDeclarationItem
            {
                PayrollYear = "23-24",
                PayrollMonth = 12 ,
                TotalAmount = 800,
                AccountId = query.AccountId,
                EmpRef = payeScheme
            };
            declarations.Add(previousMonthRecord);

            _repositoryMock.Setup(repo => repo.GetAccountLevyDeclarationsForPreviousMonths(query.AccountId, GetAccountProjectionSummaryHandler.PreviousMonths))
                .ReturnsAsync(declarations);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.AccountId.Should().Be(previousMonthRecord.AccountId);
            result.FundsIn.Should().Be(9600);
        }

        [Test]
        public async Task Handle_ShouldReturnZero_WhenNoRecordsExist()
        {
            // Arrange
            var query = _fixture.Create<GetAccountProjectionSummaryQuery>();
            var declarations = _fixture.CreateMany<LevyDeclarationItem>(5).ToList();

            _currentDateTime.Setup(x => x.Now).Returns(DateTime.UtcNow);

            _repositoryMock.Setup(repo => repo.GetAccountLevyDeclarationsForPreviousMonths(query.AccountId, GetAccountProjectionSummaryHandler.PreviousMonths))
                .ReturnsAsync(declarations);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.AccountId.Should().Be(query.AccountId);
            result.FundsIn.Should().Be(0);
        }
        
        [Test]
        public async Task Handle_ShouldReturnCurrentMonthRecordForEachPaye_WhenCMultiplePayesExist()
        {
            // Arrange
            var today = new DateTime(2024, 6, 25);
            var payeScheme1 = "PA12/YE";
            var payeScheme2 = "IAM1/ANUVA";
            _currentDateTime.Setup(x => x.Now).Returns(today);

            var query = _fixture.Create<GetAccountProjectionSummaryQuery>();

            var declarations = new List<LevyDeclarationItem>
            {
                new()
                {
                    PayrollYear = today.ToPayrollYearString(),
                    PayrollMonth = 3,
                    TotalAmount = 111,
                    AccountId = query.AccountId,
                    EmpRef = payeScheme1
                },
                new()
                {
                    PayrollYear = today.ToPayrollYearString(),
                    PayrollMonth = 2,
                    TotalAmount = 222,
                    AccountId = query.AccountId,
                    EmpRef = payeScheme1
                },
                new()
                {
                    PayrollYear = today.ToPayrollYearString(),
                    PayrollMonth = 3,
                    TotalAmount = 900,
                    AccountId = query.AccountId,
                    EmpRef = payeScheme2
                },
                new()
                {
                    PayrollYear = today.ToPayrollYearString(),
                    PayrollMonth = 2,
                    TotalAmount = 101,
                    AccountId = query.AccountId,
                    EmpRef = payeScheme2
                },

            };

            _repositoryMock
                .Setup(repo => repo.GetAccountLevyDeclarationsForPreviousMonths(query.AccountId, GetAccountProjectionSummaryHandler.PreviousMonths))
                .ReturnsAsync(declarations);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.AccountId.Should().Be(query.AccountId);
            result.FundsIn.Should().Be(12132);
        }
    }
}
