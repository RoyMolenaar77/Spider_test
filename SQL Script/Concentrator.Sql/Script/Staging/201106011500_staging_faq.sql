/****** Object:  Table [dbo].[Faq]    Script Date: 06/01/2011 15:07:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Faq](
	[FaqID] [int] IDENTITY(1,1) NOT NULL,
	[Mandatory] [bit] NULL,
	[CreatedBy] [int] NOT NULL,
	[CreationTime] [datetime] NOT NULL,
	[LastModifiedBy] [int] NULL,
	[LastModificationTime] [datetime] NULL,
 CONSTRAINT [PK_Faq] PRIMARY KEY CLUSTERED 
(
	[FaqID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FaqTranslation]    Script Date: 06/01/2011 15:07:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FaqTranslation](
	[FaqID] [int] NOT NULL,
	[LanguageID] [int] NOT NULL,
	[Question] [nvarchar](255) NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreationTime] [datetime] NOT NULL,
	[LastModifiedBy] [int] NULL,
	[LastModificationTime] [datetime] NULL,
 CONSTRAINT [PK_FaqTranslation] PRIMARY KEY CLUSTERED 
(
	[FaqID] ASC,
	[LanguageID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FaqProduct]    Script Date: 06/01/2011 15:07:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FaqProduct](
	[ProductID] [int] NOT NULL,
	[FaqID] [int] NOT NULL,
	[LanguageID] [int] NOT NULL,
	[Answer] [nvarchar](255) NULL,
	[CreatedBy] [int] NOT NULL,
	[CreationTime] [datetime] NOT NULL,
	[LastModifiedBy] [int] NULL,
	[LastModificationTime] [datetime] NULL,
 CONSTRAINT [PK_ProductFaq] PRIMARY KEY CLUSTERED 
(
	[ProductID] ASC,
	[FaqID] ASC,
	[LanguageID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Default [DF_Faq_CreationTime]    Script Date: 06/01/2011 15:07:07 ******/
ALTER TABLE [dbo].[Faq] ADD  CONSTRAINT [DF_Faq_CreationTime]  DEFAULT (getdate()) FOR [CreationTime]
GO
/****** Object:  Default [DF_ProductFaq_CreationTime]    Script Date: 06/01/2011 15:07:07 ******/
ALTER TABLE [dbo].[FaqProduct] ADD  CONSTRAINT [DF_ProductFaq_CreationTime]  DEFAULT (getdate()) FOR [CreationTime]
GO
/****** Object:  Default [DF_FaqTranslation_CreationTime]    Script Date: 06/01/2011 15:07:07 ******/
ALTER TABLE [dbo].[FaqTranslation] ADD  CONSTRAINT [DF_FaqTranslation_CreationTime]  DEFAULT (getdate()) FOR [CreationTime]
GO
/****** Object:  ForeignKey [FK_ProductFaq_Faq]    Script Date: 06/01/2011 15:07:07 ******/
ALTER TABLE [dbo].[FaqProduct]  WITH CHECK ADD  CONSTRAINT [FK_ProductFaq_Faq] FOREIGN KEY([FaqID])
REFERENCES [dbo].[Faq] ([FaqID])
GO
ALTER TABLE [dbo].[FaqProduct] CHECK CONSTRAINT [FK_ProductFaq_Faq]
GO
/****** Object:  ForeignKey [FK_ProductFaq_Language]    Script Date: 06/01/2011 15:07:07 ******/
ALTER TABLE [dbo].[FaqProduct]  WITH CHECK ADD  CONSTRAINT [FK_ProductFaq_Language] FOREIGN KEY([LanguageID])
REFERENCES [dbo].[Language] ([LanguageID])
GO
ALTER TABLE [dbo].[FaqProduct] CHECK CONSTRAINT [FK_ProductFaq_Language]
GO
/****** Object:  ForeignKey [FK_ProductFaq_Product]    Script Date: 06/01/2011 15:07:07 ******/
ALTER TABLE [dbo].[FaqProduct]  WITH CHECK ADD  CONSTRAINT [FK_ProductFaq_Product] FOREIGN KEY([ProductID])
REFERENCES [dbo].[Product] ([ProductID])
GO
ALTER TABLE [dbo].[FaqProduct] CHECK CONSTRAINT [FK_ProductFaq_Product]
GO
/****** Object:  ForeignKey [FK_FaqTranslation_Faq]    Script Date: 06/01/2011 15:07:07 ******/
ALTER TABLE [dbo].[FaqTranslation]  WITH CHECK ADD  CONSTRAINT [FK_FaqTranslation_Faq] FOREIGN KEY([FaqID])
REFERENCES [dbo].[Faq] ([FaqID])
GO
ALTER TABLE [dbo].[FaqTranslation] CHECK CONSTRAINT [FK_FaqTranslation_Faq]
GO
/****** Object:  ForeignKey [FK_FaqTranslation_Language]    Script Date: 06/01/2011 15:07:07 ******/
ALTER TABLE [dbo].[FaqTranslation]  WITH CHECK ADD  CONSTRAINT [FK_FaqTranslation_Language] FOREIGN KEY([LanguageID])
REFERENCES [dbo].[Language] ([LanguageID])
GO
ALTER TABLE [dbo].[FaqTranslation] CHECK CONSTRAINT [FK_FaqTranslation_Language]
GO
