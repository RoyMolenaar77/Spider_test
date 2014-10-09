DECLARE @intErrorCode INT

BEGIN TRAN

  --Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'ConnectorPublicationRule' )
		
		BEGIN
		
		CREATE TABLE [dbo].[ConnectorPublicationRule](
		[ConnectorPublicationRuleID] [int] IDENTITY(1,1) NOT NULL,
		[ConnectorID] [int] NULL,
		[VendorID] [int] NOT NULL,
		[PublicationType] [int] NULL,
		[ProductGroupID] [int] NULL,
		[BrandID] [int] NULL,
		[ProductID] [int] NULL,
		[PublishOnlyStock] [bit] NULL,
		[PublicationIndex] [int] NOT NULL,
		[StatusID] [int] NULL,
		[FromDate] [datetime] NULL,
		[ToDate] [datetime] NULL,
		[ConnectorRelationID] [int] NULL,
		[FromPrice] [decimal](18, 4) NULL,
		[ToPrice] [decimal](18, 4) NULL,
		[CreatedBy] [int] NOT NULL,
		[CreationTime] [datetime] NOT NULL,
		[LastModifiedBy] [int] NULL,
		[LastModificationTime] [datetime] NULL,
		[IsActive] [bit] NOT NULL,
		[EnabledByDefault] [bit] NOT NULL,
		[MasterGroupMappingID] [int] NULL,
		[OnlyApprovedProducts] [bit] NOT NULL,
		[AttributeID] [int] NULL,
		[AttributeValue] [nvarchar](150) NULL,
	CONSTRAINT [PK__Connecto__2825B2124EF81926] PRIMARY KEY CLUSTERED 
	(
		[ConnectorPublicationRuleID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
	
	
	ALTER TABLE [dbo].[ConnectorPublicationRule] ADD  CONSTRAINT [DF_ConnectorPublicationRule_CreationTime]  DEFAULT (getdate()) FOR [CreationTime]
		
	ALTER TABLE [dbo].[ConnectorPublicationRule] ADD  CONSTRAINT [DF_ConnectorPublicationRule_IsActive]  DEFAULT ((0)) FOR [IsActive]
		
	ALTER TABLE [dbo].[ConnectorPublicationRule] ADD  CONSTRAINT [DF__Connector__Enabl__32ED106C]  DEFAULT ((1)) FOR [EnabledByDefault]
		
	ALTER TABLE [dbo].[ConnectorPublicationRule] ADD  CONSTRAINT [DF__Connector__OnlyA__2A9C9224]  DEFAULT ((0)) FOR [OnlyApprovedProducts]
		
	ALTER TABLE [dbo].[ConnectorPublicationRule]  WITH CHECK ADD  CONSTRAINT [FK_ConnectorPublicationRule_AssortmentStatus] FOREIGN KEY([StatusID])
	REFERENCES [dbo].[AssortmentStatus] ([StatusID])
		
	ALTER TABLE [dbo].[ConnectorPublicationRule] CHECK CONSTRAINT [FK_ConnectorPublicationRule_AssortmentStatus]
		
	ALTER TABLE [dbo].[ConnectorPublicationRule]  WITH CHECK ADD  CONSTRAINT [FK_ConnectorPublicationRule_Brand] FOREIGN KEY([BrandID])
	REFERENCES [dbo].[Brand] ([BrandID])
		
	ALTER TABLE [dbo].[ConnectorPublicationRule] CHECK CONSTRAINT [FK_ConnectorPublicationRule_Brand]
		
	ALTER TABLE [dbo].[ConnectorPublicationRule]  WITH CHECK ADD  CONSTRAINT [FK_ConnectorPublicationRule_Connector] FOREIGN KEY([ConnectorID])
	REFERENCES [dbo].[Connector] ([ConnectorID])
		
	ALTER TABLE [dbo].[ConnectorPublicationRule] CHECK CONSTRAINT [FK_ConnectorPublicationRule_Connector]
		
	ALTER TABLE [dbo].[ConnectorPublicationRule]  WITH CHECK ADD  CONSTRAINT [FK_ConnectorPublicationRule_ConnectorRelation] FOREIGN KEY([ConnectorRelationID])
	REFERENCES [dbo].[ConnectorRelation] ([ConnectorRelationID])
		
	ALTER TABLE [dbo].[ConnectorPublicationRule] CHECK CONSTRAINT [FK_ConnectorPublicationRule_ConnectorRelation]
		
	ALTER TABLE [dbo].[ConnectorPublicationRule]  WITH CHECK ADD  CONSTRAINT [FK_ConnectorPublicationRule_Product] FOREIGN KEY([ProductID])
	REFERENCES [dbo].[Product] ([ProductID])
		
	ALTER TABLE [dbo].[ConnectorPublicationRule] CHECK CONSTRAINT [FK_ConnectorPublicationRule_Product]
		
	ALTER TABLE [dbo].[ConnectorPublicationRule]  WITH CHECK ADD  CONSTRAINT [FK_ConnectorPublicationRule_ProductAttributeMetaData] FOREIGN KEY([AttributeID])
	REFERENCES [dbo].[ProductAttributeMetaData] ([AttributeID])
		
	ALTER TABLE [dbo].[ConnectorPublicationRule] CHECK CONSTRAINT [FK_ConnectorPublicationRule_ProductAttributeMetaData]
		
	ALTER TABLE [dbo].[ConnectorPublicationRule]  WITH CHECK ADD  CONSTRAINT [FK_ConnectorPublicationRule_ProductGroup] FOREIGN KEY([ProductGroupID])
	REFERENCES [dbo].[ProductGroup] ([ProductGroupID])
	
	ALTER TABLE [dbo].[ConnectorPublicationRule] CHECK CONSTRAINT [FK_ConnectorPublicationRule_ProductGroup]
		
	ALTER TABLE [dbo].[ConnectorPublicationRule]  WITH CHECK ADD  CONSTRAINT [FK_ConnectorPublicationRule_Vendor] FOREIGN KEY([VendorID])
	REFERENCES [dbo].[Vendor] ([VendorID])
	
	ALTER TABLE [dbo].[ConnectorPublicationRule] CHECK CONSTRAINT [FK_ConnectorPublicationRule_Vendor]
	
      PRINT 'Added table ConnectorPublicationRule'
    END
  ELSE
    BEGIN
      PRINT 'Table ConnectorPublicationRule already added to the database'
    END
  --End Action 

  SELECT @intErrorCode = @@ERROR
  IF (@intErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while creating table ConnectorPublicationRule'

  ROLLBACK TRAN
END