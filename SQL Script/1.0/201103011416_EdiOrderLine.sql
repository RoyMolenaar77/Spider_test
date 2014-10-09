/****** Object:  Table [dbo].[EdiOrderLine]    Script Date: 03/01/2011 14:16:18 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[EdiOrderLine](
	[EdiOrderLineID] [int] IDENTITY(1,1) NOT NULL,
	[Remarks] [nvarchar](max) NULL,
	[OrderID] [int] NOT NULL,
	[CustomerEdiOrderLineNr] [nvarchar](100) NULL,
	[CustomerOrderNr] [nvarchar](100) NULL,
	[ProductID] [int] NULL,
	[Price] [float] NULL,
	[Quantity] [int] NOT NULL,
	[isDispatched] [bit] NOT NULL,
	[DispatchedToVendorID] [int] NULL,
	[VendorOrderNumber] [int] NULL,
	[Response] [nvarchar](max) NULL,
	[CentralDelivery] [bit] NULL,
	[CustomerItemNumber] [nvarchar](100) NULL,
	[WareHouseCode] [nvarchar](50) NULL,
	[PriceOverride] [bit] NOT NULL,
 CONSTRAINT [PK_EdiOrderLine] PRIMARY KEY CLUSTERED 
(
	[EdiOrderLineID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO



