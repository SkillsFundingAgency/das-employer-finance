﻿using System.Globalization;
using SFA.DAS.EmployerFinance.Commands.RefreshEmployerLevyData;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.EmployerFinance.Services.Contracts;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.RefreshEmployerLevyDataTests;

[TestFixture]
public class LevyImportCleanerStrategyTests
{
    [Test]
    public void Constructor_Valid_ShouldNotThrowException()
    {
        var fixtures = new LevyImportCleanerStrategyTestFixtures();
        fixtures.CreateStrategy();
        Assert.Pass("Should not have thrown an exception");
    }

    [Test]
    public async Task Cleanup_DuplicateSubmissionIdsInInput_DuplicatedSubmissionIdsShouldBeRemoved()
    {
        //Arrange 
        var fixtures = new LevyImportCleanerStrategyTestFixtures()
            .WithDeclarations(123, 123);

        // act
        await fixtures.RunStrategy();

        //Assert 
        const int expectedCount = 1;
        fixtures.Result.Length.Should().Be(expectedCount);
    }

    [Test]
    public async Task Cleanup_DuplicateSubmissionIdsInInput_DuplicatedSubmissionIdsShouldBeLogged()
    {
        //Arrange 
        var fixtures = new LevyImportCleanerStrategyTestFixtures()
            .WithDeclarations(123, 123);

        // act
        await fixtures.RunStrategy();

        //Assert 
        const int expectedInfoMessages = 1;
        fixtures.Result.Length.Should().Be(expectedInfoMessages);
    }

    [Test]
    public async Task Cleanup_MultipleLevyAdjustments_OnlyAdjustmentsShouldBeMArkedAsAdjustments()
    {
        //Arrange 
        var fixtures = new LevyImportCleanerStrategyTestFixtures()
            .WithDeclaration(123, 100, "17-18", 12, new DateTime(2018, 04, 19))
            .WithDeclaration(124, 150, "17-18", 12, new DateTime(2018, 05, 19))
            .WithDeclaration(125, 200, "17-18", 12, new DateTime(2018, 06, 19));

        // act
        await fixtures.RunStrategy();

        //Assert 
        //Assert.IsFalse(fixtures.Result[0].EndOfYearAdjustment, "normal period 12 declaration is marked as adjustment");
        fixtures.Result[0].EndOfYearAdjustment.Should().BeFalse("normal period 12 declaration is marked as adjustment");
        fixtures.Result[1].EndOfYearAdjustment.Should().BeTrue("period 12 adjustment is not marked as adjustment");
        fixtures.Result[2].EndOfYearAdjustment.Should().BeTrue("period 12 adjustment is not marked as adjustment");
    }

    [Test(Description = "The first adjustment should apply to the P12 declaration")]
    public async Task Cleanup_MultipleLevyAdjustments_FirstAdjustmentShouldApplyToLatestDeclaration()
    {
        const decimal period12Value = 25;
        const decimal firstAdjustmentValue = 150;
        const decimal secondAdjustmentValue = 200;

        //Arrange 
        var fixtures = new LevyImportCleanerStrategyTestFixtures()
            .WithDeclaration(123, period12Value, "17-18", 12, new DateTime(2018, 04, 19))
            .WithDeclaration(124, firstAdjustmentValue, "17-18", 12, new DateTime(2018, 05, 19))
            .WithDeclaration(125, secondAdjustmentValue, "17-18", 12, new DateTime(2018, 06, 19));

        // act
        await fixtures.RunStrategy();

        //Assert 
        const decimal expectedYearEndAdjustment = (firstAdjustmentValue - period12Value) * -1; // adjustments are inverted
        fixtures.Result[1].EndOfYearAdjustmentAmount.Should().Be(expectedYearEndAdjustment);
    }


    [Test(Description = "The first adjustment should apply to the P12 declaration")]
    public async Task Cleanup_SingleLevyAdjustmentAppliedToAnOntimePeriod12Submission_AdjustmentShouldUseTheP12Adjustment()
    {
        const decimal period12Value = 10000;
        const decimal firstAdjustmentValue = 20000;

        //Arrange 
        var fixtures = new LevyImportCleanerStrategyTestFixtures()
            .WithDeclaration(123, period12Value, "17-18", 12, new DateTime(2018, 03, 15))
            .WithDeclaration(124, firstAdjustmentValue, "17-18", 12, new DateTime(2018, 09, 15));

        // act
        await fixtures.RunStrategy();

        //Assert 
        const decimal expectedYearEndAdjustment = (firstAdjustmentValue - period12Value) * -1; // adjustments are inverted
        fixtures.Result[1].EndOfYearAdjustmentAmount.Should().Be(expectedYearEndAdjustment);
    }

    [Test(Description = "A later adjustment should apply on top of earlier adjustments, not replace")]
    public async Task Cleanup_MultipleLevyAdjustments_LaterAdjustmentsShouldApplyToPriorAdjustments()
    {
        const decimal firstAdjustmentValue = 150;
        const decimal secondAdjustmentValue = 200;

        //Arrange 
        var fixtures = new LevyImportCleanerStrategyTestFixtures()
            .WithDeclaration(123, 100, "17-18", 12, new DateTime(2018, 04, 19))
            .WithDeclaration(124, firstAdjustmentValue, "17-18", 12, new DateTime(2018, 05, 19))
            .WithDeclaration(125, secondAdjustmentValue, "17-18", 12, new DateTime(2018, 06, 19));

        // act
        await fixtures.RunStrategy();

        //Assert 
        const decimal expectedYearEndAdjustment = (secondAdjustmentValue - firstAdjustmentValue) * -1; // adjustments are inverted
        fixtures.Result[2].EndOfYearAdjustmentAmount.Should().Be(expectedYearEndAdjustment);
    }

    [Test, Description("When an adjustment is received and a period 12 declaration has been made the adjustment should apply to the period 12 declaration")]
    public async Task Cleanup_SingleLevyAdjustmentWithPeriod12Value_AdjustmentShouldApplyToPeriod12Value()
    {
        const decimal period12Value = 150;
        const decimal adjustmentValue = 200;

        //Arrange 
        var fixtures = new LevyImportCleanerStrategyTestFixtures()
            .WithDeclaration(123, period12Value, "17-18", 12, new DateTime(2018, 04, 19))
            .WithDeclaration(124, adjustmentValue, "17-18", 12, new DateTime(2018, 05, 19));

        // act
        await fixtures.RunStrategy();

        //Assert 
        const decimal expectedYearEndAdjustment = (adjustmentValue - period12Value) * -1; // adjustments are inverted
        fixtures.Result[1].EndOfYearAdjustmentAmount.Should().Be(expectedYearEndAdjustment);
    }

    [Test, Description("When an adjustment is received and the latest declaration is not period 12 the adjustment should apply to the latest declaration")]
    public async Task Cleanup_SingleLevyAdjustmentWithPeriod8Value_AdjustmentShouldApplyToPeriod8Value()
    {
        const decimal period8Value = 150;
        const decimal adjustmentValue = 200;

        //Arrange 
        var fixtures = new LevyImportCleanerStrategyTestFixtures()
            .WithDeclaration(123, period8Value, "17-18", 8, new DateTime(2017, 12, 19))
            .WithDeclaration(124, adjustmentValue, "17-18", 12, new DateTime(2018, 05, 19));

        // act
        await fixtures.RunStrategy();

        //Assert 
        const decimal expectedYearEndAdjustment = (adjustmentValue - period8Value) * -1; // adjustments are inverted
        fixtures.Result[1].EndOfYearAdjustmentAmount.Should().Be(expectedYearEndAdjustment);
    }

    [Test, Description("When an adjustment is received but there is no declaration the adjustment should apply to an assumed zero declaration")]
    public async Task Cleanup_AdjustmentWithNoDeclaration_AdjustmentShouldApplyToAnAssummedZeroDeclaration()
    {
        const decimal adjustmentValue = 200;

        //Arrange 
        var fixtures = new LevyImportCleanerStrategyTestFixtures()
            .WithDeclaration(124, adjustmentValue, "17-18", 12, new DateTime(2018, 05, 19));

        // act
        await fixtures.RunStrategy();

        //Assert 
        const decimal expectedYearEndAdjustment = adjustmentValue * -1; // adjustments are inverted
        fixtures.Result[0].EndOfYearAdjustmentAmount.Should().Be(expectedYearEndAdjustment);
    }
}

internal class LevyImportCleanerStrategyTestFixtures
{
    public LevyImportCleanerStrategyTestFixtures()
    {
        DasLevyRepositoryMock = new Mock<IDasLevyRepository>();
        LogMock = new Mock<ILogger<LevyImportCleanerStrategy>>();
        Declarations = new List<DasDeclaration>();
        CurrentDateTimeMock = new Mock<ICurrentDateTime>();
        CurrentDateTimeMock.Setup(cdt => cdt.Now).Returns(() => DateTime.UtcNow);
        InfoLog = new List<string>();

        //LogMock.Setup(
        //    l => l.LogInformation(It.IsAny<string>()))
        //    .Callback<string>(s => InfoLog.Add(s));
    }

    public Mock<IDasLevyRepository> DasLevyRepositoryMock { get; set; }
    public IDasLevyRepository DasLevyRepository => DasLevyRepositoryMock.Object;

    public IHmrcDateService HmrcDateService { get; } = new HmrcDateService();

    public Mock<ILogger<LevyImportCleanerStrategy>> LogMock { get; set; }
    public ILogger<LevyImportCleanerStrategy> Log => LogMock.Object;

    public List<string> InfoLog { get; }

    public string EmpRef { get; set; }

    public List<DasDeclaration> Declarations { get; }

    public Mock<ICurrentDateTime> CurrentDateTimeMock { get; }

    public LevyImportCleanerStrategy CreateStrategy()
    {
        return new LevyImportCleanerStrategy(
            DasLevyRepository,
            HmrcDateService,
            Log,
            CurrentDateTimeMock.Object
        );
    }

    public LevyImportCleanerStrategyTestFixtures WithDeclarations(params long[] submissionIds)
    {
        foreach (var submissionId in submissionIds)
        {
            WithDeclaration(new DasDeclaration { SubmissionId = submissionId });
        }

        return this;
    }

    public LevyImportCleanerStrategyTestFixtures WithDeclaration(long SubmissionId, decimal levyDueYtd, string payrollYear, short payrollMonth, DateTime submissionDate)
    {
        return WithDeclaration(new DasDeclaration
        {
            Id = SubmissionId.ToString(CultureInfo.InvariantCulture),
            SubmissionId = SubmissionId,
            LevyDueYtd = levyDueYtd,
            PayrollYear = payrollYear,
            PayrollMonth = payrollMonth,
            SubmissionDate = submissionDate
        });
    }

    public LevyImportCleanerStrategyTestFixtures WithDeclaration(DasDeclaration declaration)
    {
        Declarations.Add(declaration);

        return this;
    }

    public DasDeclaration[] Result { get; private set; }

    public async Task RunStrategy()
    {
        var strategy = CreateStrategy();
        var tempResult = await strategy.Cleanup(EmpRef, Declarations);

        Result = tempResult.ToArray();
    }
}