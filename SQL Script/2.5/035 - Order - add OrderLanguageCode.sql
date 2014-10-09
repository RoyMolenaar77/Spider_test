
IF NOT EXISTS (
	SELECT * 
	FROM   sys.columns 
	WHERE  name = 'OrderLanguageCode' 
        AND object_id = OBJECT_ID('Order')
) 

BEGIN
  alter table [Order]
	add OrderLanguageCode varchar(25) null

END
GO




