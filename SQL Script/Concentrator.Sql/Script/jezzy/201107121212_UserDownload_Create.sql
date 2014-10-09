USE [Concentrator_dev_entity]
GO

/****** Object:  Table [dbo].[UserDownload]    Script Date: 07/12/2011 12:12:55 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[UserDownload](
	[DownloadID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[MediaType] [int] NOT NULL,
	[MediaPath] [nvarchar](max) NOT NULL,
	[MediaID] [int] NOT NULL,
 CONSTRAINT [PK_UserDownload] PRIMARY KEY CLUSTERED 
(
	[DownloadID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[UserDownload]  WITH CHECK ADD  CONSTRAINT [FK_UserDownload_MediaType] FOREIGN KEY([MediaType])
REFERENCES [dbo].[MediaType] ([TypeID])
GO

ALTER TABLE [dbo].[UserDownload] CHECK CONSTRAINT [FK_UserDownload_MediaType]
GO

ALTER TABLE [dbo].[UserDownload]  WITH CHECK ADD  CONSTRAINT [FK_UserDownload_User] FOREIGN KEY([UserID])
REFERENCES [dbo].[User] ([UserID])
GO

ALTER TABLE [dbo].[UserDownload] CHECK CONSTRAINT [FK_UserDownload_User]
GO

