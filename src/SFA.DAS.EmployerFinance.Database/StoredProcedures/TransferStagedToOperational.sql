CREATE PROCEDURE [employer_financial].[TransferStagedToOperational]
    @accountId BIGINT,
    @periodEndRef VARCHAR(25)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @metadataCount INT = 0;
        DECLARE @paymentsCount INT = 0;
        DECLARE @transfersCount INT = 0;
        DECLARE @transactionLinesCount INT = 0;

        ---------------------------------------------------------------------
        -- 1. Insert PaymentMetaData (scoped to account + period payments)
        ---------------------------------------------------------------------

        DECLARE @PaymentMetaDataMap TABLE
        (
            StagingPaymentMetaDataId     BIGINT,
            OperationalPaymentMetaDataId BIGINT
        );

        MERGE employer_financial.PaymentMetaData AS target
        USING
        (
            SELECT
                pms.Id,
                pms.ProviderName,
                pms.StandardCode,
                pms.FrameworkCode,
                pms.ProgrammeType,
                pms.PathwayCode,
                pms.PathwayName,
                pms.ApprenticeshipCourseName,
                pms.ApprenticeshipCourseStartDate,
                pms.ApprenticeshipCourseLevel,
                pms.ApprenticeName,
                pms.ApprenticeNINumber,
                pms.IsHistoricProviderName
            FROM employer_financial.PaymentMetaDataStaging pms
            INNER JOIN employer_financial.PaymentStaging ps
                ON ps.PaymentId = pms.PaymentId
            WHERE ps.AccountId = @accountId
              AND ps.CollectionPeriodId = @periodEndRef
        ) AS source
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
                IsHistoricProviderName
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
                source.IsHistoricProviderName
            )
        OUTPUT
            source.Id,
            inserted.Id
        INTO @PaymentMetaDataMap
        (
            StagingPaymentMetaDataId,
            OperationalPaymentMetaDataId
        );

        SET @metadataCount = @@ROWCOUNT;

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

        SET @paymentsCount = @@ROWCOUNT;

        ---------------------------------------------------------------------
        -- 3. Insert AccountTransfers from TransferStaging
        ---------------------------------------------------------------------

        INSERT INTO employer_financial.AccountTransfers
        (
            SenderAccountId,
            SenderAccountName,
            ReceiverAccountId,
            ReceiverAccountName,
            ApprenticeshipId,
            CourseName,
            CourseLevel,
            LearningType,
            PeriodEnd,
            Amount,
            [Type],
            CreatedDate,
            RequiredPaymentId
        )
        SELECT
            ts.SenderAccountId,
            ts.SenderAccountName,
            ts.ReceiverAccountId,
            ts.ReceiverAccountName,
            ts.ApprenticeshipId,
            ISNULL(ts.CourseName, ''),
            ts.CourseLevel,
            ts.LearningType,
            ts.PeriodEnd,
            ts.Amount,
            ts.[Type],
            ts.TransferDate,
            ts.RequiredPaymentId
        FROM employer_financial.TransferStaging ts
        WHERE ts.ReceiverAccountId = @accountId
          AND ts.PeriodEnd = @periodEndRef;

        SET @transfersCount = @@ROWCOUNT;

        ---------------------------------------------------------------------
        -- 4. Insert TransactionLines from TransactionLineStaging
        ---------------------------------------------------------------------

        INSERT INTO employer_financial.TransactionLine
        (
            AccountId,
            DateCreated,
            SubmissionId,
            TransactionDate,
            TransactionType,
            LevyDeclared,
            Amount,
            EmpRef,
            PeriodEnd,
            Ukprn,
            SfaCoInvestmentAmount,
            EmployerCoInvestmentAmount,
            EnglishFraction,
            TransferSenderAccountId,
            TransferSenderAccountName,
            TransferReceiverAccountId,
            TransferReceiverAccountName
        )
        SELECT
            tls.AccountId,
            tls.DateCreated,
            tls.SubmissionId,
            tls.TransactionDate,
            tls.TransactionType,
            tls.LevyDeclared,
            tls.Amount,
            tls.EmpRef,
            tls.PeriodEnd,
            tls.Ukprn,
            tls.SfaCoInvestmentAmount,
            tls.EmployerCoInvestmentAmount,
            tls.EnglishFraction,
            tls.TransferSenderAccountId,
            tls.TransferSenderAccountName,
            tls.TransferReceiverAccountId,
            tls.TransferReceiverAccountName
        FROM employer_financial.TransactionLineStaging tls
        WHERE tls.AccountId = @accountId
          AND tls.PeriodEnd = @periodEndRef;

        SET @transactionLinesCount = @@ROWCOUNT;

        COMMIT TRANSACTION;

        SELECT
            (@paymentsCount + @transfersCount + @transactionLinesCount) AS ProcessedCount,
            @paymentsCount AS PaymentsCount,
            @transfersCount AS TransfersCount,
            @transactionLinesCount AS TransactionLinesCount,
            @metadataCount AS MetadataCount;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO
