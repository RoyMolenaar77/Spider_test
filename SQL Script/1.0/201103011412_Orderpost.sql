/****** Object:  Table [dbo].[OrderPostProcessing]    Script Date: 03/01/2011 14:12:02 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[EdiOrderPost](
	[OrderID] [int] NOT NULL,
	[CustomerID] [int] NOT NULL,
	[BackendOrderID] [int] NULL,
	[CustomerOrderID] [nvarchar](50) NULL,
	[Processed] [bit] NOT NULL,
	[Type] [varchar](50) NOT NULL,
	[PostDocument] [xml] NULL,
	[PostDocumentUrl] [nvarchar](max) NULL,
	[PostUrl] [nvarchar](255) NOT NULL,
	[Timestamp] [datetime] NOT NULL,
	[ResponseRemark] [varchar](50) NULL,
	[ResponseTime] [int] NULL,
	[ProcessedCount] [int] NULL,
	[EdiRequestID] [int] NOT NULL,
	[ErrorMessage] [nvarchar](max) NULL,
	[BSKIdentifier] [int] NOT NULL,
	[DocumentCounter] [int] NOT NULL,
	[Connector] [nvarchar](50) NULL,
 CONSTRAINT [PK_EdiOrderPost] PRIMARY KEY CLUSTERED 
(
	[OrderID] ASC,
	[CustomerID] ASC,
	[Type] ASC,
	[DocumentCounter] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[EdiOrderPost] ADD  CONSTRAINT [DF_EdiOrderPost_Processed]  DEFAULT ((0)) FOR [Processed]
GO


