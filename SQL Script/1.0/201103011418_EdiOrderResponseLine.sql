
/****** Object:  Table [dbo].[EdiOrderResponseLine]    Script Date: 03/01/2011 14:17:46 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[EdiOrderResponseLine](
	[EdiOrderResponseLineID] [int] IDENTITY(1,1) NOT NULL,
	[OrderResponseID] [int] NOT NULL,
	[OrderLineID] [int] NOT NULL,
	[Ordered] [int] NOT NULL,
	[Backordered] [int] NOT NULL,
	[Cancelled] [int] NOT NULL,
	[Shipped] [int] NOT NULL,
	[Invoiced] [int] NOT NULL,
	[Unit] [nvarchar](50) NULL,
	[Price] [decimal](18, 4) NOT NULL,
	[DeliveryDate] [datetime] NULL,
	[VendorLineNumber] [nvarchar](50) NULL,
	[VendorItemNumber] [nvarchar](50) NULL,
	[OEMNumber] [nvarchar](50) NULL,
	[Barcode] [nvarchar](50) NULL,
	[Remark] [nvarchar](150) NULL,
	[Description] [nvarchar](255) NULL,
	[processed] [bit] NOT NULL,
	[RequestDate] [datetime] NULL,
	[VatAmount] [decimal](18, 4) NULL,
	[vatPercentage] [decimal](18, 4) NULL,
	[CarrierCode] [nvarchar](50) NULL,
	[NumberOfPallets] [int] NULL,
	[NumberOfUnits] [int] NULL,
	[TrackAndTrace] [nvarchar](255) NULL,
	[SerialNumbers] [nvarchar](255) NULL,
	[Delivered] [int] NOT NULL,
	[TrackAndTraceLink] [nvarchar](255) NULL,
	[ProductName] [nvarchar](255) NULL,
	[html] [text] NULL,
 CONSTRAINT [PK_EdiOrderResponseLine] PRIMARY KEY CLUSTERED 
(
	[EdiOrderResponseLineID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

