using System.Net;
using System.Net.Http;
using Moq.Protected;
using Newtonsoft.Json;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Infrastructure;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;

namespace SFA.DAS.EmployerFinance.UnitTests.Infrastructure
{
    public class WhenHandlingAGetRequest
    {
        [Test]
        public async Task Then_The_Endpoint_Is_Called_With_Authentication_Header_And_Data_Returned()
        {
            //Arrange
            var key = "123-abc-567";
            var getTestRequest = new GetTestRequest();
            var testObject = new List<string>();
            var config = new EmployerFinanceOuterApiConfiguration {BaseUrl = "http://valid-url/", Key = key};

            var response = new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(testObject)),
                StatusCode = HttpStatusCode.Accepted
            };
            var httpMessageHandler = MessageHandler.SetupMessageHandlerMock(response, $"{config.BaseUrl}{getTestRequest.GetUrl}", config.Key);
            var client = new HttpClient(httpMessageHandler.Object);
            var apiClient = new OuterApiClient(client, config);

            //Act
            var actual = await apiClient.Get<List<string>>(getTestRequest);
            
            //Assert
            actual.Should().BeEquivalentTo(testObject);
        }
        
        [Test]
        public void Then_If_It_Is_Not_Successful_An_Exception_Is_Thrown()
        {
            //Arrange
            var key = "123-abc-567";
            var getTestRequest = new GetTestRequest();
            var config = new EmployerFinanceOuterApiConfiguration {BaseUrl = "http://valid-url/", Key = key };
            var response = new HttpResponseMessage
            {
                Content = new StringContent(""),
                StatusCode = HttpStatusCode.BadRequest
            };
            
            var httpMessageHandler = MessageHandler.SetupMessageHandlerMock(response, $"{config.BaseUrl}{getTestRequest.GetUrl}", config.Key);
            var client = new HttpClient(httpMessageHandler.Object);
            var apiClient = new OuterApiClient(client, config);
            
            //Act Assert
            Assert.ThrowsAsync<HttpRequestException>(() => apiClient.Get<List<string>>(getTestRequest));
            
        }

        public class GetTestRequest : IGetApiRequest
        {
            public string GetUrl => "test-url/get";
        }
    }
    public static class MessageHandler
    {
        public static Mock<HttpMessageHandler> SetupMessageHandlerMock(HttpResponseMessage response, string url, string key)
        {
            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(c =>
                        c.Method.Equals(HttpMethod.Get)
                        && c.Headers.Contains("Ocp-Apim-Subscription-Key")
                        && c.Headers.GetValues("Ocp-Apim-Subscription-Key").Single().Equals(key)
                        && c.Headers.Contains("X-Version")
                        && c.Headers.GetValues("X-Version").Single().Equals("1")
                        && c.RequestUri.AbsoluteUri.Equals(url)),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync((HttpRequestMessage request, CancellationToken token) => response);
            return httpMessageHandler;
        }
    }
}