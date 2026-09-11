-- =============================================================================
-- SCENARIO: Transfer Sender And Receiver
--
-- An employer account that simultaneously pays levy, receives inbound transfers
-- from one account, and sends outbound transfers to another. Useful for
-- testing the balance calculation and UI display when transfers flow in both
-- directions through the same account.
--
-- Accounts:
--   AccountId 300  -  Mixed Transfer Co   (PAYE: 300/MX00001)   [levy + receives + sends]
--   AccountId 301  -  Upstream Sender Ltd (PAYE: 301/UP00001)   [levy + sends to 300]
--   AccountId 302  -  Downstream Ltd                             [receives from 300, no levy]
--
-- Data generated:
--   Levies:    AccountId 300 - 25 months at £3,000/month
--              AccountId 301 - 25 months at £5,000/month (funds transfers to 300)
--   Transfers: AccountId 301 → 300, £2,000/month for 25 months
--              AccountId 300 → 302, £1,500/month for 25 months
-- Dates:      TransactionLine dates set from levy SubmissionDate (organic history)
-- Dependencies: none (self-contained)
-- =============================================================================

CREATE FUNCTION PayrollMonth (@date datetime)
RETURNS int
AS
BEGIN
    DECLARE @month int = DATEPART(month, @date) - 3
    IF @month < 1 SET @month = @month + 12
    RETURN(@month)
END
GO

CREATE FUNCTION PayrollYear (@date datetime)
RETURNS VARCHAR(5)
AS
BEGIN
    DECLARE @month int = DATEPART(month, @date)
    DECLARE @year  int = DATEPART(year,  @date)
    IF @month < 4 SET @year = @year - 1
    RETURN (SELECT RIGHT(CONVERT(VARCHAR(5), @year, 1), 2)) + '-' + (SELECT RIGHT(CONVERT(VARCHAR(4), @year+1, 1), 2))
END
GO

-- =================== HELPER: generate levy for one account ==================

CREATE OR ALTER PROCEDURE #createLeviesForAccount (
    @accountId         bigint,
    @payeScheme        nvarchar(50),
    @monthlyLevy       decimal(18,4),
    @levyAllowance     decimal(18,4),
    @numberOfMonths    int,
    @toDate            datetime2
) AS
BEGIN
    DECLARE @levyDecByMonth TABLE (monthBeforeToDate int, amount decimal(18,4), payrollYear varchar(5), payrollMonth int)

    DECLARE @firstPayrollMonth datetime   = DATEADD(month, -@numberOfMonths+1-1, @toDate)
    DECLARE @firstPayrollYear  VARCHAR(5) = dbo.PayrollYear(@firstPayrollMonth)
    DECLARE @n int = 1

    WHILE @n <= @numberOfMonths
    BEGIN
        INSERT INTO @levyDecByMonth (monthBeforeToDate, amount, payrollYear, payrollMonth)
        VALUES (
            -@numberOfMonths + @n,
            (CASE
                WHEN dbo.PayrollYear(DATEADD(month, -1-@numberOfMonths+@n, @toDate)) = @firstPayrollYear
                    THEN @monthlyLevy * @n
                    ELSE @monthlyLevy * dbo.PayrollMonth(DATEADD(month, -1-@numberOfMonths+@n, @toDate))
            END),
            dbo.PayrollYear(DATEADD(month,  -1-@numberOfMonths+@n, @toDate)),
            dbo.PayrollMonth(DATEADD(month, -1-@numberOfMonths+@n, @toDate)))
        SET @n = @n + 1
    END

    -- English fractions
    DECLARE @fractionMonths TABLE (dateCalculated datetime)
    DECLARE @newFractionMonths TABLE (dateCalculated datetime)
    DECLARE @firstMonth datetime = DATEADD(month, -@numberOfMonths+1, @toDate)

    INSERT @fractionMonths
    SELECT TOP 1 DATEFROMPARTS(DATEPART(year, m.d), DATEPART(month, m.d), 7)
    FROM (SELECT DATEADD(month, -DATEPART(month, @firstMonth)%3, @firstMonth) AS d) m

    INSERT @fractionMonths
    SELECT DATEFROMPARTS(DATEPART(year, DATEADD(month, monthBeforeToDate, @toDate)),
                         DATEPART(month, DATEADD(month, monthBeforeToDate, @toDate)), 7)
    FROM @levyDecByMonth
    WHERE DATEPART(month, DATEADD(month, monthBeforeToDate, @toDate))%3 = 0

    INSERT @newFractionMonths
    SELECT dateCalculated FROM @fractionMonths
    EXCEPT SELECT dateCalculated FROM employer_financial.EnglishFraction WHERE EmpRef = @payeScheme

    INSERT employer_financial.EnglishFraction (DateCalculated, Amount, EmpRef, DateCreated)
    SELECT dateCalculated, 1.0, @payeScheme, dateCalculated FROM @newFractionMonths

    DECLARE @maxId              bigint   = ISNULL((SELECT MAX(SubmissionId) FROM employer_financial.LevyDeclaration), 0)
    DECLARE @baselineSubmission datetime = DATEFROMPARTS(YEAR(@toDate), MONTH(@toDate), 18)
    DECLARE @baselinePayroll    datetime = DATEADD(month, -1, @toDate)

    INSERT INTO employer_financial.LevyDeclaration
        (AccountId, EmpRef, LevyDueYTD, LevyAllowanceForYear, SubmissionDate, SubmissionId, PayrollYear, PayrollMonth, CreatedDate, HmrcSubmissionId)
    SELECT
        @accountId, @payeScheme, amount, @levyAllowance,
        DATEADD(month, monthBeforeToDate, @baselineSubmission),
        @maxId + ROW_NUMBER() OVER (ORDER BY (SELECT NULL)),
        dbo.PayrollYear(DATEADD(month,  monthBeforeToDate, @baselinePayroll)),
        dbo.PayrollMonth(DATEADD(month, monthBeforeToDate, @baselinePayroll)),
        DATEADD(month, monthBeforeToDate, @baselineSubmission),
        @maxId + ROW_NUMBER() OVER (ORDER BY (SELECT NULL))
    FROM @levyDecByMonth

    EXEC employer_financial.ProcessDeclarationsTransactions @accountId, @payeScheme, @toDate, 24
END
GO

-- ======================== LEVY: UPSTREAM SENDER (301) =======================

BEGIN TRANSACTION ScenarioSenderAndReceiverLevies301
GO
DECLARE @toDate datetime2 = GETDATE()
EXEC #createLeviesForAccount 301, '301/UP00001', 5000, 7500, 25, @toDate
GO
COMMIT TRANSACTION ScenarioSenderAndReceiverLevies301
GO

-- ======================== LEVY: MIXED ACCOUNT (300) =========================

BEGIN TRANSACTION ScenarioSenderAndReceiverLevies300
GO
DECLARE @toDate datetime2 = GETDATE()
EXEC #createLeviesForAccount 300, '300/MX00001', 3000, 4500, 25, @toDate
GO
COMMIT TRANSACTION ScenarioSenderAndReceiverLevies300
GO

DROP FUNCTION PayrollYear
GO
DROP FUNCTION PayrollMonth
GO

-- ========================== TRANSFER HELPER FUNCTIONS =======================

CREATE FUNCTION CalendarPeriodMonth (@date datetime) RETURNS int AS BEGIN RETURN DATEPART(month, @date) END
GO
CREATE FUNCTION CalendarPeriodYear  (@date datetime) RETURNS int AS BEGIN RETURN DATEPART(year,  @date) END
GO
CREATE FUNCTION CollectionPeriodMonth (@date datetime) RETURNS int AS BEGIN RETURN dbo.CalendarPeriodMonth(@date) END
GO
CREATE FUNCTION CollectionPeriodYear  (@date datetime) RETURNS int AS BEGIN RETURN dbo.CalendarPeriodYear(@date)  END
GO

CREATE FUNCTION PeriodEndMonth (@date datetime) RETURNS VARCHAR(5)
AS
BEGIN
    DECLARE @month int = DATEPART(month, @date) - 7
    IF @month < 1 SET @month = @month + 12
    RETURN CONVERT(varchar(5), @month)
END
GO

CREATE FUNCTION PeriodEndYear (@date datetime) RETURNS VARCHAR(5)
AS
BEGIN
    DECLARE @month int = DATEPART(month, @date)
    DECLARE @year  int = DATEPART(year,  @date)
    IF @month < 8 SET @year = @year - 1
    RETURN (SELECT RIGHT(CONVERT(VARCHAR(5), @year, 1), 2)) + (SELECT RIGHT(CONVERT(VARCHAR(4), @year+1, 1), 2))
END
GO

CREATE FUNCTION PeriodEnd (@date datetime) RETURNS VARCHAR(8) AS BEGIN RETURN dbo.PeriodEndYear(@date) + '-R' + RIGHT('0' + dbo.PeriodEndMonth(@date), 2) END
GO

CREATE OR ALTER PROCEDURE #createPeriodEnd (@periodEndDate datetime) AS
BEGIN
    DECLARE @id VARCHAR(8) = dbo.PeriodEnd(@periodEndDate)
    IF NOT EXISTS (SELECT 1 FROM employer_financial.PeriodEnd WHERE PeriodEndId = @id)
        INSERT employer_financial.PeriodEnd (PeriodEndId, CalendarPeriodMonth, CalendarPeriodYear, AccountDataValidAt, CommitmentDataValidAt, CompletionDateTime, PaymentsForPeriod)
        VALUES (@id, dbo.CalendarPeriodMonth(@periodEndDate), dbo.CalendarPeriodYear(@periodEndDate),
                '2018-05-04 00:00:00.000', '2018-05-04 09:07:34.457', @periodEndDate,
                'https://pp-payments.apprenticeships.sfa.bis.gov.uk/api/payments?periodId=' + @id)
END
GO

CREATE OR ALTER PROCEDURE #createPayment (
    @accountId bigint, @providerName nvarchar(max), @courseName nvarchar(max), @courseLevel int,
    @apprenticeName varchar(max), @ukprn bigint, @uln bigint, @apprenticeshipId bigint,
    @fundingSource int, @amount decimal(18,5), @periodEndDate datetime
) AS
BEGIN
    DECLARE @metadataId bigint
    INSERT INTO employer_financial.PaymentMetadata
        (ProviderName, StandardCode, FrameworkCode, ProgrammeType, PathwayCode, ApprenticeshipCourseName,
         ApprenticeshipCourseStartDate, ApprenticeshipCourseLevel, ApprenticeName, ApprenticeNINumber)
    VALUES (@providerName, 4, null, null, null, @courseName, '01/06/2018', @courseLevel, @apprenticeName, null)
    SELECT @metadataId = SCOPE_IDENTITY()
    DECLARE @deliveryDate datetime = DATEADD(month, -FLOOR(RAND()*6), @periodEndDate)
    INSERT INTO employer_financial.Payment
        (PaymentId, Ukprn, Uln, AccountId, ApprenticeshipId,
         DeliveryPeriodMonth, DeliveryPeriodYear, CollectionPeriodId, CollectionPeriodMonth, CollectionPeriodYear,
         EvidenceSubmittedOn, EmployerAccountVersion, ApprenticeshipVersion, FundingSource, TransactionType,
         Amount, PeriodEnd, PaymentMetadataId)
    VALUES
        (NEWID(), @ukprn, @uln, @accountId, @apprenticeshipId,
         dbo.CalendarPeriodMonth(@deliveryDate), dbo.CalendarPeriodYear(@deliveryDate),
         dbo.PeriodEnd(@periodEndDate), dbo.CollectionPeriodMonth(@periodEndDate), dbo.CollectionPeriodYear(@periodEndDate),
         '2018-06-03 16:24:22.340', 20170504, 69985, @fundingSource, 1, @amount, dbo.PeriodEnd(@periodEndDate), @metadataId)
END
GO

CREATE OR ALTER PROCEDURE #processPayments (@accountId bigint, @dateCreated datetime) AS
INSERT INTO employer_financial.TransactionLine
SELECT mainUpdate.* FROM
    (SELECT x.AccountId,
        DATEFROMPARTS(DATEPART(yyyy,@dateCreated), DATEPART(MM,@dateCreated), DATEPART(dd,@dateCreated)) AS DateCreated,
        NULL AS SubmissionId, MAX(pe.CompletionDateTime) AS TransactionDate, 3 AS TransactionType,
        NULL AS LevyDeclared, SUM(ISNULL(p.Amount,0)) * -1 AS Amount, NULL AS EmpRef,
        x.PeriodEnd, x.Ukprn,
        SUM(ISNULL(pco.Amount,0)) * -1 AS SfaCoInvestmentAmount,
        SUM(ISNULL(pci.Amount,0)) * -1 AS EmployerCoInvestmentAmount,
        0 AS EnglishFraction,
        NULL AS TransferSenderAccountId, NULL AS TransferSenderAccountName,
        NULL AS TransferReceiverAccountId, NULL AS TransferReceiverAccountName
    FROM employer_financial.Payment x
    INNER JOIN employer_financial.PeriodEnd pe ON pe.PeriodEndId = x.PeriodEnd
    LEFT  JOIN employer_financial.Payment p    ON p.PeriodEnd = pe.PeriodEndId AND p.PaymentId = x.PaymentId AND p.FundingSource IN (1,5)
    LEFT  JOIN employer_financial.Payment pco  ON pco.PeriodEnd = pe.PeriodEndId AND pco.PaymentId = x.PaymentId AND pco.FundingSource = 2
    LEFT  JOIN employer_financial.Payment pci  ON pci.PeriodEnd = pe.PeriodEndId AND pci.PaymentId = x.PaymentId AND pci.FundingSource = 3
    WHERE x.AccountId = @accountId
    GROUP BY x.Ukprn, x.PeriodEnd, x.AccountId
    ) mainUpdate
    INNER JOIN (
        SELECT AccountId, Ukprn, PeriodEnd FROM employer_financial.Payment WHERE FundingSource IN (1,2,3,5)
        EXCEPT
        SELECT AccountId, Ukprn, PeriodEnd FROM employer_financial.TransactionLine WHERE TransactionType = 3
    ) dervx ON dervx.AccountId = mainUpdate.AccountId AND dervx.PeriodEnd = mainUpdate.PeriodEnd AND dervx.Ukprn = mainUpdate.Ukprn
GO

CREATE OR ALTER PROCEDURE #createTransferTransaction (
    @accountId bigint, @senderAccountId bigint, @senderAccountName nvarchar(100),
    @receiverAccountId bigint, @receiverAccountName nvarchar(100),
    @periodEnd nvarchar(20), @amount decimal(18,4), @createDate datetime
) AS
BEGIN
    INSERT INTO employer_financial.TransactionLine
        (AccountId, DateCreated, TransactionDate, TransactionType, Amount, PeriodEnd,
         TransferSenderAccountId, TransferSenderAccountName, TransferReceiverAccountId, TransferReceiverAccountName)
    VALUES
        (@accountId, @createDate, @createDate, 4, @amount, @periodEnd,
         @senderAccountId, @senderAccountName, @receiverAccountId, @receiverAccountName)
END
GO

CREATE OR ALTER PROCEDURE #createTransferForMonth (
    @senderAccountId   bigint, @senderAccountName   nvarchar(100),
    @receiverAccountId bigint, @receiverAccountName nvarchar(100),
    @createDate datetime, @totalAmount decimal(18,5)
) AS
BEGIN
BEGIN TRANSACTION
    DECLARE @periodEndDate    datetime  = DATEADD(month, -1, @createDate)
    DECLARE @periodEndId      varchar(8) = dbo.PeriodEnd(@periodEndDate)
    DECLARE @courseName       nvarchar(max) = 'Plate Spinning'
    DECLARE @ukprn            bigint = 10001378
    DECLARE @apprenticeshipId bigint = ISNULL((SELECT MAX(ApprenticeshipId) FROM employer_financial.Payment), 0) + 1

    EXEC #createPeriodEnd @periodEndDate

    EXEC #createPayment @receiverAccountId, 'CHESTERFIELD COLLEGE', @courseName, 1,
        'A Apprentice', @ukprn, 1000000000, @apprenticeshipId, /*LevyTransfer*/5, @totalAmount, @periodEndDate

    INSERT INTO employer_financial.AccountTransfers
        (SenderAccountId, SenderAccountName, ReceiverAccountId, ReceiverAccountName,
         ApprenticeshipId, CourseName, PeriodEnd, Amount, Type, CreatedDate, RequiredPaymentId)
    VALUES
        (@senderAccountId, @senderAccountName, @receiverAccountId, @receiverAccountName,
         @apprenticeshipId, @courseName, @periodEndId, @totalAmount, 'Levy', @createDate, NEWID())

    EXEC #createTransferTransaction @senderAccountId,   @senderAccountId, @senderAccountName, @receiverAccountId, @receiverAccountName, @periodEndId, -@totalAmount, @createDate
    EXEC #createTransferTransaction @receiverAccountId, @senderAccountId, @senderAccountName, @receiverAccountId, @receiverAccountName, @periodEndId,  @totalAmount, @createDate

    DELETE employer_financial.TransactionLine WHERE AccountId = @receiverAccountId AND Ukprn = @ukprn AND PeriodEnd = @periodEndId AND TransactionType = 3
    EXEC #processPayments @receiverAccountId, @createDate
COMMIT TRANSACTION
END
GO

-- ===================== TRANSFERS: 301 → 300 (inbound to mixed account) ======

DECLARE @toDate             datetime     = GETDATE()
DECLARE @numberOfMonthsToCreate int      = 25
DECLARE @monthBeforeToDate  int          = 1
DECLARE @createDate         datetime
DECLARE @months TABLE (monthBeforeToDate int, createMonth datetime)

INSERT INTO @months
SELECT TOP (@numberOfMonthsToCreate)
    -@numberOfMonthsToCreate + ROW_NUMBER() OVER (ORDER BY [object_id]),
    DATEADD(month, -@numberOfMonthsToCreate + ROW_NUMBER() OVER (ORDER BY [object_id]), @toDate)
FROM sys.all_objects ORDER BY 1

WHILE (1=1)
BEGIN
    SELECT TOP 1 @monthBeforeToDate = monthBeforeToDate, @createDate = createMonth
    FROM @months WHERE monthBeforeToDate < @monthBeforeToDate ORDER BY monthBeforeToDate DESC
    IF @@ROWCOUNT = 0 BREAK
    EXEC #createTransferForMonth 301, 'Upstream Sender Ltd', 300, 'Mixed Transfer Co', @createDate, 2000
END

-- ===================== TRANSFERS: 300 → 302 (outbound from mixed account) ===

SET @monthBeforeToDate = 1
DELETE @months

INSERT INTO @months
SELECT TOP (@numberOfMonthsToCreate)
    -@numberOfMonthsToCreate + ROW_NUMBER() OVER (ORDER BY [object_id]),
    DATEADD(month, -@numberOfMonthsToCreate + ROW_NUMBER() OVER (ORDER BY [object_id]), @toDate)
FROM sys.all_objects ORDER BY 1

WHILE (1=1)
BEGIN
    SELECT TOP 1 @monthBeforeToDate = monthBeforeToDate, @createDate = createMonth
    FROM @months WHERE monthBeforeToDate < @monthBeforeToDate ORDER BY monthBeforeToDate DESC
    IF @@ROWCOUNT = 0 BREAK
    EXEC #createTransferForMonth 300, 'Mixed Transfer Co', 302, 'Downstream Ltd', @createDate, 1500
END

DROP FUNCTION CalendarPeriodYear
GO
DROP FUNCTION CalendarPeriodMonth
GO
DROP FUNCTION CollectionPeriodYear
GO
DROP FUNCTION CollectionPeriodMonth
GO
DROP FUNCTION PeriodEndYear
GO
DROP FUNCTION PeriodEndMonth
GO
DROP FUNCTION PeriodEnd
GO
