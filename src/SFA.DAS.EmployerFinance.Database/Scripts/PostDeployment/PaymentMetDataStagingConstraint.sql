IF EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_PaymentMetaDataStaging_PaymentStaging'
)
BEGIN
    ALTER TABLE [employer_financial].[PaymentMetaDataStaging]
    DROP CONSTRAINT [FK_PaymentMetaDataStaging_PaymentStaging];
END

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE name = 'PaymentMetaDataId'
    AND object_id = OBJECT_ID('[employer_financial].[PaymentStaging]')
)
BEGIN
    ALTER TABLE [employer_financial].[PaymentStaging]
    DROP COLUMN [PaymentMetaDataId];
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_PaymentMetaDataStaging_PaymentStaging'
)
BEGIN
    ALTER TABLE [employer_financial].[PaymentMetaDataStaging]
    ADD CONSTRAINT [FK_PaymentMetaDataStaging_PaymentStaging]
    FOREIGN KEY ([PaymentId])
    REFERENCES [employer_financial].[PaymentStaging] ([PaymentId]);
END