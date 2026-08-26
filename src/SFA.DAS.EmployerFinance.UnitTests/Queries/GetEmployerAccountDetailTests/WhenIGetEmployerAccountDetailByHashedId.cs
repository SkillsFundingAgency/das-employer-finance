using AutoMapper;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountDetail;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetEmployerAccountDetailTests;

public class WhenIGetEmployerAccountDetailByHashedId : QueryBaseTest<GetEmployerAccountDetailByHashedIdQueryHandler, GetEmployerAccountDetailByHashedIdQuery, GetEmployerAccountDetailByHashedIdResponse>
{
    private Mock<IAccountApiClient> _accountApiClient;
    private Mock<IMapper> _mapper;
    private const string ExpectedHashedId = "VW6B97";
    private AccountDetailViewModel _accountDetail;
    private AccountDetailDto _accountDetailDto;

    public override GetEmployerAccountDetailByHashedIdQuery Query { get; set; }
    public override GetEmployerAccountDetailByHashedIdQueryHandler RequestHandler { get; set; }
    public override Mock<IValidator<GetEmployerAccountDetailByHashedIdQuery>> RequestValidator { get; set; }

    [SetUp]
    public void Arrange()
    {
        base.SetUp();

        _accountDetail = new AccountDetailViewModel();
        _accountDetailDto = new AccountDetailDto { HashedId = ExpectedHashedId };

        _accountApiClient = new Mock<IAccountApiClient>();
        _accountApiClient.Setup(x => x.GetAccount(ExpectedHashedId)).ReturnsAsync(_accountDetail);

        _mapper = new Mock<IMapper>();
        _mapper.Setup(x => x.Map<AccountDetailDto>(_accountDetail)).Returns(_accountDetailDto);

        RequestHandler = new GetEmployerAccountDetailByHashedIdQueryHandler(
            RequestValidator.Object,
            _accountApiClient.Object,
            _mapper.Object);
        Query = new GetEmployerAccountDetailByHashedIdQuery { HashedAccountId = ExpectedHashedId };
    }

    [Test]
    public override async Task ThenIfTheMessageIsValidTheRepositoryIsCalled()
    {
        await RequestHandler.Handle(Query, CancellationToken.None);

        _accountApiClient.Verify(x => x.GetAccount(ExpectedHashedId), Times.Once);
    }

    [Test]
    public override async Task ThenIfTheMessageIsValidTheValueIsReturnedInTheResponse()
    {
        var result = await RequestHandler.Handle(Query, CancellationToken.None);

        result.Should().NotBeNull();
        result.AccountDetail.Should().Be(_accountDetailDto);
    }
}
