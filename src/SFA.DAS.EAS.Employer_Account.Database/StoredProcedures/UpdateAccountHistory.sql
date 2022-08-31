﻿CREATE PROCEDURE [employer_account].[UpdateAccountHistory]
	@AccountId BIGINT,
	@PayeRef VARCHAR(20),
	@RemovedDate DATETIME
AS
	UPDATE [employer_account].[AccountHistory] SET RemovedDate = @RemovedDate 
	WHERE AccountId = @AccountId AND PayeRef = @PayeRef AND RemovedDate IS NULL

