/****** Object:  Table [dbo].[ImageStore]    Script Date: 10/23/2009 11:22:41 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ImageStore](
	[ImageID] [int] IDENTITY(1,1) NOT NULL,
	[ManufacturerID] [nvarchar](50) NULL,
	[BrandID] [int] NULL,
	[ImageUrl] [nvarchar](255) NULL,
	[SizeID] [int] NULL,
	[CustomerProductID] [nvarchar](50) NULL,
	[ProductGroupID] [int] NULL,
	[ImageType] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[ImageID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
