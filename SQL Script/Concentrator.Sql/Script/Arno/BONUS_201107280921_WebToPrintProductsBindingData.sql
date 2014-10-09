USE [Concentrator]
GO
/****** Object:  Table [dbo].[WebToPrintBinding]    Script Date: 07/28/2011 09:20:48 ******/
SET IDENTITY_INSERT [dbo].[WebToPrintBinding] ON
INSERT [dbo].[WebToPrintBinding] ([BindingID], [Name], [Query]) VALUES (1, N'Product', N'select Product.ProductID as ''1'', ImageUrl as ''2'', ShortContentDescription as ''3'', LongContentDescription as ''4'', ShortSummaryDescription as ''5'', WarrantyInfo as ''6'', ModelName as ''7'' FROM Product   LEFT JOIN ProductImage ON Product.ProductID = ProductImage.ProductID  LEFT JOIN ProductDescription ON Product.ProductID = ProductDescription.ProductID where Product.ProductID=@8')
SET IDENTITY_INSERT [dbo].[WebToPrintBinding] OFF
/****** Object:  Table [dbo].[WebToPrintBindingField]    Script Date: 07/28/2011 09:20:48 ******/
INSERT [dbo].[WebToPrintBindingField] ([FieldID], [BindingID], [Name], [Type], [SearchType]) VALUES (1, 1, N'Product.ProductID', 5, 0)
INSERT [dbo].[WebToPrintBindingField] ([FieldID], [BindingID], [Name], [Type], [SearchType]) VALUES (2, 1, N'ImageUrl', 5, 0)
INSERT [dbo].[WebToPrintBindingField] ([FieldID], [BindingID], [Name], [Type], [SearchType]) VALUES (3, 1, N'ShortContentDescription', 5, 0)
INSERT [dbo].[WebToPrintBindingField] ([FieldID], [BindingID], [Name], [Type], [SearchType]) VALUES (4, 1, N'LongContentDescription', 5, 0)
INSERT [dbo].[WebToPrintBindingField] ([FieldID], [BindingID], [Name], [Type], [SearchType]) VALUES (5, 1, N'ShortSummaryDescription', 5, 0)
INSERT [dbo].[WebToPrintBindingField] ([FieldID], [BindingID], [Name], [Type], [SearchType]) VALUES (6, 1, N'WarrantyInfo', 5, 0)
INSERT [dbo].[WebToPrintBindingField] ([FieldID], [BindingID], [Name], [Type], [SearchType]) VALUES (7, 1, N'ModelName', 5, 0)
INSERT [dbo].[WebToPrintBindingField] ([FieldID], [BindingID], [Name], [Type], [SearchType]) VALUES (8, 1, N'productid', 2, 1)
/****** Object:  Table [dbo].[WebToPrintProject]    Script Date: 07/28/2011 09:20:48 ******/
SET IDENTITY_INSERT [dbo].[WebToPrintProject] ON
INSERT [dbo].[WebToPrintProject] ([ProjectID], [UserID], [Name], [Description]) VALUES (1, 1, N'Test', N'Test')
SET IDENTITY_INSERT [dbo].[WebToPrintProject] OFF
