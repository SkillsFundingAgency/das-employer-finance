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
            _currentDateTime.Setup(x => x.Now).Returns(today);

            var query = _fixture.Create<GetAccountProjectionSummaryQuery>();
            var declarations = _fixture.CreateMany<LevyDeclarationItem>(5).ToList();

            var currentMonthRecord = new LevyDeclarationItem
            {
                PayrollYear = today.ToPayrollYearString(),
                PayrollMonth = 2,
                TotalAmount = 1000,
                AccountId = query.AccountId
            };

            declarations.Add(currentMonthRecord);

            _repositoryMock.Setup(repo => repo.GetAccountLevyDeclarations(query.AccountId))
                .ReturnsAsync(declarations);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.AccountId.Should().Be(currentMonthRecord.AccountId);
            result.FundsIn.Should().Be(currentMonthRecord.TotalAmount);
        }

        [Test]
        public async Task Handle_ShouldReturnPreviousMonthRecord_WhenCurrentMonthRecordNotExistsButPreviousMonthRecordExists()
        {
            // Arrange
            var today = new DateTime(2024, 6, 25);
            _currentDateTime.Setup(x => x.Now).Returns(today);

            var query = _fixture.Create<GetAccountProjectionSummaryQuery>();
            var declarations = _fixture.CreateMany<LevyDeclarationItem>(5).ToList();

            var previousMonthRecord = new LevyDeclarationItem
            {
                PayrollYear = today.ToPayrollYearString(),
                PayrollMonth = 1,
                TotalAmount = 800,
                AccountId = query.AccountId
            };
            declarations.Add(previousMonthRecord);

            _repositoryMock.Setup(repo => repo.GetAccountLevyDeclarations(query.AccountId))
                .ReturnsAsync(declarations);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.AccountId.Should().Be(previousMonthRecord.AccountId);
            result.FundsIn.Should().Be(previousMonthRecord.TotalAmount);
        }

        [Test]
        public async Task Handle_ShouldReturnZero_WhenNoRecordsExist()
        {
            // Arrange
            var query = _fixture.Create<GetAccountProjectionSummaryQuery>();
            var declarations = _fixture.CreateMany<LevyDeclarationItem>(5).ToList();

            _currentDateTime.Setup(x => x.Now).Returns(DateTime.UtcNow);

            _repositoryMock.Setup(repo => repo.GetAccountLevyDeclarations(query.AccountId))
                .ReturnsAsync(declarations);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.AccountId.Should().Be(query.AccountId);
            result.FundsIn.Should().Be(0);
        }
    }
}
