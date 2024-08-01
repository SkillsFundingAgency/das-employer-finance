using AutoFixture.NUnit3;
using SFA.DAS.Caches;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.ApprenticeshipCourse;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerFinance.UnitTests.Services.PaymentServiceTests;

public class WhenIAddSinglePaymentDetailsMetadata
{
    [Test, MoqAutoData]
    public async Task ThenIGetProvider(
        long accountId,
        EmployerFinance.Models.ApprenticeshipProvider.Provider provider,
        [Frozen] Mock<IProviderService> providerService,
        PaymentDetails paymentDetails,
        PaymentService service
    )
    {
        providerService.Setup(x => x.Get(paymentDetails.Ukprn))
            .ReturnsAsync(provider)
            .Verifiable(Times.Once);

        var actual = await service.AddSinglePaymentDetailsMetadata(accountId, paymentDetails);

        providerService.Verify();
        actual.ProviderName.Should().Be(provider.Name);
        actual.IsHistoricProviderName.Should().Be(provider.IsHistoricProviderName);
    }

    [Test, MoqAutoData]
    public async Task ThenIGetApprenticeship(
        long accountId,
        GetApprenticeshipResponse apprenticeshipResponse,
        [Frozen] Mock<ICommitmentsV2ApiClient> commitmentsClient,
        PaymentDetails paymentDetails,
        PaymentService service
    )
    {
        commitmentsClient.Setup(x => x.GetApprenticeship(paymentDetails.ApprenticeshipId))
            .ReturnsAsync(apprenticeshipResponse)
            .Verifiable(Times.Once);

        await service.AddSinglePaymentDetailsMetadata(accountId, paymentDetails);

        commitmentsClient.Verify();
    }

    [Test, MoqAutoData]
    public async Task WhenApprenticeshipIsNotNullThenApprenticeDetailsArePopulated(
        long accountId,
        GetApprenticeshipResponse apprenticeshipResponse,
        [Frozen] Mock<ICommitmentsV2ApiClient> commitmentsClient,
        PaymentDetails paymentDetails,
        PaymentService service
    )
    {
        commitmentsClient.Setup(x => x.GetApprenticeship(paymentDetails.ApprenticeshipId))
            .ReturnsAsync(apprenticeshipResponse);

        var actual = await service.AddSinglePaymentDetailsMetadata(accountId, paymentDetails);

        actual.ApprenticeName.Should().Be($"{apprenticeshipResponse.FirstName} {apprenticeshipResponse.LastName}");
        actual.CourseStartDate.Should().Be(apprenticeshipResponse.StartDate);
    }

    [Test, MoqAutoData]
    public async Task WhenStandardCodeIsPopulatedThenStandardIsRetrieved(
        long accountId,
        GetApprenticeshipResponse apprenticeshipResponse,
        Standard standard,
        EmployerFinance.Models.ApprenticeshipProvider.Provider provider,
        [Frozen] Mock<IInProcessCache> inprocessCache,
        [Frozen] Mock<IProviderService> providerService,
        [Frozen] Mock<ICommitmentsV2ApiClient> commitmentsClient,
        [Frozen] Mock<IApprenticeshipInfoServiceWrapper> apprenticeshipInfoService,
        PaymentDetails paymentDetails,
        PaymentService service
    )
    {
        paymentDetails.StandardCode = 100;
        standard.Code = paymentDetails.StandardCode.Value;

        inprocessCache.Setup(x => x.Get<StandardsView>(nameof(StandardsView))).Returns(() => null);
        apprenticeshipInfoService.Setup(x => x.GetStandardsAsync(It.IsAny<bool>())).ReturnsAsync(new StandardsView
        {
            Standards = [standard]
        }).Verifiable(Times.Once);

        var actual = await service.AddSinglePaymentDetailsMetadata(accountId, paymentDetails);

        actual.CourseName.Should().Be(standard.CourseName);
        actual.CourseLevel.Should().Be(standard.Level);

        apprenticeshipInfoService.Verify();
    }
    
    [Test, MoqAutoData]
    public async Task WhenStandardCodeIsNotPopulatedAndFrameworkCodeHasValueThenFrameworkIsRetrieved(
        long accountId,
        GetApprenticeshipResponse apprenticeshipResponse,
        FrameworksView frameworksView,
        EmployerFinance.Models.ApprenticeshipProvider.Provider provider,
        [Frozen] Mock<IInProcessCache> inprocessCache,
        [Frozen] Mock<IProviderService> providerService,
        [Frozen] Mock<ICommitmentsV2ApiClient> commitmentsClient,
        [Frozen] Mock<IApprenticeshipInfoServiceWrapper> apprenticeshipInfoService,
        PaymentDetails paymentDetails,
        PaymentService service
    )
    {
        paymentDetails.StandardCode = null;
        frameworksView.Frameworks.Clear();
        var framework = new Framework
        {
            FrameworkCode = paymentDetails.FrameworkCode.Value,
            ProgrammeType = paymentDetails.ProgrammeType.Value,
            PathwayCode = paymentDetails.PathwayCode.Value
        };
        frameworksView.Frameworks.Add(framework);
        
        inprocessCache.Setup(x => x.Get<FrameworksView>(nameof(FrameworksView))).Returns(() => null);
        
        apprenticeshipInfoService.Setup(x => 
            x.GetFrameworksAsync(It.IsAny<bool>()))
            .ReturnsAsync(frameworksView)
            .Verifiable(Times.Once);

        var actual = await service.AddSinglePaymentDetailsMetadata(accountId, paymentDetails);

        actual.CourseName.Should().Be(framework.FrameworkName);
        actual.CourseLevel.Should().Be(framework.Level);
        actual.PathwayName.Should().Be(framework.PathwayName);

        apprenticeshipInfoService.Verify();
    }
}