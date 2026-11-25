using SFA.DAS.EmployerFinance.Queries.GetContent;
using SFA.DAS.EmployerFinance.Web.Helpers;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Helpers;

[TestFixture]
public class HtmlHelpersTests
{
    [Test]
    public async Task WhenICallGetContentByTypeAsync_ThenTheContentIsReturnedAsHtmlString()
    {
        // Arrange
        const string contentType = "test-content-type";
        const string content = "<p>Some content</p>";
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.Is<GetContentQuery>(q => q.ContentType == contentType), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetContentResponse { Content = content });
        var sut = new HtmlHelpers(mediator.Object);

        // Act
        var actual = await sut.GetContentByTypeAsync(contentType);

        // Assert
        actual.Should().NotBeNull();
        actual.Value.Should().Be(content);
        mediator.Verify(m => m.Send(It.Is<GetContentQuery>(q => q.ContentType == contentType), It.IsAny<CancellationToken>()), Times.Once);
    }
}
