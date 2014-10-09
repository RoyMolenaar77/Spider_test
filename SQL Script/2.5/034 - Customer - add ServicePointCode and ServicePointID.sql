
IF NOT EXISTS (
	SELECT * 
	FROM   sys.columns 
	WHERE  name = 'ServicePointCode' 
        AND object_id = OBJECT_ID('Customer')
) 

BEGIN
    alter table Customer
	add ServicePointCode nvarchar(25) null,
	ServicePointID nvarchar(25) null,
  ServicePointName nvarchar(250) null
END
GO