DECLARE @intErrorCode INT

BEGIN TRAN

  --Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'UserLanguage' )
		
		BEGIN	

	CREATE TABLE [dbo].[UserLanguage](
		[UserID] [int] NOT NULL,
		[LanguageID] [int] NOT NULL,
	CONSTRAINT [PK_UserLanguage] PRIMARY KEY CLUSTERED 
	(
		[UserID] ASC,
		[LanguageID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]	
	
      PRINT 'Added table UserLanguage'
    END
  ELSE
    BEGIN
      PRINT 'Table UserLanguage already added to the database'
    END
  --End Action 

  SELECT @intErrorCode = @@ERROR
  IF (@intErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while creating table UserLanguage'

  ROLLBACK TRAN
END