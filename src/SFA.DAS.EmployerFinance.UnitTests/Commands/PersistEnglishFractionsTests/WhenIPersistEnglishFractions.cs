using SFA.DAS.EmployerFinance.Commands.PersistEnglishFractions;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.PersistEnglishFractionsTests
{
    public class WhenIPersistEnglishFractions
    {
        private PersistEnglishFractionsCommandHandler _handler;
        private Mock<IEnglishFractionRepository> _englishFractionRepository;
        private Mock<ILogger<PersistEnglishFractionsCommandHandler>> _logger;
        private Mock<IValidator<PersistEnglishFractionsCommand>> _validator;

        private const string EmployerReference = "123/AB456";

        [SetUp]
        public void Arrange()
        {
            _englishFractionRepository = new Mock<IEnglishFractionRepository>();
            _logger = new Mock<ILogger<PersistEnglishFractionsCommandHandler>>();
            _validator = new Mock<IValidator<PersistEnglishFractionsCommand>>();

            _validator.Setup(x => x.Validate(It.IsAny<PersistEnglishFractionsCommand>()))
                .Returns(new ValidationResult { ValidationDictionary = new Dictionary<string, string>() });

            _handler = new PersistEnglishFractionsCommandHandler(
                _englishFractionRepository.Object,
                _logger.Object,
                _validator.Object);
        }

        [Test]
        public async Task ThenItStoresNewFractions_WhenUpdateIsRequired()
        {
            // Arrange
            var existingFractions = new List<DasEnglishFraction>
            {
                new() { EmpRef = EmployerReference, DateCalculated = new DateTime(2025, 01, 01), Amount = 0.5m }
            };

            var incomingFractions = new List<DasEnglishFraction>
            {
                new() { EmpRef = EmployerReference, DateCalculated = new DateTime(2025, 01, 01), Amount = 0.5m },
                new() { EmpRef = EmployerReference, DateCalculated = new DateTime(2025, 02, 01), Amount = 0.6m }
            };

            _englishFractionRepository.Setup(x => x.GetAllEmployerFractions(EmployerReference))
                .ReturnsAsync(existingFractions);

            var request = new PersistEnglishFractionsCommand
            {
                EmployerReference = EmployerReference,
                UpdateRequired = true,
                DateCalculated = new DateTime(2025, 02, 01),
                Fractions = incomingFractions
            };

            // Act
            var response = await _handler.Handle(request, CancellationToken.None);

            // Assert
            response.Stored.Should().Be(1);
            response.Ignored.Should().Be(1);
            _englishFractionRepository.Verify(x => x.CreateEmployerFraction(
                It.Is<DasEnglishFraction>(f => f.DateCalculated == new DateTime(2025, 02, 01)),
                EmployerReference), Times.Once);
        }

        [Test]
        public async Task ThenItDoesNotStoreFractions_WhenNoUpdateIsRequiredAndFractionsAreOlder()
        {
            // Arrange
            var existingFractions = new List<DasEnglishFraction>
            {
                new() { EmpRef = EmployerReference, DateCalculated = new DateTime(2025, 02, 01), Amount = 0.6m }
            };

            var incomingFractions = new List<DasEnglishFraction>
            {
                new() { EmpRef = EmployerReference, DateCalculated = new DateTime(2025, 01, 01), Amount = 0.5m }
            };

            _englishFractionRepository.Setup(x => x.GetAllEmployerFractions(EmployerReference))
                .ReturnsAsync(existingFractions);

            var request = new PersistEnglishFractionsCommand
            {
                EmployerReference = EmployerReference,
                UpdateRequired = false,
                DateCalculated = new DateTime(2025, 01, 01),
                Fractions = incomingFractions
            };

            // Act
            var response = await _handler.Handle(request, CancellationToken.None);

            // Assert
            response.Stored.Should().Be(0);
            response.Ignored.Should().Be(1);
            _englishFractionRepository.Verify(x => x.CreateEmployerFraction(It.IsAny<DasEnglishFraction>(), It.IsAny<string>()), Times.Never);
        }
    }
}
