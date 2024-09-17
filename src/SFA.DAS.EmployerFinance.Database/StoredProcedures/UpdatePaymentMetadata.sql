CREATE PROCEDURE [employer_financial].[UpdatePaymentMetadata]
(
    @ProviderName VARCHAR(MAX),
    @ApprenticeName VARCHAR(MAX),
    @IsHistoricProviderName BIT,
    @CourseName VARCHAR(MAX),
    @CourseLevel INT,
    @CourseStartDate DATETIME,
    @PathwayName VARCHAR(MAX),
    @PaymentMetaDataId BIGINT
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
WHERE [Id] = @PaymentMetadataId

UPDATE at
SET at.CourseName = @CourseName,
    at.CourseLevel = @CourseLevel
    FROM employer_financial.AccountTransfers at
    INNER JOIN employer_financial.Payment p
ON at.ApprenticeshipId = p.ApprenticeshipId
    AND at.PeriodEnd = p.PeriodEnd
    AND at.ReceiverAccountId = p.AccountId
WHERE p.PaymentMetaDataId = @PaymentMetaDataId;