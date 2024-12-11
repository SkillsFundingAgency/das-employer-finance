using SFA.DAS.Caches;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.TrainingCourses;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.TrainingCourses;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Models.ApprenticeshipCourse;
using SFA.DAS.EmployerFinance.Services;


namespace SFA.DAS.EmployerFinance.UnitTests.Services.ApprenticeshipInfoServiceWrapperTests;

public class WhenGettingApprenticeshipInfo
{
    private Mock<IOuterApiClient> _apiClient;
    private Mock<IInProcessCache> _cache;
    private ApprenticeshipInfoServiceWrapper _service;

    [SetUp]
    public void Arrange()
    {
        _apiClient = new Mock<IOuterApiClient>();
        _cache = new Mock<IInProcessCache>();

        _service = new ApprenticeshipInfoServiceWrapper(_cache.Object, _apiClient.Object);
    }

    [Test]
    public async Task Then_The_Standards_Are_Retrieved_From_The_Api_And_Added_To_Cached()
    {
        //Arrange
        StandardsView returnData = null;
        var apiResponse = new GetStandardsResponse
        {
            Standards = new List<StandardResponse>
            {
                new StandardResponse
                {
                    Id = 1
                }
            }
        };
        _apiClient.Setup(x => x.Get<GetStandardsResponse>(It.IsAny<GetStandardsRequest>()))
            .ReturnsAsync(apiResponse);
        _cache.Setup(x => x.Exists("Standards")).Returns(false);
        _cache.Setup(x => x.Set("Standards", It.IsAny<StandardsView>())).Callback<string, object>(
            (key, value) => { returnData = value as StandardsView; });

        //Act
        await _service.GetStandardsAsync();

        //Assert
        _apiClient.Verify(x => x.Get<GetStandardsResponse>(It.IsAny<GetStandardsRequest>()), Times.Once);
        returnData.Standards.Count.Should().Be(apiResponse.Standards.Count);
        returnData.Standards.ToList().First().Should().BeEquivalentTo(apiResponse.Standards.First(), options => options
            .Excluding(c => c.Title)
            .Excluding(c => c.Id)
        );
    }

    [Test]
    public async Task Then_Standards_Retrieved_From_Cache_If_Cached()
    {
        //Arrange
        var cacheData = new StandardsView
        {
            Standards =
            [
                new Standard
                {
                    Code = 1
                }
            ]
        };
        _cache.Setup(x => x.Exists("Standards")).Returns(true);
        _cache.Setup(x => x.Get<StandardsView>("Standards")).Returns(cacheData);

        //Act
        var actual = await _service.GetStandardsAsync();

        //Assert
        _apiClient.Verify(x => x.Get<GetStandardsResponse>(It.IsAny<GetStandardsRequest>()), Times.Never);
        actual.Should().BeEquivalentTo(cacheData);
    }

    [Test]
    public async Task Then_The_Frameworks_Are_Retrieved_From_The_Api_And_Added_To_Cached()
    {
        //Arrange
        var apiResponse = new GetFrameworksResponse
        {
            Frameworks =
            [
                new FrameworkResponse
                {
                    Id = "123",
                    FrameworkName = "test",
                    PathwayName = "test",
                    Title = "test"
                }
            ]
        };

        FrameworksView returnData = null;
        _apiClient.Setup(x => x.Get<GetFrameworksResponse>(It.IsAny<GetFrameworksRequest>()))
            .ReturnsAsync(apiResponse);
        _cache.Setup(x => x.Exists("Frameworks")).Returns(false);
        _cache.Setup(x => x.Set("Frameworks", It.IsAny<FrameworksView>())).Callback<string, object>(
            (key, value) => { returnData = value as FrameworksView; });

        //Act
        await _service.GetFrameworksAsync();

        //Assert
        _apiClient.Verify(x => x.Get<GetFrameworksResponse>(It.IsAny<GetFrameworksRequest>()), Times.Once);
        returnData.Frameworks.Count.Should().Be(apiResponse.Frameworks.Count);
        returnData.Frameworks.ToList().First().Should().BeEquivalentTo(apiResponse.Frameworks.First(), options => options
            .Excluding(c => c.Title)
            .Excluding(c => c.ProgType)
        );
    }

    [Test]
    public async Task Then_Frameworks_Retrieved_From_Cache_If_Cached()
    {
        //Arrange
        var cacheData = new FrameworksView
        {
            Frameworks = new List<Framework>
            {
                new Framework
                {
                    Id = "123"
                }
            }
        };
        _cache.Setup(x => x.Exists("Frameworks")).Returns(true);
        _cache.Setup(x => x.Get<FrameworksView>("Frameworks")).Returns(cacheData);

        //Act
        var actual = await _service.GetFrameworksAsync();

        //Assert
        _apiClient.Verify(x => x.Get<GetFrameworksResponse>(It.IsAny<GetFrameworksRequest>()), Times.Never);
        actual.Should().BeEquivalentTo(cacheData);
    }
}