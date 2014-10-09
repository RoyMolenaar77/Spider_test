
/****** Object:  Table [dbo].[MagentoProductGroupSetting]    Script Date: 06/30/2011 18:34:57 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[MagentoProductGroupSetting](
	[MagentoProductGroupSettingID] [int] IDENTITY(1,1) NOT NULL,
	[ProductGroupmappingID] [int] NOT NULL,
	[ShowInMenu] [bit] NULL,
	[DisabledMenu] [bit] NULL,
	[IsAnchor] [bit] NULL,
	[CreatedBy] [int] NOT NULL,
	[CreationTime] [datetime] NOT NULL,
	[LastModifiedBy] [int] NULL,
	[LastModificationTime] [datetime] NULL,
 CONSTRAINT [PK_MagentoProductGroupSetting] PRIMARY KEY CLUSTERED 
(
	[MagentoProductGroupSettingID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[MagentoProductGroupSetting]  WITH CHECK ADD  CONSTRAINT [FK_MagentoProductGroupSetting_ProductGroupMapping] FOREIGN KEY([ProductGroupmappingID])
REFERENCES [dbo].[ProductGroupMapping] ([ProductGroupMappingID])
GO

ALTER TABLE [dbo].[MagentoProductGroupSetting] CHECK CONSTRAINT [FK_MagentoProductGroupSetting_ProductGroupMapping]
GO

ALTER TABLE [dbo].[MagentoProductGroupSetting] ADD  CONSTRAINT [DF_MagentoProductGroupSetting_CreationTime]  DEFAULT (getdate()) FOR [CreationTime]
GO


