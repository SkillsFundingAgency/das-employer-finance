IF OBJECT_ID('[employer_financial].[PaymentMetaDataStaging]', 'U') IS NOT NULL
BEGIN
    ;WITH DuplicateRows AS
    (
        SELECT
            Id,
            PaymentId,
            ROW_NUMBER() OVER (PARTITION BY PaymentId ORDER BY Id DESC) AS RowNumber
        FROM [employer_financial].[PaymentMetaDataStaging]
    )
    DELETE target
    FROM [employer_financial].[PaymentMetaDataStaging] AS target
    INNER JOIN DuplicateRows AS duplicateRow
        ON duplicateRow.Id = target.Id
    WHERE duplicateRow.RowNumber > 1;

    IF OBJECT_ID('[employer_financial].[PaymentStaging]', 'U') IS NOT NULL
    BEGIN
        DELETE target
        FROM [employer_financial].[PaymentMetaDataStaging] AS target
        LEFT JOIN [employer_financial].[PaymentStaging] AS payment
            ON payment.PaymentId = target.PaymentId
        WHERE payment.PaymentId IS NULL;
    END
END
