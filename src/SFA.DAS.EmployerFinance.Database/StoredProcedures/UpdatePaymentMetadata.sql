CREATE PROCEDURE [employer_financial].[UpdatePaymentMetadata]
(
    @ProviderName VARCHAR(MAX),
    @ApprenticeName VARCHAR(MAX),
    @IsHistoricProviderName BIT,
    @CourseName VARCHAR(MAX),
    @CourseLevel INT,
    @CourseStartDate DATETIME,
    @PathwayName VARCHAR(MAX),
    @PaymentId BIGINT
)
AS

UPDATE [employer_financial].[PaymentMetaData]
SET
    ProviderName = @ProviderName
    ,ApprenticeName = @ApprenticeName   
    ,IsHistoricProviderName = @IsHistoricProviderName
    ,ApprenticeshipCourseName = @CourseName
    ,ApprenticeshipCourseLevel = @CourseLevel
    ,ApprenticeshipCourseStartDate = @CourseStartDate
    ,PathwayName = @PathwayName
WHERE [Id] = @PaymentID