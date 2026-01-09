CREATE PROCEDURE [employer_financial].[GetPaymentForPaymentDetails]
(
    @PaymentId uniqueidentifier    
)
AS
SELECT
    p.PaymentId AS Id
    ,p.PaymentMetaDataId
    ,p.ApprenticeshipId
    ,p.Ukprn
    ,pmd.StandardCode
    ,pmd.FrameworkCode
    ,pmd.PathwayCode
    ,pmd.ProgrammeType
    ,pmd.ApprenticeName
    ,pmd.ProviderName
FROM [employer_financial].[Payment] p
JOIN [employer_financial].[PaymentMetaData] pmd ON p.PaymentMetaDataId = pmd.Id
WHERE p.PaymentId = @paymentId    