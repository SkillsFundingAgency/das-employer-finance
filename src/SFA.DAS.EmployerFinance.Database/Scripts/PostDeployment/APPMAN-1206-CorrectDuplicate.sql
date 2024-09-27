BEGIN TRY
BEGIN TRANSACTION
	
	SELECT EmpRef, DateCalculated, Amount, COUNT(*) AS NumOfRepeats
	INTO #TempNewRecords
	from [employer_financial].EnglishFraction ef
		GROUP BY EmpRef, DateCalculated, Amount 
		HAVING COUNT(*) > 2

	--SELECT SUM(NumOfRepeats)
	--FROM
	--(SELECT EmpRef, DateCalculated, Amount, COUNT(*) AS NumOfRepeats
	--from [employer_financial].EnglishFraction ef
	--	GROUP BY EmpRef, DateCalculated, Amount 
	--	HAVING COUNT(*) > 2) AS Matching

	SELECT Id, EmpRef, DateCalculated, Amount 
	INTO #TempExistingRecords
	from [employer_financial].EnglishFraction WHERE EmpRef + CAST(DateCalculated as nvarchar(20)) IN 
	(SELECT EmpRef+ CAST(DateCalculated as nvarchar(20))
	FROM (
	SELECT EmpRef, DateCalculated, Amount, COUNT(*) AS NumOfRepeats
		from [employer_financial].EnglishFraction ef
		GROUP BY EmpRef, DateCalculated, Amount 
		HAVING COUNT(*) > 2) AS SubQ)

	--PRINT 'Expected Rows deleted should be '  
	--SELECT COUNT(*) FROM #TempExistingRecords

	PRINT 'Deleting all duplicates'	
	DELETE [employer_financial].EnglishFraction WHERE Id in (SELECT Id FROM #TempExistingRecords)

	--PRINT 'Expected Rows inserted should be '
	--SELECT COUNT(*) FROM #TempNewRecords

	PRINT 'Inserted new rows '
	INSERT INTO [employer_financial].EnglishFraction (EmpRef, DateCalculated, Amount, DateCreated)
	SELECT EmpRef, DateCalculated, Amount, GETDATE()
	FROM #TempNewRecords 

	
END TRY
BEGIN CATCH
	DECLARE @ErrorMsg nvarchar(max)
	DECLARE @ErrorSeverity INT;  
	DECLARE @ErrorState INT;  

	SET @ErrorMsg = ERROR_NUMBER() + ERROR_LINE() + ERROR_MESSAGE()
	SET @ErrorSeverity = ERROR_SEVERITY()
	SET @ErrorState = ERROR_STATE()

	IF @@TRANCOUNT > 0
		ROLLBACK TRANSACTION 
	
	RAISERROR(@ErrorMsg, @ErrorSeverity, @ErrorState)
END CATCH

IF @@TRANCOUNT > 0
	COMMIT TRANSACTION