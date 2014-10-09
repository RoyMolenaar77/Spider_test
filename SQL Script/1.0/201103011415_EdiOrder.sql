/****** Object:  Table [dbo].[Order]    Script Date: 03/01/2011 14:14:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[EdiOrder](
	[EdiOrderID] [int] IDENTITY(1,1) NOT NULL,
	[Document] [nvarchar](max) NULL,
	[ConnectorID] [int] NOT NULL,
	[IsDispatched] [bit] NOT NULL,
	[DispatchToVendorDate] [datetime] NULL,
	[ReceivedDate] [datetime] NOT NULL,
	[isDropShipment] [bit] NULL,
	[Remarks] [nvarchar](max) NULL,
	[ShipToCustomerID] [int] NULL,
	[SoldToCustomerID] [int] NULL,
	[CustomerOrderReference] [nvarchar](max) NULL,
	[EdiVersion] [nvarchar](50) NULL,
	[BSKIdentifier] [nvarchar](200) NULL,
	[WebSiteOrderNumber] [nvarchar](100) NULL,
	[PaymentTermsCode] [nvarchar](50) NULL,
	[PaymentInstrument] [nvarchar](50) NULL,
	[BackOrdersAllowed] [bit] NULL,
	[RouteCode] [nvarchar](50) NULL,
	[HoldCode] [nvarchar](50) NULL,
	[HoldOrder] [bit] NOT NULL,
 CONSTRAINT [PK_EdiOrder] PRIMARY KEY CLUSTERED 
(
	[EdiOrderID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO



