DECLARE @intErrorCode INT

BEGIN TRAN

  --Begin Action
  IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'VendorTranslation' )
    BEGIN
    
      CREATE TABLE [dbo].[VendorTranslation](
        [SourceVendorID] [int] NOT NULL,
        [SourceVendorValue] [nvarchar](50) NOT NULL,
        [DestinationVendorID] [int] NOT NULL,
        [DestinationVendorValue] [nvarchar](50) NOT NULL,
        [TranslationType] [int] NOT NULL
       CONSTRAINT [PK_VendorTranslation] PRIMARY KEY CLUSTERED 
      (
			[SourceVendorID] ASC,
			[SourceVendorValue] ASC,
			[TranslationType] ASC
      )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
      ) ON [PRIMARY]

      

      ALTER TABLE [dbo].[VendorTranslation]  WITH CHECK ADD  CONSTRAINT [FK_VendorTranslation_DestinationVendor] FOREIGN KEY([DestinationVendorID])
      REFERENCES [dbo].[Vendor] ([VendorID])
      

      ALTER TABLE [dbo].[VendorTranslation] CHECK CONSTRAINT [FK_VendorTranslation_DestinationVendor]
      

      ALTER TABLE [dbo].[VendorTranslation]  WITH CHECK ADD  CONSTRAINT [FK_VendorTranslation_Vendor] FOREIGN KEY([SourceVendorID])
      REFERENCES [dbo].[Vendor] ([VendorID])
      

      ALTER TABLE [dbo].[VendorTranslation] CHECK CONSTRAINT [FK_VendorTranslation_Vendor]
      


      PRINT 'Added table VendorTranslation'
    END
  ELSE
    BEGIN
      PRINT 'Table VendorTranslation already added to the database'
    END
  --End Action 

  SELECT @intErrorCode = @@ERROR
  IF (@intErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while creating table VendorTranslation'

  ROLLBACK TRAN
END