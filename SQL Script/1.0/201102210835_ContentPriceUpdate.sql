alter table contentprice add FixedPrice decimal(18,4)


/****** Object:  Index [IX_ProductID_ShortDescription]    Script Date: 02/16/2011 16:03:48 ******/
CREATE NONCLUSTERED INDEX [IX_ProductID_ShortDescription] ON [dbo].[VendorAssortment] 
(
	[ProductID] ASC,
	[ShortDescription] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO


