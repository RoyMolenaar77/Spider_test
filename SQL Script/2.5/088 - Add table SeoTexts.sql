
-- =============================================
-- Author     : Floris Teunissen	 	
-- Create date: 11-02-2014
-- Description: Create Table 'SeoTexts'
-- =============================================

DECLARE @intErrorCode INT

BEGIN TRAN

	--Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'SeoTexts' )
		BEGIN
		
			CREATE TABLE [dbo].[SeoTexts](
				[SeoTextID] [int] IDENTITY(1,1) NOT NULL,
				[ProductGroupMappingID] [int] NOT NULL,
				[Description] [nvarchar](max) NULL,
				[DescriptionType] [int] NOT NULL,
				[LanguageID] [int] NOT NULL,
			 CONSTRAINT [PK_SeoTexts] PRIMARY KEY CLUSTERED 
			(
				[SeoTextID] ASC
			)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


			ALTER TABLE [dbo].[SeoTexts] ADD  CONSTRAINT [DF_SeoTexts_LanguageID]  DEFAULT ((2)) FOR [LanguageID]

			ALTER TABLE [dbo].[SeoTexts]  WITH CHECK ADD  CONSTRAINT [FK__SeoTexts__Langua__34179FEE] FOREIGN KEY([LanguageID])
			REFERENCES [dbo].[Language] ([LanguageID])

			ALTER TABLE [dbo].[SeoTexts] CHECK CONSTRAINT [FK__SeoTexts__Langua__34179FEE]


			PRINT 'Added table SeoTexts'
		END
	ELSE
		BEGIN
			PRINT 'Table SeoTexts already added to the database'
		END
	--End Action 

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while creating table SeoTexts'

	ROLLBACK TRAN
END