DECLARE @intErrorCode INT

BEGIN TRAN

  --Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'MasterGroupMappingUser' )
		
		BEGIN
    
	CREATE TABLE [dbo].[MasterGroupMappingUser](
		[MasterGroupMappingID] [int] NOT NULL,
		[UserID] [int] NOT NULL,
	CONSTRAINT [PK_MasterGroupMappingUser] PRIMARY KEY CLUSTERED 
	(
		[MasterGroupMappingID] ASC,
		[UserID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
	
	
	ALTER TABLE [dbo].[MasterGroupMappingUser]  WITH CHECK ADD  CONSTRAINT [FK__MasterGro__UserI__0C208E03] FOREIGN KEY([UserID])
	REFERENCES [dbo].[User] ([UserID])
	
	ALTER TABLE [dbo].[MasterGroupMappingUser] CHECK CONSTRAINT [FK__MasterGro__UserI__0C208E03]
	
	ALTER TABLE [dbo].[MasterGroupMappingUser]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingUser_MasterGroupMapping] FOREIGN KEY([MasterGroupMappingID])
	REFERENCES [dbo].[MasterGroupMapping] ([MasterGroupMappingID])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[MasterGroupMappingUser] CHECK CONSTRAINT [FK_MasterGroupMappingUser_MasterGroupMapping]
	
      PRINT 'Added table MasterGroupMappingUser'
    END
  ELSE
    BEGIN
      PRINT 'Table MasterGroupMappingUser already added to the database'
    END
  --End Action 

  SELECT @intErrorCode = @@ERROR
  IF (@intErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while creating table MasterGroupMappingUser'

  ROLLBACK TRAN
END