/*
   dinsdag 22 maart 201110:34:56
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
ALTER TABLE dbo.ProductMatch ADD
	MatchStatus int NOT NULL CONSTRAINT DF_ProductMatch_MatchStatus DEFAULT ((1))
GO
ALTER TABLE dbo.ProductMatch SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
