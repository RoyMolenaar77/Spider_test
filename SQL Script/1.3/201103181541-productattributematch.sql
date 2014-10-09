/****** Object:  Table [dbo].[ProductAttributeMatch]    Script Date: 03/18/2011 15:40:51 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ProductAttributeMatch](
	[ProductAttributeMatchID] [int] IDENTITY(1,1) NOT NULL,
	[ProductAttributeGroupID] [int] NOT NULL,
	[AttributeID] [int] NULL,
	[CorrespondingProductAttributeGroupID] [int] NOT NULL,
	[CorrespondingAttributeID] [int] NULL,
	[IsMatched] [bit] NULL,
	[CreatedBy] [int] NOT NULL,
	[CreationTime] [datetime] NOT NULL,
	[LastModifiedBy] [int] NULL,
	[LastModificationTime] [datetime] NULL,
	[ConnectorID] [int] NULL,
 CONSTRAINT [PK_ProductAttributeMatch] PRIMARY KEY CLUSTERED 
(
	[ProductAttributeMatchID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[ProductAttributeMatch] ADD  CONSTRAINT [DF_ProductAttributeMatch_CreationTime]  DEFAULT (getdate()) FOR [CreationTime]
GO


