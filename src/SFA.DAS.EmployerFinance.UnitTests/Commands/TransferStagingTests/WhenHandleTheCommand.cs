using SFA.DAS.EmployerFinance.Commands.StagingTransfers;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.EmployerFinance.Validation;
using System.ComponentModel.DataAnnotations;
using ValidationResult = SFA.DAS.EmployerFinance.Validation.ValidationResult;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.StagingTransfersTests
{
    public class WhenIStageTransfers
    {
        private StageTransfersCommandHandler _handler;
        private Mock<IValidator<StageTransfersCommand>> _validator;
        private Mock<ITransferStagingRepository> _repository;

        [SetUp]
        public void Arrange()
        {
            _repository = new Mock<ITransferStagingRepository>();

            _validator = new Mock<IValidator<StageTransfersCommand>>();
            _validator
                .Setup(x => x.Validate(It.IsAny<StageTransfersCommand>()))
                .Returns(new ValidationResult
                {
                    ValidationDictionary = new Dictionary<string, string>()
                });

            _handler = new StageTransfersCommandHandler(
                _validator.Object,
                _repository.Object);
        }

        [Test]
        public async Task ThenTheCommandIsValidated()
        {
            // Arrange
            var command = new StageTransfersCommand
            {
                Transfers = new List<TransferStaging>()
            };

            _repository
                .Setup(x => x.GetExistingTransferIds(It.IsAny<List<long>>()))
                .ReturnsAsync(new List<long>());

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _validator.Verify(
                x => x.Validate(It.IsAny<StageTransfersCommand>()),
                Times.Once);
        }


        [Test]
        public void ThenAnInvalidRequestExceptionIsThrownWhenTheMessageIsNotValid()
        {
            // Arrange
            _validator
                .Setup(x => x.Validate(It.IsAny<StageTransfersCommand>()))
                .Returns(new ValidationResult
                {
                    ValidationDictionary = new Dictionary<string, string>
                    {
                        { "", "" }
                    }
                });

            // Assert
            Assert.ThrowsAsync<ValidationException>(async () =>
                await _handler.Handle(new StageTransfersCommand(), CancellationToken.None));
        }

        [Test]
        public async Task ThenConflictingTransfersAreReturnedWhenTransferIdsAlreadyExist()
        {
            // Arrange
            var command = new StageTransfersCommand
            {
                Transfers = new List<TransferStaging>
                {
                    new() { TransferId = 1 },
                    new() { TransferId = 2 }
                }
            };

            _repository
                .Setup(x => x.GetExistingTransferIds(It.IsAny<List<long>>()))
                .ReturnsAsync(new List<long> { 2 });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ConflictingTransferIds.Should().ContainSingle()
                .Which.Should().Be(2);

            _repository.Verify(
                x => x.BulkInsertTransfers(It.IsAny<List<TransferStaging>>()),
                Times.Never);
        }

        [Test]
        public async Task ThenTransfersAreInsertedWhenNoConflictsExist()
        {
            // Arrange
            var command = new StageTransfersCommand
            {
                Transfers = new List<TransferStaging>
                {
                    new() { TransferId = 1 },
                    new() { TransferId = 2 }
                }
            };

            _repository
                .Setup(x => x.GetExistingTransferIds(It.IsAny<List<long>>()))
                .ReturnsAsync(new List<long>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _repository.Verify(
                x => x.BulkInsertTransfers(It.IsAny<List<TransferStaging>>()),
                Times.Once);

            result.IsSuccess.Should().BeTrue();
            result.InsertedTransferIds.Should().BeEquivalentTo(new List<long> { 1, 2 });
        }
    }
}
