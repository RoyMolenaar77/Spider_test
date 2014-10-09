/****** Object:  Table [dbo].[EdiOrderResponse]    Script Date: 03/01/2011 14:17:13 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[EdiOrderResponse](
	[EdiOrderResponseID] [int] IDENTITY(1,1) NOT NULL,
	[ResponseType] [nvarchar](50) NOT NULL,
	[VendorDocument] [text] NOT NULL,
	[VendorID] [int] NOT NULL,
	[AdministrationCost] [decimal](18, 4) NULL,
	[DropShipmentCost] [decimal](18, 4) NULL,
	[ShipmentCost] [decimal](18, 4) NULL,
	[OrderDate] [datetime] NULL,
	[PartialDelivery] [bit] NULL,
	[VendorDocumentNumber] [nvarchar](50) NOT NULL,
	[VendorDocumentDate] [datetime] NULL,
	[VatPercentage] [decimal](18, 4) NULL,
	[VatAmount] [decimal](18, 4) NULL,
	[TotalGoods] [decimal](18, 4) NULL,
	[TotalExVat] [decimal](18, 4) NULL,
	[TotalAmount] [decimal](18, 4) NULL,
	[PaymentConditionDays] [int] NULL,
	[PaymentConditionCode] [nvarchar](50) NULL,
	[PaymentConditionDiscount] [nvarchar](50) NULL,
	[PaymentConditionDiscountDescription] [nvarchar](100) NULL,
	[TrackAndTrace] [nvarchar](255) NULL,
	[InvoiceDocumentNumber] [nvarchar](50) NULL,
	[ShippingNumber] [nvarchar](50) NULL,
	[ReqDeliveryDate] [datetime] NULL,
	[InvoiceDate] [datetime] NULL,
	[Currency] [nvarchar](50) NULL,
	[DespAdvice] [nvarchar](50) NULL,
	[ShipToCustomerID] [int] NULL,
	[SoldToCustomerID] [int] NULL,
	[ReceiveDate] [datetime] NOT NULL,
	[TrackAndTraceLink] [nvarchar](255) NULL,
	[VendorDocumentReference] [nvarchar](255) NULL,
	[OrderID] [int] NULL,
 CONSTRAINT [PK_EdiOrderResponse] PRIMARY KEY CLUSTERED 
(
	[EdiOrderResponseID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

