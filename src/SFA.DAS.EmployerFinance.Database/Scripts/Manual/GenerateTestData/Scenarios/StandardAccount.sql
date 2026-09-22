-- =============================================================================
-- SCENARIO: Standard Account
--
-- A fully established employer levy account with a consistent 25-month history
-- of levy declarations and matching training payments.
--
-- Requires SQLCMD mode: Query > SQLCMD Mode in SSMS, or natively in Azure Data Studio
-- =============================================================================

-- ===================== CONFIGURATION - CHANGE THESE ========================
:setvar AccountId         100
:setvar PayeScheme        "100/SA00001"
:setvar AccountName       "Standard Account Ltd"
:setvar MonthlyLevy       1000
:setvar LevyAllowance     1500
:setvar NumberOfMonths    25
:setvar MonthlyPayments   500
:setvar PaymentsPerMonth  3
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

-- ================================= LEVIES ===================================

BEGIN TRANSACTION ScenarioStandardAccountLevies
GO

DECLARE @accountId              bigint          = $(AccountId)
DECLARE @payeScheme             nvarchar(50)    = '$(PayeScheme)'
DECLARE @monthlyLevy            decimal(18,4)   = $(MonthlyLevy)
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
    @accountId, @payeScheme, amount, $(LevyAllowance).0000,
    DATEADD(month, monthBeforeToDate, @baselineSubmissionDate),
    @maxSubmissionId + ROW_NUMBER() OVER (ORDER BY (SELECT NULL)),
    dbo.PayrollYear(DATEADD(month,  monthBeforeToDate, @baselinePayrollDate)),
    dbo.PayrollMonth(DATEADD(month, monthBeforeToDate, @baselinePayrollDate)),
    DATEADD(month, monthBeforeToDate, @baselineSubmissionDate),
    @maxSubmissionId + ROW_NUMBER() OVER (ORDER BY (SELECT NULL))
FROM @levyDecByMonth

EXEC employer_financial.ProcessDeclarationsTransactions @accountId, @payeScheme, @toDate, 24
GO

COMMIT TRANSACTION ScenarioStandardAccountLevies

DROP FUNCTION PayrollYear
GO
DROP FUNCTION PayrollMonth
GO

-- =============================== PAYMENTS ===================================

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

CREATE OR ALTER PROCEDURE #createPaymentsForMonth (
    @accountId bigint, @createDate datetime, @totalAmount decimal(18,5), @numberOfPayments int
) AS
BEGIN
BEGIN TRANSACTION
    DECLARE @periodEndDate datetime  = DATEADD(month, -1, @createDate)
    DECLARE @periodEndId   varchar(8) = dbo.PeriodEnd(@periodEndDate)
    DECLARE @ukprn         bigint = 10001378
    DECLARE @n             int = @numberOfPayments
    DECLARE @each          decimal(18,5) = @totalAmount / @numberOfPayments
    EXEC #createPeriodEnd @periodEndDate
    WHILE @n > 0
    BEGIN
        SET @n = @n - 1
        EXEC #createPayment @accountId, 'CHESTERFIELD COLLEGE', 'Accounting', 1,
            CHAR(ASCII('A') + @n) + ' Apprentice', @ukprn, 1000000000 + @n, 1000 + @n, /*Levy*/1, @each, @periodEndDate
    END
    DELETE employer_financial.TransactionLine WHERE AccountId = @accountId AND Ukprn = @ukprn AND PeriodEnd = @periodEndId AND TransactionType = 3
    EXEC #processPayments @accountId, @createDate
COMMIT TRANSACTION
END
GO

DECLARE @accountId              bigint          = $(AccountId)
DECLARE @toDate                 datetime        = GETDATE()
DECLARE @numberOfMonthsToCreate int             = $(NumberOfMonths)
DECLARE @monthlyTotalPayments   decimal(18,5)   = $(MonthlyPayments)
DECLARE @paymentsPerMonth       int             = $(PaymentsPerMonth)

DECLARE @paymentsByMonth TABLE (monthBeforeToDate int, amount decimal(18,4), paymentsToGenerate int, createMonth datetime)
INSERT INTO @paymentsByMonth
SELECT TOP (@numberOfMonthsToCreate)
    -@numberOfMonthsToCreate + ROW_NUMBER() OVER (ORDER BY [object_id]),
    @monthlyTotalPayments,
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
    FROM @paymentsByMonth WHERE monthBeforeToDate < @monthBeforeToDate ORDER BY monthBeforeToDate DESC
    IF @@ROWCOUNT = 0 BREAK
    EXEC #createPaymentsForMonth @accountId, @createDate, @amount, @paymentsToGenerate
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
