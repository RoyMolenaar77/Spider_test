IF NOT EXISTS (
	SELECT * 
    FROM   sys.columns 
    WHERE  name = 'HasOption' 
    AND object_id = OBJECT_ID('ProductAttributeMetaData')) 

BEGIN
       ALTER TABLE ProductAttributeMetaData
       ADD HasOption bit null
END
GO