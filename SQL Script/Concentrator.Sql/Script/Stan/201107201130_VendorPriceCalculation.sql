

/****** Object:  Table [dbo].[VendorPriceCalculation]    Script Date: 07/21/2011 11:34:44 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[VendorPriceCalculation](
	[VendorPriceCalculationID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Calculation] [nvarchar](255) NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreationTime] [datetime] NOT NULL,
	[LastModificationTime] [datetime] NULL,
	[LastModifiedBy] [int] NULL,
 CONSTRAINT [PK_VendorPriceCalculation] PRIMARY KEY CLUSTERED 
(
	[VendorPriceCalculationID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[VendorPriceCalculation] ADD  DEFAULT (getdate()) FOR [CreationTime]
GO


