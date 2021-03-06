/*
   dinsdag 8 maart 201111:11:11
   User: 
   Server: DIRACT-025\SQL2008
   Database: Concentrator_storage
   Application: 
*/

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.EdiOrderListener
	DROP CONSTRAINT DF_EdiOrderListener_Processed
GO
CREATE TABLE dbo.Tmp_EdiOrderListener
	(
	EdiRequestID int NOT NULL IDENTITY (1, 1),
	CustomerName nvarchar(255) NULL,
	CustomerIP nvarchar(50) NULL,
	CustomerHostName nvarchar(50) NULL,
	RequestDocument nvarchar(MAX) NULL,
	Type nvarchar(50) NULL,
	ReceivedDate datetime NULL,
	Processed bit NULL,
	ResponseRemark nvarchar(255) NULL,
	ResponseTime int NULL,
	ErrorMessage nvarchar(MAX) NULL,
	InstanceID uniqueidentifier NULL,
	Status int NULL,
	CustomerOrderReference nvarchar(255) NULL,
	BSKIdentifier int NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_EdiOrderListener SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_EdiOrderListener ADD CONSTRAINT
	DF_EdiOrderListener_Processed DEFAULT ((0)) FOR Processed
GO
SET IDENTITY_INSERT dbo.Tmp_EdiOrderListener ON
GO
IF EXISTS(SELECT * FROM dbo.EdiOrderListener)
	 EXEC('INSERT INTO dbo.Tmp_EdiOrderListener (EdiRequestID, CustomerName, CustomerIP, CustomerHostName, RequestDocument, Type, ReceivedDate, Processed, ResponseRemark, ResponseTime, ErrorMessage, InstanceID, Status, CustomerOrderReference, BSKIdentifier)
		SELECT EdiRequestID, CustomerName, CustomerIP, CustomerHostName, RequestDocument, Type, ReceivedDate, Processed, ResponseRemark, CONVERT(int, ResponseTime), ErrorMessage, InstanceID, Status, CustomerOrderReference, BSKIdentifier FROM dbo.EdiOrderListener WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_EdiOrderListener OFF
GO
DROP TABLE dbo.EdiOrderListener
GO
EXECUTE sp_rename N'dbo.Tmp_EdiOrderListener', N'EdiOrderListener', 'OBJECT' 
GO
ALTER TABLE dbo.EdiOrderListener ADD CONSTRAINT
	PK_EdiOrderListener PRIMARY KEY CLUSTERED 
	(
	EdiRequestID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT
