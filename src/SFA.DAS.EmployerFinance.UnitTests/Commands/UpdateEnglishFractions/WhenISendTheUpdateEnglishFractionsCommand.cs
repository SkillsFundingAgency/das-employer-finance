﻿using System.Globalization;
using HMRC.ESFA.Levy.Api.Types;
using SFA.DAS.EmployerFinance.Commands.UpdateEnglishFractions;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Interfaces.Hmrc;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Queries.GetEnglishFractionsUpdateRequired;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.UpdateEnglishFractions;

class WhenISendTheUpdateEnglishFractionsCommand
{
    private UpdateEnglishFractionsCommandHandler _handler;
    private Mock<IHmrcService> _hmrcService;
    private Mock<IEnglishFractionRepository> _englishFractionRepository;
    private Mock<ILogger<UpdateEnglishFractionsCommandHandler>> _logger;
    private List<DasEnglishFraction> _existingFractions;
    private string _employerReference;
    private List<FractionCalculation> _fractionCalculations;

    [SetUp]
    public void Arrange()
    {
        _employerReference = "123/AB456";
        _englishFractionRepository = new Mock<IEnglishFractionRepository>();
        _hmrcService = new Mock<IHmrcService>();
        _logger = new Mock<ILogger<UpdateEnglishFractionsCommandHandler>>();
        _handler = new UpdateEnglishFractionsCommandHandler(_hmrcService.Object, _englishFractionRepository.Object, _logger.Object);

        _existingFractions = new List<DasEnglishFraction>
        {
            new DasEnglishFraction
            {
                Id = "1",
                DateCalculated = DateTime.Today.AddDays(-20),
                EmpRef = _employerReference,
                Amount = 0.45M
            },
            new DasEnglishFraction
            {
                Id = "2",
                DateCalculated = DateTime.Today.AddDays(-10),
                EmpRef = _employerReference,
                Amount = 0.5M
            }
        };

        _fractionCalculations = new List<FractionCalculation>
        {
            new FractionCalculation
            {
                CalculatedAt = DateTime.Today.AddDays(-20),
                Fractions = new List<Fraction>
                {
                    new Fraction { Region = "England", Value = "0.45" }
                }
            },
            new FractionCalculation
            {
                CalculatedAt = DateTime.Today.AddDays(-10),
                Fractions = new List<Fraction>
                {
                    new Fraction { Region = "England", Value = "0.5" }
                }
            },
            new FractionCalculation
            {
                CalculatedAt = DateTime.Today.AddDays(-5),
                Fractions = new List<Fraction>
                {
                    new Fraction { Region = "England", Value = "0.55" }
                }
            },
            new FractionCalculation
            {
                CalculatedAt = DateTime.Today,
                Fractions = new List<Fraction>
                {
                    new Fraction { Region = "England", Value = "0.6" }
                }
            }
        };
    }

    [Test]
    public async Task ThenIShouldUpdateEnglishFractions()
    {
        //Assign
        _englishFractionRepository.Setup(
                x => x.CreateEmployerFraction(It.IsAny<DasEnglishFraction>(), It.IsAny<string>()))
            .Returns(Task.Run(() => { }));

        _englishFractionRepository.Setup(x => x.GetAllEmployerFractions(_employerReference))
            .ReturnsAsync(_existingFractions);

        _hmrcService.Setup(x => x.GetEnglishFractions(_employerReference, It.IsAny<DateTime?>()))
            .ReturnsAsync(new EnglishFractionDeclarations
            {
                Empref = _employerReference,
                FractionCalculations = _fractionCalculations
            });

        //Act
        await _handler.Handle(new UpdateEnglishFractionsCommand
        {
            EmployerReference = _employerReference,
            EnglishFractionUpdateResponse = new GetEnglishFractionUpdateRequiredResponse { UpdateRequired = true }
        }, CancellationToken.None);

        //Assert
        _englishFractionRepository.Verify(x => x.GetAllEmployerFractions(_employerReference), Times.Once);
        _hmrcService.Verify(x => x.GetEnglishFractions(_employerReference, It.IsAny<DateTime?>()), Times.Once);

        _englishFractionRepository.Verify(x => x.CreateEmployerFraction(
            It.Is<DasEnglishFraction>(fraction => IsSameAsFractionCalculation(fraction, _fractionCalculations[2])),
            _employerReference), Times.Once);

        _englishFractionRepository.Verify(x => x.CreateEmployerFraction(
            It.Is<DasEnglishFraction>(fraction => IsSameAsFractionCalculation(fraction, _fractionCalculations[3])),
            _employerReference), Times.Once);
    }

    [Test]
    public async Task ThenIShouldUpdateFractionsWithValidAmountValues()
    {
        //Assign
        _fractionCalculations[2].Fractions[0].Value = "this is not an amount";

        _englishFractionRepository.Setup(
                x => x.CreateEmployerFraction(It.IsAny<DasEnglishFraction>(), It.IsAny<string>()))
            .Returns(Task.Run(() => { }));

        _englishFractionRepository.Setup(x => x.GetAllEmployerFractions(_employerReference))
            .ReturnsAsync(_existingFractions);

        _hmrcService.Setup(x => x.GetEnglishFractions(_employerReference, It.IsAny<DateTime?>()))
            .ReturnsAsync(new EnglishFractionDeclarations
            {
                Empref = _employerReference,
                FractionCalculations = _fractionCalculations
            });

        //Act
        await _handler.Handle(new UpdateEnglishFractionsCommand
        {
            EmployerReference = _employerReference,
            EnglishFractionUpdateResponse = new GetEnglishFractionUpdateRequiredResponse { UpdateRequired = true }
        }, CancellationToken.None);

        //Assert
        _englishFractionRepository.Verify(x => x.CreateEmployerFraction(
            It.Is<DasEnglishFraction>(fraction => IsSameAsFractionCalculation(fraction, _fractionCalculations[2])),
            _employerReference), Times.Never);

        _englishFractionRepository.Verify(x => x.CreateEmployerFraction(
            It.Is<DasEnglishFraction>(fraction => IsSameAsFractionCalculation(fraction, _fractionCalculations[3])),
            _employerReference), Times.Once);
    }

    [Test]
    public async Task ThenIWontAddDuplicateValuesToTheRepository()
    {
        //Arrange
        _englishFractionRepository.Setup(x => x.GetAllEmployerFractions(_employerReference))
            .ReturnsAsync(_existingFractions);
        _hmrcService.Setup(x => x.GetEnglishFractions(_employerReference, It.IsAny<DateTime?>()))
            .ReturnsAsync(new EnglishFractionDeclarations
            {
                Empref = _employerReference,
                FractionCalculations = new List<FractionCalculation>
                {
                    new FractionCalculation
                    {
                        CalculatedAt = DateTime.Today.AddDays(-20),
                        Fractions = new List<Fraction>
                        {
                            new Fraction { Region = "England", Value = "0.45" }
                        }
                    },
                    new FractionCalculation
                    {
                        CalculatedAt = DateTime.Today.AddDays(-10),
                        Fractions = new List<Fraction>
                        {
                            new Fraction { Region = "England", Value = "0.5" }
                        }
                    }
                }
            });


        //Act
        await _handler.Handle(new UpdateEnglishFractionsCommand
        {
            EmployerReference = _employerReference,
            EnglishFractionUpdateResponse = new GetEnglishFractionUpdateRequiredResponse { UpdateRequired = true }
        }, CancellationToken.None);

        //Assert
        _englishFractionRepository.Verify(x => x.CreateEmployerFraction(It.IsAny<DasEnglishFraction>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task ThenIfThereIsNotANewEnglishFractionCalculationAndTheFractionExistsThenItIsNotRetrievedFromHmrc()
    {
        //Arrange
        _englishFractionRepository.Setup(x => x.GetAllEmployerFractions(_employerReference)).ReturnsAsync(_existingFractions);

        //Act
        await _handler.Handle(new UpdateEnglishFractionsCommand
        {
            EmployerReference = _employerReference,
            EnglishFractionUpdateResponse = new GetEnglishFractionUpdateRequiredResponse { DateCalculated = new DateTime(2016, 01, 01), UpdateRequired = false }
        }, CancellationToken.None);

        //Assert
        _hmrcService.Verify(x => x.GetEnglishFractions(_employerReference), Times.Never);
    }

    [Test]
    public async Task ThenIfThereIsANewEnglishFractionCalculationAndTheFractionExistsThenItIsRetrievedFromHmrc()
    {
        //Arrange
        _hmrcService.Setup(x => x.GetEnglishFractions(_employerReference, It.IsAny<DateTime?>()))
            .ReturnsAsync(new EnglishFractionDeclarations
            {
                Empref = _employerReference,
                FractionCalculations = _fractionCalculations
            });

        //Act
        await _handler.Handle(new UpdateEnglishFractionsCommand
        {
            EmployerReference = _employerReference,
            EnglishFractionUpdateResponse = new GetEnglishFractionUpdateRequiredResponse { DateCalculated = new DateTime(2016, 01, 01), UpdateRequired = true }
        }, CancellationToken.None);

        //Assert
        _hmrcService.Verify(x => x.GetEnglishFractions(_employerReference, It.IsAny<DateTime?>()), Times.Once);
    }

    [Test]
    public async Task ThenIfThereAreExistingFractionsNoFractionUpdateRequiredButAFractionDateInTheFutureTheApiIsCalled()
    {
        //Arrange
        _hmrcService.Setup(x => x.GetEnglishFractions(_employerReference, It.IsAny<DateTime?>()))
            .ReturnsAsync(new EnglishFractionDeclarations
            {
                Empref = _employerReference,
                FractionCalculations = _fractionCalculations
            });

        _englishFractionRepository.Setup(x => x.GetAllEmployerFractions(_employerReference)).ReturnsAsync(new List<DasEnglishFraction>
        {
            new DasEnglishFraction
            {
                DateCalculated = new DateTime(2016, 01, 01),
                EmpRef = _employerReference,
                Amount = 0.45M,
                Id = "10"
            }
        });

        //Act
        await _handler.Handle(new UpdateEnglishFractionsCommand
        {
            EmployerReference = _employerReference,
            EnglishFractionUpdateResponse = new GetEnglishFractionUpdateRequiredResponse { DateCalculated = new DateTime(2016, 01, 02), UpdateRequired = false }
        }, CancellationToken.None);

        //Assert
        _hmrcService.Verify(x => x.GetEnglishFractions(_employerReference, It.IsAny<DateTime?>()), Times.Once);
    }

    [Test]
    public async Task ThenIfThereIsNotANewEnglishFractionCalculationAndTheFractionDoesNotExistThenItIsRetrievedFromHmrc()
    {
        //Arrange
        _hmrcService.Setup(x => x.GetEnglishFractions(_employerReference, It.IsAny<DateTime?>()))
            .ReturnsAsync(new EnglishFractionDeclarations
            {
                Empref = _employerReference,
                FractionCalculations = _fractionCalculations
            });

        //Act
        await _handler.Handle(new UpdateEnglishFractionsCommand
        {
            EmployerReference = _employerReference,
            EnglishFractionUpdateResponse = new GetEnglishFractionUpdateRequiredResponse { DateCalculated = new DateTime(2016, 01, 01), UpdateRequired = false }
        }, CancellationToken.None);

        //Assert
        _hmrcService.Verify(x => x.GetEnglishFractions(_employerReference, It.IsAny<DateTime?>()), Times.Once);
    }

    [Test]
    public async Task ThenIfThereAreFractionsAndTheFlagIsSetToUpdateThenTheDateIsPassedAsAFilterParameter()
    {
        //Arrange
        _hmrcService.Setup(x => x.GetEnglishFractions(_employerReference, It.IsAny<DateTime?>()))
            .ReturnsAsync(new EnglishFractionDeclarations
            {
                Empref = _employerReference,
                FractionCalculations = _fractionCalculations
            });
        _englishFractionRepository.Setup(x => x.GetAllEmployerFractions(_employerReference))
            .ReturnsAsync(_existingFractions);

        //Act
        await _handler.Handle(new UpdateEnglishFractionsCommand
        {
            EmployerReference = _employerReference,
            EnglishFractionUpdateResponse = new GetEnglishFractionUpdateRequiredResponse { DateCalculated = new DateTime(2016, 01, 01), UpdateRequired = true }
        }, CancellationToken.None);

        //Assert
        _hmrcService.Verify(x => x.GetEnglishFractions(_employerReference, DateTime.Today.AddDays(-11)), Times.Once);
    }

    private static bool IsSameAsFractionCalculation(DasEnglishFraction fraction, FractionCalculation fractionCalculation)
    {
        var fractiondate = fraction.DateCalculated;
        var fractionAmountString = fraction.Amount.ToString(CultureInfo.InvariantCulture);

        var fractionCalculationAmount = fractionCalculation.Fractions.First().Value;

        return fractiondate.Equals(fractionCalculation.CalculatedAt) &&
               fractionAmountString.Equals(fractionCalculationAmount);
    }
}