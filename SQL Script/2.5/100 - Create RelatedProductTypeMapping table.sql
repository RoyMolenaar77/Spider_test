DECLARE @intErrorCode INT

BEGIN TRAN

  --Begin Action
  IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'RelatedProductTypeMapping' )
    BEGIN
    
      CREATE TABLE [dbo].[RelatedProductTypeMapping](
        [RelatedProductTypeMappingID] [int] NOT NULL,
        [RelatedProductTypeMappingName] [varchar](50) NOT NULL,
       CONSTRAINT [PK_RelatedProductTypeMapping] PRIMARY KEY CLUSTERED 
      (
        [RelatedProductTypeMappingID] ASC
      )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
      ) ON [PRIMARY]


      INSERT INTO RelatedProductTypeMapping (RelatedProductTypeMappingID, RelatedProductTypeMappingName) VALUES (1, 'CrossSell')
      INSERT INTO RelatedProductTypeMapping (RelatedProductTypeMappingID, RelatedProductTypeMappingName) VALUES (2, 'UpSell')
      INSERT INTO RelatedProductTypeMapping (RelatedProductTypeMappingID, RelatedProductTypeMappingName) VALUES (3, 'Accessory')

      PRINT 'Added table RelatedProductTypeMapping'
    END
  ELSE
    BEGIN
      PRINT 'Table RelatedProductTypeMapping already added to the database'
    END
  --End Action 

  SELECT @intErrorCode = @@ERROR
  IF (@intErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while creating table RelatedProductTypeMapping'

  ROLLBACK TRAN
END