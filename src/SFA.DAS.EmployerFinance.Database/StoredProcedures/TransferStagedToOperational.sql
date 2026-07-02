CREATE PROCEDURE [employer_financial].[TransferStagedToOperational]
    @accountId BIGINT,
    @periodEndRef VARCHAR(25)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        ---------------------------------------------------------------------
        -- 1. Insert PaymentMetaData
        ---------------------------------------------------------------------

        DECLARE @PaymentMetaDataMap TABLE
        (
            StagingPaymentMetaDataId     BIGINT,
            OperationalPaymentMetaDataId BIGINT
        );

        MERGE employer_financial.PaymentMetaData AS target
        USING employer_financial.PaymentMetaDataStaging AS source
            ON 1 = 0 -- insert-only
        WHEN NOT MATCHED THEN
            INSERT
            (
                ProviderName,
                StandardCode,
                FrameworkCode,
                ProgrammeType,
                PathwayCode,
                PathwayName,
                ApprenticeshipCourseName,
                ApprenticeshipCourseStartDate,
                ApprenticeshipCourseLevel,
                ApprenticeName,
                ApprenticeNINumber,
                IsHistoricProviderName,
                LearningType,
                CourseCode,
                CohortId
            )
            VALUES
            (
                source.ProviderName,
                source.StandardCode,
                source.FrameworkCode,
                source.ProgrammeType,
                source.PathwayCode,
                source.PathwayName,
                source.ApprenticeshipCourseName,
                source.ApprenticeshipCourseStartDate,
                source.ApprenticeshipCourseLevel,
                source.ApprenticeName,
                source.ApprenticeNINumber,
                source.IsHistoricProviderName,
                source.LearningType,
                source.CourseCode,
                source.CohortId
            )
        OUTPUT
            source.Id,
            inserted.Id
        INTO @PaymentMetaDataMap
        (
            StagingPaymentMetaDataId,
            OperationalPaymentMetaDataId
        );

        ---------------------------------------------------------------------
        -- 2. Insert Payments
        ---------------------------------------------------------------------

        MERGE employer_financial.Payment AS target
        USING
        (
            SELECT
                ps.PaymentId,
                ps.Ukprn,
                ps.Uln,
                ps.AccountId,
                ps.ApprenticeshipId,
                ps.DeliveryPeriodMonth,
                ps.DeliveryPeriodYear,
                ps.CollectionPeriodId,
                ps.CollectionPeriodMonth,
                ps.CollectionPeriodYear,
                ps.EvidenceSubmittedOn,
                ps.EmployerAccountVersion,
                ps.ApprenticeshipVersion,
                ps.FundingSource,
                ps.TransactionType,
                ps.Amount,
                map.OperationalPaymentMetaDataId AS PaymentMetaDataId
            FROM employer_financial.PaymentStaging ps
            INNER JOIN employer_financial.PaymentMetaDataStaging pms
                ON ps.PaymentId = pms.PaymentId
            INNER JOIN @PaymentMetaDataMap map
                ON pms.Id = map.StagingPaymentMetaDataId
            WHERE ps.AccountId = @accountId
              AND ps.CollectionPeriodId = @periodEndRef
        ) AS source
            ON 1 = 0 -- insert-only
        WHEN NOT MATCHED THEN
            INSERT
            (
                PaymentId,
                Ukprn,
                Uln,
                AccountId,
                ApprenticeshipId,
                DeliveryPeriodMonth,
                DeliveryPeriodYear,
                CollectionPeriodId,
                CollectionPeriodMonth,
                CollectionPeriodYear,
                EvidenceSubmittedOn,
                EmployerAccountVersion,
                ApprenticeshipVersion,
                FundingSource,
                TransactionType,
                Amount,
                PaymentMetaDataId
            )
            VALUES
            (
                source.PaymentId,
                source.Ukprn,
                source.Uln,
                source.AccountId,
                source.ApprenticeshipId,
                source.DeliveryPeriodMonth,
                source.DeliveryPeriodYear,
                source.CollectionPeriodId,
                source.CollectionPeriodMonth,
                source.CollectionPeriodYear,
                source.EvidenceSubmittedOn,
                source.EmployerAccountVersion,
                source.ApprenticeshipVersion,
                source.FundingSource,
                source.TransactionType,
                source.Amount,
                source.PaymentMetaDataId
            );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO
