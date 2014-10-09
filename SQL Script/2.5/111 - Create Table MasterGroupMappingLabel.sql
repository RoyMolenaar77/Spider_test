DECLARE @intErrorCode INT

BEGIN TRAN

  --Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'MasterGroupMappingLabel' )
		
		BEGIN
		
		CREATE TABLE [dbo].[MasterGroupMappingLabel](
			[MasterGroupMappingLabelID] [int] IDENTITY(1,1) NOT NULL,
			[MasterGroupMappingID] [int] NOT NULL,
			[Label] [nvarchar](200) NOT NULL,
			[SearchEngine] [bit] NULL,
			[LanguageID] [int] NOT NULL,
		CONSTRAINT [PK__MasterGr__98F87EA315A9F83D] PRIMARY KEY CLUSTERED 
		(
			[MasterGroupMappingLabelID] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) ON [PRIMARY]
					
		ALTER TABLE [dbo].[MasterGroupMappingLabel]  WITH CHECK ADD  CONSTRAINT [FK__MasterGro__Maste__179240AF] FOREIGN KEY([MasterGroupMappingID])
		REFERENCES [dbo].[MasterGroupMapping] ([MasterGroupMappingID])
				
		ALTER TABLE [dbo].[MasterGroupMappingLabel] CHECK CONSTRAINT [FK__MasterGro__Maste__179240AF]
			
      PRINT 'Added table MasterGroupMappingLabel'
    END
  ELSE
    BEGIN
      PRINT 'Table MasterGroupMappingLabel already added to the database'
    END
  --End Action 

  SELECT @intErrorCode = @@ERROR
  IF (@intErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while creating table MasterGroupMappingLabel'

  ROLLBACK TRAN
END