using System.Net;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Queries.FindEmployerAccountLevyDeclarationTransactions;
using SFA.DAS.EmployerFinance.Queries.GetPayeSchemeByRef;
using SFA.DAS.EmployerFinance.Web.Orchestrators;
using SFA.DAS.Encoding;
using SFA.DAS.GovUK.Auth.Employer;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Orchestrators;

internal class WhenFindingAccountLevyDeclarationTransactions
{
    private const string HashedAccountId = "ABC123";
    private readonly DateTime _fromDate = new DateTime(2024, 1, 1);
    private readonly DateTime _toDate = new DateTime(2024, 1, 31);

    private Mock<IMediator> _mediator;
    private EmployerAccountTransactionsOrchestrator _orchestrator;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();

        _mediator
            .Setup(m => m.Send(It.IsAny<GetPayeSchemeByRefQuery>(), CancellationToken.None))
            .ReturnsAsync(new GetPayeSchemeByRefResponse
            {
                PayeScheme = new PayeSchemeView { Name = "Test Scheme" }
            });

        _orchestrator = new EmployerAccountTransactionsOrchestrator(
            Mock.Of<IAccountApiClient>(),
            _mediator.Object,
            Mock.Of<ICurrentDateTime>(),
            Mock.Of<ILogger<EmployerAccountTransactionsOrchestrator>>(),
            Mock.Of<IEncodingService>(),
            Mock.Of<IAuthenticationOrchestrator>(),
            Mock.Of<IGovAuthEmployerAccountService>(),
            Mock.Of<EmployerFinanceWebConfiguration>());
    }

    [Test]
    public async Task ThenItReturnsNotFoundWhenThereAreNoTransactions()
    {
        _mediator
            .Setup(m => m.Send(It.IsAny<FindEmployerAccountLevyDeclarationTransactionsQuery>(), CancellationToken.None))
            .ReturnsAsync(new FindEmployerAccountLevyDeclarationTransactionsResponse
            {
                Transactions = new List<LevyDeclarationTransactionLine>(),
                Total = 0
            });

        var result = await _orchestrator.FindAccountLevyDeclarationTransactions(HashedAccountId, _fromDate, _toDate);

        result.Status.Should().Be(HttpStatusCode.NotFound);
        result.Data.Should().BeNull();
    }

    [Test]
    public async Task ThenItReturnsOkWithTransactions()
    {
        var transaction = new LevyDeclarationTransactionLine
        {
            EmpRef = "123/ABC",
            DateCreated = _fromDate,
            Amount = 500m
        };

        _mediator
            .Setup(m => m.Send(It.Is<FindEmployerAccountLevyDeclarationTransactionsQuery>(q =>
                q.HashedAccountId == HashedAccountId &&
                q.FromDate == _fromDate &&
                q.ToDate == _toDate), CancellationToken.None))
            .ReturnsAsync(new FindEmployerAccountLevyDeclarationTransactionsResponse
            {
                Transactions = [transaction],
                Total = 500m
            });
        _mediator
            .Setup(m => m.Send(It.Is<GetPayeSchemeByRefQuery>(q =>
                q.HashedAccountId == HashedAccountId && q.Ref == "123/ABC"), CancellationToken.None))
            .ReturnsAsync(new GetPayeSchemeByRefResponse
            {
                PayeScheme = new PayeSchemeView { Name = "My Scheme" }
            });

        var result = await _orchestrator.FindAccountLevyDeclarationTransactions(HashedAccountId, _fromDate, _toDate);

        result.Status.Should().Be(HttpStatusCode.OK);
        result.Data.Should().NotBeNull();
        result.Data.HashedAccountId.Should().Be(HashedAccountId);
        result.Data.Amount.Should().Be(500m);
        result.Data.SubTransactions.Should().HaveCount(1);
        result.Data.TransactionDate.Should().Be(_fromDate);
        result.Data.SubTransactions.First().PayeSchemeName.Should().Be("My Scheme");
    }

    [Test]
    public async Task ThenItSetsPayeSchemeNameToEmptyStringWhenPayeSchemeIsNull()
    {
        var transaction = new LevyDeclarationTransactionLine { EmpRef = "456/DEF", DateCreated = _fromDate };

        _mediator
            .Setup(m => m.Send(It.IsAny<FindEmployerAccountLevyDeclarationTransactionsQuery>(), CancellationToken.None))
            .ReturnsAsync(new FindEmployerAccountLevyDeclarationTransactionsResponse
            {
                Transactions = new List<LevyDeclarationTransactionLine> { transaction },
                Total = 0
            });

        _mediator
            .Setup(m => m.Send(It.IsAny<GetPayeSchemeByRefQuery>(), CancellationToken.None))
            .ReturnsAsync((GetPayeSchemeByRefResponse)null);

        var result = await _orchestrator.FindAccountLevyDeclarationTransactions(HashedAccountId, _fromDate, _toDate);

        result.Data.SubTransactions.First().PayeSchemeName.Should().BeEmpty();
    }

    [Test]
    public async Task ThenShouldShowTopUpIsTrueWhenAnyTransactionHasTopUp()
    {
        var transactions = new List<LevyDeclarationTransactionLine>
        {
            new() { EmpRef = "123/ABC", DateCreated = _fromDate, TopUp = 50m },
            new() { EmpRef = "456/DEF", DateCreated = _fromDate, TopUp = 0m }
        };

        _mediator
            .Setup(m => m.Send(It.IsAny<FindEmployerAccountLevyDeclarationTransactionsQuery>(), CancellationToken.None))
            .ReturnsAsync(new FindEmployerAccountLevyDeclarationTransactionsResponse
            {
                Transactions = transactions,
                Total = 0
            });

        var result = await _orchestrator.FindAccountLevyDeclarationTransactions(HashedAccountId, _fromDate, _toDate);

        result.Data.ShouldShowTopUp.Should().BeTrue();
    }

    [Test]
    public async Task ThenShouldShowTopUpIsFalseWhenNoTransactionsHaveTopUp()
    {
        var transactions = new List<LevyDeclarationTransactionLine>
        {
            new() { EmpRef = "123/ABC", DateCreated = _fromDate, TopUp = 0m },
            new() { EmpRef = "456/DEF", DateCreated = _fromDate, TopUp = 0m }
        };

        _mediator
            .Setup(m => m.Send(It.IsAny<FindEmployerAccountLevyDeclarationTransactionsQuery>(), CancellationToken.None))
            .ReturnsAsync(new FindEmployerAccountLevyDeclarationTransactionsResponse
            {
                Transactions = transactions,
                Total = 0
            });

        var result = await _orchestrator.FindAccountLevyDeclarationTransactions(HashedAccountId, _fromDate, _toDate);

        result.Data.ShouldShowTopUp.Should().BeFalse();
    }

    [Test]
    public async Task ThenShouldShowTopUpIsTrueWhenAllTransactionsHaveTopUp()
    {
        var transactions = new List<LevyDeclarationTransactionLine>
        {
            new() { EmpRef = "123/ABC", DateCreated = _fromDate, TopUp = 100m },
            new() { EmpRef = "456/DEF", DateCreated = _fromDate, TopUp = 75m }
        };

        _mediator
            .Setup(m => m.Send(It.IsAny<FindEmployerAccountLevyDeclarationTransactionsQuery>(), CancellationToken.None))
            .ReturnsAsync(new FindEmployerAccountLevyDeclarationTransactionsResponse
            {
                Transactions = transactions,
                Total = 0
            });

        var result = await _orchestrator.FindAccountLevyDeclarationTransactions(HashedAccountId, _fromDate, _toDate);

        result.Data.ShouldShowTopUp.Should().BeTrue();
    }
}
