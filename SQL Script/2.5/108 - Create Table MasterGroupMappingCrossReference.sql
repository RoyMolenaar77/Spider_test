DECLARE @intErrorCode INT

BEGIN TRAN

  --Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'MasterGroupMappingCrossReference' )
		
		BEGIN
		
		CREATE TABLE [dbo].[MasterGroupMappingCrossReference](
			[MasterGroupMappingID] [int] NOT NULL,
			[CrossReferenceID] [int] NOT NULL,
		CONSTRAINT [PK_MasterGroupMappingCrossReference] PRIMARY KEY CLUSTERED 
		(
			[MasterGroupMappingID] ASC,
			[CrossReferenceID] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) ON [PRIMARY]
		
		ALTER TABLE [dbo].[MasterGroupMappingCrossReference]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingCrossReference_MasterGroupMapping] FOREIGN KEY([MasterGroupMappingID])
		REFERENCES [dbo].[MasterGroupMapping] ([MasterGroupMappingID])
		ON DELETE CASCADE
		
		ALTER TABLE [dbo].[MasterGroupMappingCrossReference] CHECK CONSTRAINT [FK_MasterGroupMappingCrossReference_MasterGroupMapping]
		
		ALTER TABLE [dbo].[MasterGroupMappingCrossReference]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingCrossReference_MasterGroupMapping1] FOREIGN KEY([CrossReferenceID])
		REFERENCES [dbo].[MasterGroupMapping] ([MasterGroupMappingID])
		
		ALTER TABLE [dbo].[MasterGroupMappingCrossReference] CHECK CONSTRAINT [FK_MasterGroupMappingCrossReference_MasterGroupMapping1]
	
      PRINT 'Added table MasterGroupMappingCrossReference'
    END
  ELSE
    BEGIN
      PRINT 'Table MasterGroupMappingCrossReference already added to the database'
    END
  --End Action 

  SELECT @intErrorCode = @@ERROR
  IF (@intErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while creating table MasterGroupMappingCrossReference'

  ROLLBACK TRAN
END