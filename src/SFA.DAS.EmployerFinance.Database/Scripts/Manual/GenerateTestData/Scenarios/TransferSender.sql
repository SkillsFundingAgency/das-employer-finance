-- =============================================================================
-- SCENARIO: Transfer Sender and Receiver
--
-- A high-levy employer (sender) that sends transfers to a receiver each month.
-- Creates levy declarations for both accounts and transfer transaction lines.
--
-- Requires SQLCMD mode: Query > SQLCMD Mode in SSMS, or natively in Azure Data Studio
-- =============================================================================

-- ===================== CONFIGURATION - CHANGE THESE ========================
:setvar SenderAccountId        200
:setvar SenderPayeScheme       "200/TS00001"
:setvar SenderAccountName      "Transfer Sender Ltd"
:setvar SenderMonthlyLevy      5000
:setvar SenderLevyAllowance    7500
:setvar ReceiverAccountId      201
:setvar ReceiverPayeScheme     "201/TR00001"
:setvar ReceiverAccountName    "Transfer Receiver Ltd"
:setvar ReceiverMonthlyLevy    1000
:setvar ReceiverLevyAllowance  1500
:setvar NumberOfMonths         25
:setvar MonthlyTransfer        2000
:setvar PaymentsPerMonth       1
-- ===========================================================================

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

-- ======================= LEVIES FOR SENDER ACCOUNT ==========================

BEGIN TRANSACTION ScenarioTransferSenderLevies
GO

DECLARE @accountId              bigint          = $(SenderAccountId)
DECLARE @payeScheme             nvarchar(50)    = '$(SenderPayeScheme)'
DECLARE @monthlyLevy            decimal(18,4)   = $(SenderMonthlyLevy)
DECLARE @toDate                 datetime2       = GETDATE()
DECLARE @numberOfMonthsToCreate int             = $(NumberOfMonths)

DECLARE @levyDecByMonth TABLE (monthBeforeToDate int, amount decimal(18,4), createMonth datetime, payrollYear varchar(5), payrollMonth int)

DECLARE @firstPayrollMonth  datetime   = DATEADD(month, -@numberOfMonthsToCreate+1-1, @toDate)
DECLARE @firstPayrollYear   VARCHAR(5) = dbo.PayrollYear(@firstPayrollMonth)
DECLARE @monthNumber        int        = 1

WHILE @monthNumber <= @numberOfMonthsToCreate
BEGIN
    INSERT INTO @levyDecByMonth (monthBeforeToDate, amount, createMonth, payrollYear, payrollMonth)
    VALUES (
        -@numberOfMonthsToCreate + @monthNumber,
        (CASE
            WHEN dbo.PayrollYear(DATEADD(month, -1-@numberOfMonthsToCreate+@monthNumber, @toDate)) = @firstPayrollYear
                THEN @monthlyLevy * @monthNumber
                ELSE @monthlyLevy * dbo.PayrollMonth(DATEADD(month, -1-@numberOfMonthsToCreate+@monthNumber, @toDate))
        END),
        DATEADD(month, -@numberOfMonthsToCreate+@monthNumber, @toDate),
        dbo.PayrollYear(DATEADD(month,  -1-@numberOfMonthsToCreate+@monthNumber, @toDate)),
        dbo.PayrollMonth(DATEADD(month, -1-@numberOfMonthsToCreate+@monthNumber, @toDate)))
    SET @monthNumber = @monthNumber + 1
END

DECLARE @englishFractionMonths    TABLE (dateCalculated datetime)
DECLARE @newEnglishFractionMonths TABLE (dateCalculated datetime)

INSERT @englishFractionMonths
SELECT TOP 1 DATEADD(month, -DATEPART(month, createMonth)%3, createMonth) FROM @levyDecByMonth ORDER BY createMonth

INSERT @englishFractionMonths
SELECT createMonth
FROM (SELECT createMonth FROM @levyDecByMonth EXCEPT SELECT TOP 1 createMonth FROM @levyDecByMonth ORDER BY createMonth) x
WHERE DATEPART(month, createMonth)%3 = 0

INSERT @newEnglishFractionMonths
SELECT DATEFROMPARTS(DATEPART(year, dateCalculated), DATEPART(month, dateCalculated), 7)
FROM @englishFractionMonths
EXCEPT SELECT dateCalculated FROM employer_financial.EnglishFraction WHERE EmpRef = @payeScheme

INSERT employer_financial.EnglishFraction (DateCalculated, Amount, EmpRef, DateCreated)
SELECT dateCalculated, 1.0, @payeScheme, dateCalculated FROM @newEnglishFractionMonths

DECLARE @maxSubmissionId        bigint   = ISNULL((SELECT MAX(SubmissionId) FROM employer_financial.LevyDeclaration), 0)
DECLARE @baselineSubmissionDate datetime = DATEFROMPARTS(YEAR(@toDate), MONTH(@toDate), 18)
DECLARE @baselinePayrollDate    datetime = DATEADD(month, -1, @toDate)

INSERT INTO employer_financial.LevyDeclaration
    (AccountId, EmpRef, LevyDueYTD, LevyAllowanceForYear, SubmissionDate, SubmissionId, PayrollYear, PayrollMonth, CreatedDate, HmrcSubmissionId)
SELECT
    @accountId, @payeScheme, amount, $(SenderLevyAllowance).0000,
    DATEADD(month, monthBeforeToDate, @baselineSubmissionDate),
    @maxSubmissionId + ROW_NUMBER() OVER (ORDER BY (SELECT NULL)),
    dbo.PayrollYear(DATEADD(month,  monthBeforeToDate, @baselinePayrollDate)),
    dbo.PayrollMonth(DATEADD(month, monthBeforeToDate, @baselinePayrollDate)),
    DATEADD(month, monthBeforeToDate, @baselineSubmissionDate),
    @maxSubmissionId + ROW_NUMBER() OVER (ORDER BY (SELECT NULL))
FROM @levyDecByMonth

EXEC employer_financial.ProcessDeclarationsTransactions @accountId, @payeScheme, @toDate, 24
GO

COMMIT TRANSACTION ScenarioTransferSenderLevies
GO

-- ====================== LEVIES FOR RECEIVER ACCOUNT =========================

BEGIN TRANSACTION ScenarioTransferReceiverLevies
GO

DECLARE @accountId              bigint          = $(ReceiverAccountId)
DECLARE @payeScheme             nvarchar(50)    = '$(ReceiverPayeScheme)'
DECLARE @monthlyLevy            decimal(18,4)   = $(ReceiverMonthlyLevy)
DECLARE @toDate                 datetime2       = GETDATE()
DECLARE @numberOfMonthsToCreate int             = $(NumberOfMonths)

DECLARE @levyDecByMonth TABLE (monthBeforeToDate int, amount decimal(18,4), createMonth datetime, payrollYear varchar(5), payrollMonth int)

DECLARE @firstPayrollMonth  datetime   = DATEADD(month, -@numberOfMonthsToCreate+1-1, @toDate)
DECLARE @firstPayrollYear   VARCHAR(5) = dbo.PayrollYear(@firstPayrollMonth)
DECLARE @monthNumber        int        = 1

WHILE @monthNumber <= @numberOfMonthsToCreate
BEGIN
    INSERT INTO @levyDecByMonth (monthBeforeToDate, amount, createMonth, payrollYear, payrollMonth)
    VALUES (
        -@numberOfMonthsToCreate + @monthNumber,
        (CASE
            WHEN dbo.PayrollYear(DATEADD(month, -1-@numberOfMonthsToCreate+@monthNumber, @toDate)) = @firstPayrollYear
                THEN @monthlyLevy * @monthNumber
                ELSE @monthlyLevy * dbo.PayrollMonth(DATEADD(month, -1-@numberOfMonthsToCreate+@monthNumber, @toDate))
        END),
        DATEADD(month, -@numberOfMonthsToCreate+@monthNumber, @toDate),
        dbo.PayrollYear(DATEADD(month,  -1-@numberOfMonthsToCreate+@monthNumber, @toDate)),
        dbo.PayrollMonth(DATEADD(month, -1-@numberOfMonthsToCreate+@monthNumber, @toDate)))
    SET @monthNumber = @monthNumber + 1
END

DECLARE @englishFractionMonths    TABLE (dateCalculated datetime)
DECLARE @newEnglishFractionMonths TABLE (dateCalculated datetime)

INSERT @englishFractionMonths
SELECT TOP 1 DATEADD(month, -DATEPART(month, createMonth)%3, createMonth) FROM @levyDecByMonth ORDER BY createMonth

INSERT @englishFractionMonths
SELECT createMonth
FROM (SELECT createMonth FROM @levyDecByMonth EXCEPT SELECT TOP 1 createMonth FROM @levyDecByMonth ORDER BY createMonth) x
WHERE DATEPART(month, createMonth)%3 = 0

INSERT @newEnglishFractionMonths
SELECT DATEFROMPARTS(DATEPART(year, dateCalculated), DATEPART(month, dateCalculated), 7)
FROM @englishFractionMonths
EXCEPT SELECT dateCalculated FROM employer_financial.EnglishFraction WHERE EmpRef = @payeScheme

INSERT employer_financial.EnglishFraction (DateCalculated, Amount, EmpRef, DateCreated)
SELECT dateCalculated, 1.0, @payeScheme, dateCalculated FROM @newEnglishFractionMonths

DECLARE @maxSubmissionId        bigint   = ISNULL((SELECT MAX(SubmissionId) FROM employer_financial.LevyDeclaration), 0)
DECLARE @baselineSubmissionDate datetime = DATEFROMPARTS(YEAR(@toDate), MONTH(@toDate), 18)
DECLARE @baselinePayrollDate    datetime = DATEADD(month, -1, @toDate)

INSERT INTO employer_financial.LevyDeclaration
    (AccountId, EmpRef, LevyDueYTD, LevyAllowanceForYear, SubmissionDate, SubmissionId, PayrollYear, PayrollMonth, CreatedDate, HmrcSubmissionId)
SELECT
    @accountId, @payeScheme, amount, $(ReceiverLevyAllowance).0000,
    DATEADD(month, monthBeforeToDate, @baselineSubmissionDate),
    @maxSubmissionId + ROW_NUMBER() OVER (ORDER BY (SELECT NULL)),
    dbo.PayrollYear(DATEADD(month,  monthBeforeToDate, @baselinePayrollDate)),
    dbo.PayrollMonth(DATEADD(month, monthBeforeToDate, @baselinePayrollDate)),
    DATEADD(month, monthBeforeToDate, @baselineSubmissionDate),
    @maxSubmissionId + ROW_NUMBER() OVER (ORDER BY (SELECT NULL))
FROM @levyDecByMonth

EXEC employer_financial.ProcessDeclarationsTransactions @accountId, @payeScheme, @toDate, 24
GO

COMMIT TRANSACTION ScenarioTransferReceiverLevies

DROP FUNCTION PayrollYear
GO
DROP FUNCTION PayrollMonth
GO

-- ===================== TRANSFERS: SENDER → RECEIVER =========================

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

CREATE OR ALTER PROCEDURE #createTransferForMonth (
    @senderAccountId   bigint, @senderAccountName   nvarchar(100),
    @receiverAccountId bigint, @receiverAccountName nvarchar(100),
    @createDate datetime, @totalAmount decimal(18,5), @numberOfPayments int
) AS
BEGIN
BEGIN TRANSACTION
    DECLARE @periodEndDate    datetime  = DATEADD(month, -1, @createDate)
    DECLARE @periodEndId      varchar(8) = dbo.PeriodEnd(@periodEndDate)
    DECLARE @courseName       nvarchar(max) = 'Plate Spinning'
    DECLARE @ukprn            bigint = 10001378
    DECLARE @firstApprenticeshipId bigint = ISNULL((SELECT MAX(ApprenticeshipId) FROM employer_financial.Payment), 0) + 1
    DECLARE @n                int = @numberOfPayments
    DECLARE @each             decimal(18,5) = @totalAmount / @numberOfPayments
    DECLARE @apprenticeName   varchar(20)
    DECLARE @uln              bigint
    DECLARE @apprenticeshipId bigint

    EXEC #createPeriodEnd @periodEndDate

    WHILE @n > 0
    BEGIN
        SET @n = @n - 1
        SET @apprenticeName   = CHAR(ASCII('A') + @n) + ' Apprentice'
        SET @uln              = 1000000000 + @n
        SET @apprenticeshipId = @firstApprenticeshipId + @n
        EXEC #createPayment @receiverAccountId, 'CHESTERFIELD COLLEGE', @courseName, 1,
            @apprenticeName, @ukprn, @uln, @apprenticeshipId, /*LevyTransfer*/5, @each, @periodEndDate
    END

    INSERT INTO employer_financial.AccountTransfers
        (SenderAccountId, SenderAccountName, ReceiverAccountId, ReceiverAccountName,
         ApprenticeshipId, CourseName, PeriodEnd, Amount, Type, CreatedDate, RequiredPaymentId)
    VALUES
        (@senderAccountId, @senderAccountName, @receiverAccountId, @receiverAccountName,
         @firstApprenticeshipId, @courseName, @periodEndId, @totalAmount, 'Levy', @createDate, CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier))

    INSERT INTO employer_financial.TransactionLine
        (AccountId, DateCreated, TransactionDate, TransactionType, Amount, PeriodEnd,
         TransferSenderAccountId, TransferSenderAccountName, TransferReceiverAccountId, TransferReceiverAccountName)
    VALUES
        (@senderAccountId,   @createDate, @createDate, 4, -@totalAmount, @periodEndId, @senderAccountId, @senderAccountName, @receiverAccountId, @receiverAccountName),
        (@receiverAccountId, @createDate, @createDate, 4,  @totalAmount, @periodEndId, @senderAccountId, @senderAccountName, @receiverAccountId, @receiverAccountName)

    DELETE employer_financial.TransactionLine WHERE AccountId = @receiverAccountId AND Ukprn = @ukprn AND PeriodEnd = @periodEndId AND TransactionType = 3
    EXEC #processPayments @receiverAccountId, @createDate
COMMIT TRANSACTION
END
GO

DECLARE @senderAccountId     bigint        = $(SenderAccountId)
DECLARE @senderAccountName   nvarchar(100) = 'Transfer Sender Ltd'    -- keep in sync with SenderAccountName above
DECLARE @receiverAccountId   bigint        = $(ReceiverAccountId)
DECLARE @receiverAccountName nvarchar(100) = 'Transfer Receiver Ltd'  -- keep in sync with ReceiverAccountName above
DECLARE @toDate              datetime      = GETDATE()
DECLARE @numberOfMonthsToCreate int        = $(NumberOfMonths)
DECLARE @monthlyTransfer     decimal(18,5) = $(MonthlyTransfer)
DECLARE @paymentsPerMonth    int           = $(PaymentsPerMonth)

DECLARE @months TABLE (monthBeforeToDate int, amount decimal(18,4), paymentsToGenerate int, createMonth datetime)
INSERT INTO @months
SELECT TOP (@numberOfMonthsToCreate)
    -@numberOfMonthsToCreate + ROW_NUMBER() OVER (ORDER BY [object_id]),
    @monthlyTransfer,
    @paymentsPerMonth,
    DATEADD(month, -@numberOfMonthsToCreate + ROW_NUMBER() OVER (ORDER BY [object_id]), @toDate)
FROM sys.all_objects ORDER BY 1

DECLARE @monthBeforeToDate  int = 1
DECLARE @createDate         datetime
DECLARE @amount             decimal(18,4)
DECLARE @paymentsToGenerate int

WHILE (1=1)
BEGIN
    SELECT TOP 1 @monthBeforeToDate = monthBeforeToDate, @createDate = createMonth,
                 @amount = amount, @paymentsToGenerate = paymentsToGenerate
    FROM @months WHERE monthBeforeToDate < @monthBeforeToDate ORDER BY monthBeforeToDate DESC
    IF @@ROWCOUNT = 0 BREAK
    EXEC #createTransferForMonth @senderAccountId, @senderAccountName, @receiverAccountId, @receiverAccountName,
                                 @createDate, @amount, @paymentsToGenerate
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
