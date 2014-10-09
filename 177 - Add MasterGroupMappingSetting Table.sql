DECLARE @intErrorCode INT

BEGIN TRAN

  --Begin Action
	IF 
  ( 
    EXISTS 
      ( SELECT * FROM sys.tables WHERE NAME = 'MasterGroupMappingSetting' ) 
      AND NOT EXISTS ( SELECT * FROM MasterGroupMappingSetting )
  ) 
  OR 
  ( 
    NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'MasterGroupMappingSetting' ) 
  )
		
		BEGIN
		
    IF EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'MasterGroupMappingSetting' ) AND NOT EXISTS ( SELECT * FROM MasterGroupMappingSetting )
    BEGIN
      DROP TABLE MasterGroupMappingSetting
    END

	CREATE TABLE [dbo].[MasterGroupMappingSetting](
	[MasterGroupMappingSettingID] [int] IDENTITY(1,1) NOT NULL,
	[Group] [nvarchar](50) NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](250) NOT NULL,
  CONSTRAINT [PK_MasterGroupMappingSetting] PRIMARY KEY CLUSTERED 
  (
  	[MasterGroupMappingSettingID] ASC
  )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
  ) ON [PRIMARY]
		
      PRINT 'Added table MasterGroupMappingSetting'
    END
  ELSE
    BEGIN
      PRINT 'Table MasterGroupMappingSetting already added to the database'
    END
  --End Action 

  SELECT @intErrorCode = @@ERROR
  IF (@intErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while creating table MasterGroupMappingSetting'

  ROLLBACK TRAN
END