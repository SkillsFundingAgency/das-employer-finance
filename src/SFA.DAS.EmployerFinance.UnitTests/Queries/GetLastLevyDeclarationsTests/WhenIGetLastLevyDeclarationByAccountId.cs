using AutoFixture.NUnit4;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Queries.GetLastLevyDeclarationByAccountId;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetLastLevyDeclarationsTests;

[TestFixture]
public class WhenIGetLastLevyDeclarationByAccountId
{
    [Test, MoqAutoData]
    public async Task ThenIfTheMessageIsValidTheRepositoryIsCalled(
        long accountId,
        [Frozen] Mock<IDasLevyRepository> dasLevyRepository,
        [Greedy] GetLastLevyDeclarationByAccountIdQueryHandler requestHandler)
    {
        //Act
        await requestHandler.Handle(new GetLastLevyDeclarationByAccountIdQuery(accountId), CancellationToken.None);

        //Assert
        dasLevyRepository.Verify(x => x.GetLastPositiveNetDeclarationForAccount(accountId: accountId));
    }

    [Test, MoqAutoData]
    public async Task ThenIfTheMessageIsValidTheValueIsReturnedInTheResponse(long accountId,
        [Frozen] Mock<IDasLevyRepository> dasLevyRepository,
        [Greedy] GetLastLevyDeclarationByAccountIdQueryHandler requestHandler
        )
    {
        //Arrange
        var expectedDate = new DateTime(2016, 01, 29);
        dasLevyRepository.Setup(x => x.GetLastPositiveNetDeclarationForAccount(accountId: accountId)).ReturnsAsync(new DasDeclaration { SubmissionDate = expectedDate });

        //Act
        var actual = await requestHandler.Handle(new GetLastLevyDeclarationByAccountIdQuery(accountId), CancellationToken.None);

        //Assert
        actual.Transaction.Should().NotBeNull();
        actual.Transaction.SubmissionDate.Should().Be(expectedDate);
    }
}