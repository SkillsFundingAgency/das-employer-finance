using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Validation;
using ValidationResult = SFA.DAS.EmployerFinance.Validation.ValidationResult;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries
{
    public abstract class QueryBaseTest<THandler, TRequest, TResponse>
        where THandler : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public abstract TRequest Query { get; set; }
        public abstract THandler RequestHandler { get; set; }

        public abstract Mock<IValidator<TRequest>> RequestValidator { get; set; }

        private int _validationCallCount;

        protected void SetUp()
        {
            _validationCallCount = 0;
            RequestValidator = new Mock<IValidator<TRequest>>();
            RequestValidator.Setup(x => x.Validate(It.IsAny<TRequest>())).Returns(new ValidationResult()).Callback(() => _validationCallCount++);
            RequestValidator.Setup(x => x.ValidateAsync(It.IsAny<TRequest>())).ReturnsAsync(new ValidationResult()).Callback(() => _validationCallCount++);
        }

        [Test]
        public abstract Task ThenIfTheMessageIsValidTheRepositoryIsCalled();

        public abstract Task ThenIfTheMessageIsValidTheValueIsReturnedInTheResponse();

        [Test]
        public async Task ThenTheReturnValueIsAssignableToTheResponse()
        {
            //Act
            var actual = await RequestHandler.Handle(Query, CancellationToken.None);

            //Assert
            actual.Should().BeAssignableTo<TResponse>();
        }

        [Test]
        public async Task ThenTheValidatorIsCalled()
        {
            //Act
            await RequestHandler.Handle(Query, CancellationToken.None);

            //Assert
            _validationCallCount.Should().Be(1);
        }

        [Test]
        public void ThenAnInvalidRequestExceptionIsThrownIfTheRequestIsNotValid()
        {
            //Arrange
            RequestValidator.Setup(x => x.Validate(It.IsAny<TRequest>())).Returns(new ValidationResult { ValidationDictionary = new Dictionary<string, string> { { "", "" } } });
            RequestValidator.Setup(x => x.ValidateAsync(It.IsAny<TRequest>())).ReturnsAsync(new ValidationResult { ValidationDictionary = new Dictionary<string, string> { { "", "" } } });

            //Act
            Assert.ThrowsAsync<ValidationException>( () =>  RequestHandler.Handle(Query, CancellationToken.None));
        }
    }
}