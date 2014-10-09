DECLARE @intErrorCode INT

BEGIN TRAN

	--Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'WehkampMessage' )
		BEGIN
			CREATE TABLE [dbo].[WehkampMessage](
				[MessageID] [int] IDENTITY(1,1) NOT NULL,
				[MessageType] [int] NOT NULL,
				[Filename] [nvarchar](128) NOT NULL,
				[Path] [nvarchar](1024) NULL,
				[Status] [int] NOT NULL,  
				[Received] [datetime] NULL,
				[Sent] [datetime] NULL,
				[LastModified] [datetime] NULL,
				[Attempts] [int] NOT NULL,
			 CONSTRAINT [PK_WehkampMessage] PRIMARY KEY CLUSTERED 
			(
				[MessageID] ASC
			)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY]
			
			ALTER TABLE [dbo].[WehkampMessage] ADD  CONSTRAINT [DF_WehkampMessage_Attempts]  DEFAULT ((0)) FOR [Attempts]
			PRINT 'Added table WehkampMessage'
		END
	ELSE
		BEGIN
			PRINT 'Table WehkampMessage already added to the database'
		END
	--End Action 

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while creating table WehkampMessage'

	ROLLBACK TRAN
END