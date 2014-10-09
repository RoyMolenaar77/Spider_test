/****** Object:  Table [dbo].[OrderListenerProcessing]    Script Date: 03/01/2011 14:09:35 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[EdiOrderListener](
	[EdiRequestID] [int] IDENTITY(1,1) NOT NULL,
	[CustomerName] [nvarchar](255) NULL,
	[CustomerIP] [nvarchar](50) NULL,
	[CustomerHostName] [nvarchar](50) NULL,
	[RequestDocument] [nvarchar](max) NULL,
	[Type] [nvarchar](50) NULL,
	[ReceivedDate] [datetime] NULL,
	[Processed] [bit] NULL,
	[ResponseRemark] [nvarchar](255) NULL,
	[ResponseTime] [datetime] NULL,
	[ErrorMessage] [nvarchar](max) NULL,
	[InstanceID] [uniqueidentifier] NULL,
	[Status] [int] NULL,
	[CustomerOrderReference] [nvarchar](255) NULL,
	[BSKIdentifier] [int] NULL,
 CONSTRAINT [PK_EdiOrderListener] PRIMARY KEY CLUSTERED 
(
	[EdiRequestID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[EdiOrderListener] ADD  CONSTRAINT [DF_EdiOrderListener_Processed]  DEFAULT ((0)) FOR [Processed]
GO


