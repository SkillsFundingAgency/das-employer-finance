using NUnit.Framework;
using SFA.DAS.EmployerFinance.Commands.UpdatePaymentMetadataStaging;
using SFA.DAS.EmployerFinance.Models.Payments;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.UpdatePaymentMetadataStaging;

public class WhenValidatingUpdatePaymentMetadataStagingCommand
{
    private UpdatePaymentMetadataStagingCommandValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new UpdatePaymentMetadataStagingCommandValidator();
    }

    [Test]
    public void Then_Partial_Metadata_Is_Valid()
    {
        var result = _validator.Validate(new UpdatePaymentMetadataStagingCommand
        {
            PaymentId = Guid.NewGuid(),
            PaymentMetadataStaging = new PaymentMetaDataStaging()
        });

        Assert.That(result.IsValid(), Is.True);
    }

    [Test]
    public void Then_App_Unit_Metadata_Fields_Are_Valid()
    {
        var result = _validator.Validate(new UpdatePaymentMetadataStagingCommand
        {
            PaymentId = Guid.NewGuid(),
            PaymentMetadataStaging = new PaymentMetaDataStaging
            {
                LearningType = "ApprenticeshipUnit",
                CourseCode = "ST0001",
                CohortId = 123456
            }
        });

        Assert.That(result.IsValid(), Is.True);
    }

    [Test]
    public void Then_Invalid_Ni_Is_Rejected_When_Supplied()
    {
        var result = _validator.Validate(new UpdatePaymentMetadataStagingCommand
        {
            PaymentId = Guid.NewGuid(),
            PaymentMetadataStaging = new PaymentMetaDataStaging
            {
                ApprenticeNINumber = "not-valid"
            }
        });

        Assert.That(result.IsValid(), Is.False);
        Assert.That(result.ValidationDictionary.Values, Does.Contain("Invalid NI format"));
    }

    [Test]
    public void Then_Old_Start_Date_Is_Rejected_When_Supplied()
    {
        var result = _validator.Validate(new UpdatePaymentMetadataStagingCommand
        {
            PaymentId = Guid.NewGuid(),
            PaymentMetadataStaging = new PaymentMetaDataStaging
            {
                ApprenticeshipCourseStartDate = new DateTime(1900, 1, 1)
            }
        });

        Assert.That(result.IsValid(), Is.False);
        Assert.That(result.ValidationDictionary.Values, Does.Contain("StartDate must be after 1900-01-01"));
    }
}
