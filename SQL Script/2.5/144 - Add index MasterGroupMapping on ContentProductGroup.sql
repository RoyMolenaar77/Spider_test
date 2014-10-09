DECLARE @intErrorCode INT

BEGIN TRAN

IF (select count(*) from ContentProductGroup WHERE MasterGroupMappingID is null) = 0
BEGIN

--Begin Action
IF NOT EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE NAME = 'Index'
			AND object_id = OBJECT_ID('ContentProductGroup')
		) 
BEGIN
	CREATE UNIQUE NONCLUSTERED INDEX [Index] ON [dbo].[ContentProductGroup] (
		[ConnectorID] ASC
		,[ProductID] ASC
		,[MasterGroupMappingID] ASC
		,[IsCustom] ASC
		)
		WITH (
				PAD_INDEX = OFF
				,STATISTICS_NORECOMPUTE = OFF
				,SORT_IN_TEMPDB = OFF
				,IGNORE_DUP_KEY = OFF
				,DROP_EXISTING = OFF
				,ONLINE = OFF
				,ALLOW_ROW_LOCKS = ON
				,ALLOW_PAGE_LOCKS = ON
				) ON [PRIMARY]

	PRINT 'Created index on Table ContentProductGroup'
END
ELSE
BEGIN
	PRINT 'Index [Index] already created on table ContentProductGroup'
END
END
ELSE
BEGIN 
 PRINT 'Content Product Group still contains records for the product group mapping, please remove these first'
 END

SELECT @intErrorCode = @@ERROR

IF (@intErrorCode <> 0)
	GOTO PROBLEM

COMMIT TRAN

PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while creating index [Index] on table ContentProductGroup'

	ROLLBACK TRAN
END
