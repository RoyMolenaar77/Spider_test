IF NOT EXISTS (SELECT * 
               FROM   sys.columns 
               WHERE  name = 'LastChanged' 
                      AND object_id = OBJECT_ID('ProductMedia')) 

BEGIN
       ALTER TABLE ProductMedia
       ADD [LastChanged] [datetime] NOT NULL
       CONSTRAINT DF_ProductMedia_LastChanged  DEFAULT (GETDATE())	

END
GO



