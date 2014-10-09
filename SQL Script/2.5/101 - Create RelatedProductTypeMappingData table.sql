DECLARE @intErrorCode INT

BEGIN TRAN

  --Begin Action
  IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'RelatedProductTypeMappingData' )
    BEGIN
    
     CREATE TABLE [dbo].[RelatedProductTypeMappingData](
    [RelatedProductTypeMappingID] [int] NOT NULL,
    [RelatedProductTypeID] [int] NOT NULL,
    [ConnectorID] [int] NOT NULL,
    CONSTRAINT [PK_RelatedProductTypeMappingData] PRIMARY KEY CLUSTERED 
  (
    [RelatedProductTypeMappingID] ASC,
    [RelatedProductTypeID] ASC,
    [ConnectorID] ASC
  )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
  ) ON [PRIMARY]
  

      PRINT 'Added table RelatedProductTypeMappingData'
    END
  ELSE
    BEGIN
      PRINT 'Table RelatedProductTypeMappingData already added to the database'
    END
  --End Action 

  SELECT @intErrorCode = @@ERROR
  IF (@intErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while creating table RelatedProductTypeMappingData'

  ROLLBACK TRAN
END