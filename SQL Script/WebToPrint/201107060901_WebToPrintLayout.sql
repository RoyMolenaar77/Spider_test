/****** Object:  Table [dbo].[WebToPrintLayout]    Script Date: 07/06/2011 09:00:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[WebToPrintLayout](
	[LayoutID] [int] IDENTITY(1,1) NOT NULL,
	[ProjectID] [int] NOT NULL,
	[LayoutType] [nvarchar](255) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Data] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_WebToPrintLayout] PRIMARY KEY CLUSTERED 
(
	[LayoutID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[WebToPrintLayout]  WITH CHECK ADD  CONSTRAINT [FK_WebToPrintLayout_WebToPrintProject] FOREIGN KEY([ProjectID])
REFERENCES [dbo].[WebToPrintProject] ([ProjectID])
GO

ALTER TABLE [dbo].[WebToPrintLayout] CHECK CONSTRAINT [FK_WebToPrintLayout_WebToPrintProject]
GO
