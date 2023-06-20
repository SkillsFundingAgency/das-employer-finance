using AutoFixture.NUnit3;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Accounts;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Accounts;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitationAuthorization;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetTransferConnectionInvitationAuthorizationTests;

public class WhenHandlingGetTransferConnectionInvitationAuthorizationQuery
{
    [Test, MoqAutoData]
    public async Task Then_The_Query_Is_Handled_Repository_Called_And_Agreement_Checked(
        decimal transferAllowance,
        GetTransferConnectionInvitationAuthorizationQuery query,
        GetMinimumSignedAgreementVersionResponse apiResponse,
        TransferAllowance repositoryResponse,
        [Frozen] Mock<ITransferRepository> repository,
        [Frozen] Mock<IOuterApiClient> outerApiClient)
    {
        apiResponse.MinimumSignedAgreementVersion = 5;
        repositoryResponse.RemainingTransferAllowance = 5;
        repository.Setup(x => x.GetTransferAllowance(query.AccountId, transferAllowance))
            .ReturnsAsync(repositoryResponse);
        outerApiClient.Setup(x =>
            x.Get<GetMinimumSignedAgreementVersionResponse>(It.Is<GetMinimumSignedAgreementVersionRequest>(c =>
                c.GetUrl == $"accounts/{query.AccountId}/minimum-signed-agreement-version"))).ReturnsAsync(apiResponse);
        var handler = new GetTransferConnectionInvitationAuthorizationQueryHandler(repository.Object,
            new EmployerFinanceConfiguration{TransferAllowancePercentage = transferAllowance}, outerApiClient.Object);
        
        var actual = await handler.Handle(query, CancellationToken.None);

        actual.AuthorizationResult.Should().BeTrue();
        actual.IsValidSender.Should().BeTrue();
        actual.TransferAllowancePercentage.Should().Be(transferAllowance);
    }
    [Test, MoqAutoData]
    public async Task Then_The_Query_Is_Handled_Repository_Called_And_Agreement_Checked_And_False_Returned_For_Not_Valid(
        decimal transferAllowance,
        GetTransferConnectionInvitationAuthorizationQuery query,
        GetMinimumSignedAgreementVersionResponse apiResponse,
        TransferAllowance repositoryResponse,
        [Frozen] Mock<ITransferRepository> repository,
        [Frozen] Mock<IOuterApiClient> outerApiClient)
    {
        apiResponse.MinimumSignedAgreementVersion = 2;
        repositoryResponse.RemainingTransferAllowance = 0;
        repository.Setup(x => x.GetTransferAllowance(query.AccountId, transferAllowance))
            .ReturnsAsync(repositoryResponse);
        outerApiClient.Setup(x =>
            x.Get<GetMinimumSignedAgreementVersionResponse>(It.Is<GetMinimumSignedAgreementVersionRequest>(c =>
                c.GetUrl == $"accounts/{query.AccountId}/minimum-signed-agreement-version"))).ReturnsAsync(apiResponse);
        var handler = new GetTransferConnectionInvitationAuthorizationQueryHandler(repository.Object,
            new EmployerFinanceConfiguration{TransferAllowancePercentage = transferAllowance}, outerApiClient.Object);
        
        var actual = await handler.Handle(query, CancellationToken.None);

        actual.AuthorizationResult.Should().BeFalse();
        actual.IsValidSender.Should().BeFalse();
        actual.TransferAllowancePercentage.Should().Be(transferAllowance);
    }
}