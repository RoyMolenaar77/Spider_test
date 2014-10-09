alter table dbo.ConnectorPaymentProvider drop column [profile] 
alter table dbo.ConnectorPaymentProvider add Portfolio int NOT NULL 
alter table dbo.ConnectorPaymentProvider add UserName nvarchar(50) NOT NULL
alter table dbo.ConnectorPaymentProvider add [Password] nvarchar(50) NOT NULL
