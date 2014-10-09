/****** Object:  Table [dbo].[OrderLedger]    Script Date: 03/01/2011 14:15:14 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[EdiOrderLedger](
	[EdiOrderLedgerID] [int] IDENTITY(1,1) NOT NULL,
	[EdiOrderLineID] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[LedgerDate] [datetime] NOT NULL,
 CONSTRAINT [PK_EdiOrderLedger] PRIMARY KEY CLUSTERED 
(
	[EdiOrderLedgerID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[EdiOrderLedger]  WITH CHECK ADD  CONSTRAINT [FK_EdiOrderLedger_EdiOrderLine] FOREIGN KEY([EdiOrderLineID])
REFERENCES [dbo].[EdiOrderLine] ([EdiOrderLineID])
GO

ALTER TABLE [dbo].[EdiOrderLedger] CHECK CONSTRAINT [FK_EdiOrderLedger_EdiOrderLine]
GO


