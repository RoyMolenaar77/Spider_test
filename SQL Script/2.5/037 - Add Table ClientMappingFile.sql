
-- =============================================
-- Author     : Dashti Salar	 	
-- Create date: 12-08-2013
-- Description: Create Table 'ClientMappingTNT'
-- =============================================

DECLARE @intErrorCode INT

BEGIN TRAN

	--Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'ClientMappingTNT' )
		BEGIN
		
			CREATE TABLE ClientMappingTNT
			(
				 ClientCode NVARCHAR(10) NOT NULL PRIMARY KEY,
				 ClientName NVARCHAR(100) NOT NULL
			)

			PRINT 'Added table ClientMappingTNT'
		END
	ELSE
		BEGIN
			PRINT 'Table ClientMappingTNT already added to the database'
		END
	--End Action 

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while creating table ClientMappingTNT'

	ROLLBACK TRAN
END