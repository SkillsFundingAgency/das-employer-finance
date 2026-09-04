# Testing the expire-funds API

How to set up a TEST levy employer with `das-hmrc-mock-api`, plant a declaration that expiry can see, and call `POST /api/accounts/{accountId}/expire-funds`.

These steps were written for TEST in September 2026. Adjust calendar months if you run this later.

## What the API returns

| Field | Meaning |
| --- | --- |
| `fundsExpired` | At least one month has a **non-zero** expiry amount |
| `longTermExpiredFundsCount` | Number of type-5 rows written this run, **including a £0 current-month placeholder** |
| `shortTermExpiredFundsCount` | Number of type-6 rows written this run |

Type `5` on `TransactionLine` is `ExpiredFund`. That is the transaction type of the row, not a "funds actually expired" flag. A type-5 row with `Amount = 0` still means `fundsExpired: false`.

Typical first-run response when **no money** expired:

```json
{
  "accountId": 12345,
  "correlationId": "...",
  "fundsExpired": false,
  "longTermExpiredFundsCount": 1,
  "shortTermExpiredFundsCount": 0
}
```

That `1` is a £0 placeholder for the current calendar month, not expired levy.

## Why a new mock levy account is not enough

1. Gateway id `LE_{n}_{amount}` creates **n** months of HMRC declarations.
2. Finance import sets `LevyDeclaration.CreatedDate` to **now**.
3. `GetLevyFundsIn` groups type-1 `TransactionLine` rows by **`DateCreated`**, not payroll date. A new account therefore has all funds in the **current calendar month**.
4. TEST expiry config (finance jobs / API):
   - Long-term: **24 months** (`FundsExpiryPeriod`)
   - Short-term: **12 months** (`NewFundsExpiryPeriod`)
   - Policy change: **2026-08-01** (`FundsExpiryPolicyChangeDate`)
5. Levy in the current month (after August 2026) is short-term and expires 12 months later. In September 2026 that is September 2027.
6. Calling expire-funds then writes a £0 type-5 row for the current month and returns `fundsExpired: false`.

The script `das-hmrc-mock-api/scripts/AddHistoricLevyDeclaration.sql` is also not enough as written. It inserts a `LevyDeclaration` more than 24 months old, then `ProcessDeclarationsTransactions` skips it because it is outside the in-date window. You get a declaration and **no** type-1 `TransactionLine`, so expiry still sees nothing to expire.

To get `fundsExpired: true` you must create a type-1 line whose `DateCreated` is **24 months ago** (long-term, and before the policy change date).

Short-term expiry cannot be forced in TEST until August 2027 unless you change `NewFundsExpiryPeriod` in config.

## Prerequisites

- TEST employer accounts: [https://test-accounts.apprenticeships.education.gov.uk/](https://test-accounts.apprenticeships.education.gov.uk/)
- TEST finance API: `https://das-test-finapi-as.azurewebsites.net/`
- Azure AD app id / resource: `https://citizenazuresfabisgov.onmicrosoft.com/das-test-finapi-as-ar`
- Role: `ReadAllEmployerAccountBalances`
- Finance database access (for example `das-test2-eas-fin-db`)

---

## 1. Create a levy employer via the HMRC mock

1. Create a new employer account in TEST (or use a throwaway account).
2. Add a PAYE scheme. That redirects to the mock gateway.
3. Sign in with a **new** convention id (first time only):

   ```text
   User ID:  LE_12_1000
   Password: anything
   ```

   Pattern: `LE_{months}_{monthlyAmount}`

   | Part | Meaning |
   | --- | --- |
   | `LE` | Levy account: mock creates declarations. Use `NL` for non-levy (no declarations). |
   | `12` | Months of history. Keep this modest. `LE_36_9999` is messy because of YTD and the 12-month in-date window. |
   | `1000` | Monthly levy amount in pounds. |

   First sign-in stores `LE_12_1000` plus ticks as the real gateway id. Use that full id if you sign in again.

4. Complete PAYE add and wait for levy import (finance jobs).

5. Confirm in the finance database:

```sql
DECLARE @AccountId BIGINT = /* your new id */;

SELECT AccountId, EmpRef, PayrollYear, PayrollMonth, LevyDueYTD, CreatedDate, SubmissionDate
FROM employer_financial.LevyDeclaration
WHERE AccountId = @AccountId
ORDER BY PayrollYear, PayrollMonth;

SELECT Id, TransactionDate, DateCreated, TransactionType, Amount, EmpRef
FROM employer_financial.TransactionLine
WHERE AccountId = @AccountId AND TransactionType = 1
ORDER BY DateCreated, TransactionDate;

EXEC employer_financial.GetLevyFundsIn @AccountId;
```

Expect declarations and type-1 lines. `GetLevyFundsIn` will usually show **one bucket for the current year and month**, because import used `CreatedDate = now`. That is expected.

Do **not** call expire-funds yet.

---

## 2. Plant a declaration that expiry can see

You need a type-1 line whose `DateCreated` is 24 months ago. In September 2026 that is September 2024:

- Calendar period September 2024 is before the 2026-08-01 policy change, so it is **long-term**.
- September 2024 + 24 months = September 2026, so it expires **this month**.

Use the script below (not the `> 24` months historic script). Important parameters:

- `@MonthsAgo = 24`
- `@ExpiryPeriod = 36` so `IsInDateLevy` allows a `TransactionLine`
- `CreatedDate` must be historical. Expiry uses `TransactionLine.DateCreated`, which is copied from the declaration `CreatedDate`.

```sql
-- Run against the Employer Finance database (for example das-test2-eas-fin-db).
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @AccountId BIGINT = 0; -- replace with the TEST account id
DECLARE @EmpRef NVARCHAR(50) = NULL; -- leave NULL to use the account's latest PAYE scheme
DECLARE @MonthsAgo INT = 24;         -- 24 months ago expires this month (long-term)
DECLARE @ExpiryPeriod INT = 36;      -- must be > MonthsAgo so a TransactionLine is created
DECLARE @CurrentDate DATETIME = GETDATE();

IF (@AccountId <= 0)
BEGIN
    RAISERROR('Set @AccountId to the TEST employer account id.', 16, 1);
    RETURN;
END;

IF NOT EXISTS (SELECT 1 FROM employer_financial.LevyDeclaration WHERE AccountId = @AccountId)
BEGIN
    RAISERROR('No levy declarations found. Create the account and import levy first.', 16, 1);
    RETURN;
END;

IF (@EmpRef IS NULL)
BEGIN
    SELECT TOP (1) @EmpRef = EmpRef
    FROM employer_financial.LevyDeclaration
    WHERE AccountId = @AccountId
    ORDER BY SubmissionDate DESC, Id DESC;
END;

-- English fraction is required or the transaction amount can be 0
IF NOT EXISTS (
    SELECT 1 FROM employer_financial.EnglishFraction WHERE EmpRef = @EmpRef
)
BEGIN
    INSERT employer_financial.EnglishFraction (DateCalculated, Amount, EmpRef, DateCreated)
    VALUES (DATEFROMPARTS(2024, 4, 7), 1.0, @EmpRef, DATEFROMPARTS(2024, 4, 7));
END;

DECLARE @MonthlyAmount DECIMAL(18, 4);

SELECT TOP (1) @MonthlyAmount = CASE
        WHEN ld.PayrollMonth = 1 THEN ld.LevyDueYTD
        ELSE ld.LevyDueYTD - ISNULL((
            SELECT TOP (1) prev.LevyDueYTD
            FROM employer_financial.LevyDeclaration prev
            WHERE prev.AccountId = ld.AccountId
              AND prev.EmpRef = ld.EmpRef
              AND prev.PayrollYear = ld.PayrollYear
              AND prev.PayrollMonth < ld.PayrollMonth
              AND prev.LevyDueYTD IS NOT NULL
            ORDER BY prev.PayrollMonth DESC
        ), 0)
    END
FROM employer_financial.LevyDeclaration ld
WHERE ld.AccountId = @AccountId
  AND ld.EmpRef = @EmpRef
  AND ld.LevyDueYTD IS NOT NULL
ORDER BY ld.PayrollYear DESC, ld.PayrollMonth DESC;

IF (@MonthlyAmount IS NULL OR @MonthlyAmount = 0)
BEGIN
    RAISERROR('Could not infer a monthly levy amount. Check LevyDeclaration for this account.', 16, 1);
    RETURN;
END;

DECLARE @TargetDate DATE = DATEADD(MONTH, -@MonthsAgo, CAST(@CurrentDate AS DATE));
DECLARE @CalMonth INT = DATEPART(MONTH, @TargetDate);
DECLARE @CalYear INT = DATEPART(YEAR, @TargetDate);
DECLARE @PayrollMonth INT = CASE WHEN @CalMonth >= 4 THEN @CalMonth - 3 ELSE @CalMonth + 9 END;
DECLARE @PayrollStartYear INT = CASE WHEN @CalMonth < 4 THEN @CalYear - 1 ELSE @CalYear END;
DECLARE @PayrollYear NVARCHAR(5) = RIGHT('0' + CAST(@PayrollStartYear % 100 AS VARCHAR(2)), 2)
    + '-' + RIGHT('0' + CAST((@PayrollStartYear + 1) % 100 AS VARCHAR(2)), 2);

-- If that period already exists, walk back until a free month is found
WHILE EXISTS (
    SELECT 1
    FROM employer_financial.LevyDeclaration
    WHERE AccountId = @AccountId
      AND EmpRef = @EmpRef
      AND PayrollYear = @PayrollYear
      AND PayrollMonth = @PayrollMonth
)
BEGIN
    SET @TargetDate = DATEADD(MONTH, -1, @TargetDate);
    SET @CalMonth = DATEPART(MONTH, @TargetDate);
    SET @CalYear = DATEPART(YEAR, @TargetDate);
    SET @PayrollMonth = CASE WHEN @CalMonth >= 4 THEN @CalMonth - 3 ELSE @CalMonth + 9 END;
    SET @PayrollStartYear = CASE WHEN @CalMonth < 4 THEN @CalYear - 1 ELSE @CalYear END;
    SET @PayrollYear = RIGHT('0' + CAST(@PayrollStartYear % 100 AS VARCHAR(2)), 2)
        + '-' + RIGHT('0' + CAST((@PayrollStartYear + 1) % 100 AS VARCHAR(2)), 2);
END;

DECLARE @PreviousYtd DECIMAL(18, 4);

SELECT TOP (1) @PreviousYtd = LevyDueYTD
FROM employer_financial.LevyDeclaration
WHERE AccountId = @AccountId
  AND EmpRef = @EmpRef
  AND PayrollYear = @PayrollYear
  AND PayrollMonth < @PayrollMonth
  AND LevyDueYTD IS NOT NULL
ORDER BY PayrollMonth DESC;

DECLARE @LevyDueYtd DECIMAL(18, 4) = CASE
    WHEN @PayrollMonth = 1 OR @PreviousYtd IS NULL THEN @MonthlyAmount
    ELSE @PreviousYtd + @MonthlyAmount
END;

DECLARE @LevyAllowance DECIMAL(18, 4) = ISNULL((
    SELECT TOP (1) LevyAllowanceForYear
    FROM employer_financial.LevyDeclaration
    WHERE AccountId = @AccountId AND EmpRef = @EmpRef AND LevyAllowanceForYear IS NOT NULL
    ORDER BY SubmissionDate DESC
), 15000);

DECLARE @PayrollCalendarMonth INT = CASE WHEN @PayrollMonth >= 10 THEN @PayrollMonth - 9 ELSE @PayrollMonth + 3 END;
DECLARE @PayrollCalendarYear INT = CASE
    WHEN @PayrollMonth >= 10 THEN 2000 + CAST(RIGHT(@PayrollYear, 2) AS INT)
    ELSE 2000 + CAST(LEFT(@PayrollYear, 2) AS INT)
END;

-- CreatedDate must be historical. Expiry uses TransactionLine.DateCreated from this.
DECLARE @CreatedDate DATETIME = DATEFROMPARTS(@PayrollCalendarYear, @PayrollCalendarMonth, 20);
DECLARE @SubmissionDate DATETIME = DATEADD(MONTH, 1, DATEFROMPARTS(@PayrollCalendarYear, @PayrollCalendarMonth, 18));
DECLARE @SubmissionId BIGINT = ISNULL((SELECT MAX(SubmissionId) FROM employer_financial.LevyDeclaration), 0) + 1;
DECLARE @DeclarationCreated INT;

EXEC employer_financial.CreateDeclaration
    @LevyDueYtd = @LevyDueYtd,
    @EmpRef = @EmpRef,
    @SubmissionDate = @SubmissionDate,
    @SubmissionId = @SubmissionId,
    @HmrcSubmissionId = @SubmissionId,
    @AccountId = @AccountId,
    @LevyAllowanceForYear = @LevyAllowance,
    @PayrollYear = @PayrollYear,
    @PayrollMonth = @PayrollMonth,
    @CreatedDate = @CreatedDate,
    @DateCeased = NULL,
    @InactiveFrom = NULL,
    @InactiveTo = NULL,
    @EndOfYearAdjustment = 0,
    @EndOfYearAdjustmentAmount = 0,
    @NoPaymentForPeriod = 0,
    @DeclarationCreated = @DeclarationCreated OUTPUT;

IF (@DeclarationCreated <> 1)
BEGIN
    RAISERROR('CreateDeclaration did not insert a row (duplicate SubmissionId?).', 16, 1);
    RETURN;
END;

DECLARE @TransactionsCreated INT = 0;

EXEC employer_financial.ProcessDeclarationsTransactions
    @AccountId = @AccountId,
    @EmpRef = @EmpRef,
    @currentDate = @CurrentDate,
    @expiryPeriod = @ExpiryPeriod,
    @TransactionsCreated = @TransactionsCreated OUTPUT;

SELECT
    @AccountId AS AccountId,
    @EmpRef AS EmpRef,
    @MonthlyAmount AS MonthlyAmountCopied,
    @PayrollYear AS PayrollYear,
    @PayrollMonth AS PayrollMonth,
    @LevyDueYtd AS LevyDueYTD,
    @SubmissionId AS SubmissionId,
    @CreatedDate AS CreatedDate,
    @DeclarationCreated AS DeclarationCreated,
    @TransactionsCreated AS TransactionsCreated;

EXEC employer_financial.GetLevyFundsIn @AccountId;
```

Success looks like:

- `DeclarationCreated = 1`
- `TransactionsCreated >= 1`
- `GetLevyFundsIn` showing a **2024 / 9** (or nearby) row with a non-zero `FundsIn`

If `TransactionsCreated = 0`, the declaration was stored but still treated as out of date. Increase `@ExpiryPeriod` or check `IsInDateLevy`.

### Faster alternative

If the account already has type-1 lines, backdate one line instead of inserting:

```sql
UPDATE TOP (1) employer_financial.TransactionLine
SET DateCreated = '2024-09-20'
WHERE AccountId = /* account id */
  AND TransactionType = 1
  AND Amount > 0;
```

Same effect for expiry. Slightly less realistic as a "new declaration".

---

## 3. If you already called expire-funds on this account

`CreateExpiredFunds` is a plain insert. There is a unique index on `(AccountId, TransactionType, TransactionDate)` for type 5.

If a £0 type-5 row already exists for the current month (for example `2026-09-01`), a second run that expires into that same month will fail. Delete the placeholder first:

```sql
DELETE FROM employer_financial.TransactionLine
WHERE AccountId = /* account id */
  AND TransactionType = 5
  AND TransactionDate = '2026-09-01';
```

On a **new** account, skip expire-funds until the historic funds-in row exists.

---

## 4. Call expire-funds

```http
POST https://das-test-finapi-as.azurewebsites.net/api/accounts/{accountId}/expire-funds
Authorization: Bearer {token}
Content-Type: application/json

{
  "correlationId": "11111111-1111-1111-1111-111111111111"
}
```

`correlationId` is required and must be non-blank.

### Expected response when unused September 2024 funds-in exist

```json
{
  "accountId": 12345,
  "correlationId": "11111111-1111-1111-1111-111111111111",
  "fundsExpired": true,
  "longTermExpiredFundsCount": 1,
  "shortTermExpiredFundsCount": 0
}
```

`longTermExpiredFundsCount` can be `2` if the expired month is **not** the current month (real expiry row plus the current-month £0 placeholder).

### Confirm in the database

```sql
SELECT TransactionDate, DateCreated, TransactionType, Amount
FROM employer_financial.TransactionLine
WHERE AccountId = /* account id */
  AND TransactionType IN (5, 6)
ORDER BY TransactionType, TransactionDate;
```

You want a type-5 row with **negative** `Amount` (stored as `-FundsIn`). Amount `0` still means `fundsExpired: false`.

---

## Suggested run order

1. New TEST account, add PAYE via `LE_12_1000`.
2. Confirm declarations and type-1 lines.
3. Run the SQL in section 2 (or backdate `DateCreated`).
4. Confirm `GetLevyFundsIn` has a 2024 bucket with non-zero `FundsIn`.
5. POST expire-funds once.
6. Confirm `fundsExpired: true` and a non-zero type-5 amount.

## Related code

- API: `POST api/accounts/{accountId}/expire-funds` in `SFA.DAS.EmployerFinance.Api`
- Handler: `SFA.DAS.EmployerFinance/Commands/ExpireAccountFunds/ExpireAccountFundsCommandHandler.cs`
- Funds in: `[employer_financial].[GetLevyFundsIn]` (groups type-1 lines by `DateCreated`)
- Mock gateway convention: `das-hmrc-mock-api` `HomeController` (`LE_{n}_{amount}` / `NL_{n}_{amount}`)
