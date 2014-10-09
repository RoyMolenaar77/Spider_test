DECLARE @intErrorCode INT

BEGIN TRAN

  --Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'UserPlugin' )
		
		BEGIN	

	    CREATE TABLE [dbo].[UserPlugin](
	    [UserID] [int] NOT NULL,
	    [PluginID] [int] NOT NULL,
	    [TypeID] [int] NOT NULL,
	    [SubscriptionTime] [datetime] NOT NULL,
     CONSTRAINT [PK_UserPlugin] PRIMARY KEY CLUSTERED 
    (
	    [UserID] ASC,
	    [PluginID] ASC,
	    [TypeID] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]

      ALTER TABLE [dbo].[UserPlugin] ADD  CONSTRAINT [DF__UserPlugin__SubscriptionTime]  DEFAULT (getdate()) FOR [SubscriptionTime]

      ALTER TABLE [dbo].[UserPlugin]  WITH CHECK ADD  CONSTRAINT [FK_UserPlugin_EventType] FOREIGN KEY([TypeID])
      REFERENCES [dbo].[EventType] ([TypeID])

      ALTER TABLE [dbo].[UserPlugin]  WITH CHECK ADD  CONSTRAINT [FK_UserPlugin_Plugin] FOREIGN KEY([PluginID])
      REFERENCES [dbo].[Plugin] ([PluginID])
      
      ALTER TABLE [dbo].[UserPlugin]  WITH CHECK ADD  CONSTRAINT [FK_UserPlugin_User] FOREIGN KEY([UserID])
      REFERENCES [dbo].[User] ([UserID])

    END
  ELSE
    BEGIN
      PRINT 'Table UserPlugin already added to the database'
    END
  --End Action 

  SELECT @intErrorCode = @@ERROR
  IF (@intErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while creating table UserPlugin'

  ROLLBACK TRAN
END