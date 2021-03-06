/*
   woensdag 22 juni 201112:39:36
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
ALTER TABLE dbo.Role
	DROP CONSTRAINT DF_Role_isHidden
GO
CREATE TABLE dbo.Tmp_Role
	(
	RoleID int NOT NULL IDENTITY (1, 1),
	RoleName nvarchar(50) NULL,
	isHidden bit NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_Role SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_Role ADD CONSTRAINT
	DF_Role_isHidden DEFAULT ((0)) FOR isHidden
GO
SET IDENTITY_INSERT dbo.Tmp_Role ON
GO
IF EXISTS(SELECT * FROM dbo.Role)
	 EXEC('INSERT INTO dbo.Tmp_Role (RoleID, RoleName, isHidden)
		SELECT RoleID, RoleName, isHidden FROM dbo.Role WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_Role OFF
GO
ALTER TABLE dbo.FunctionalityRole
	DROP CONSTRAINT FK_FunctionalityRole_Role
GO
ALTER TABLE dbo.ManagementPage
	DROP CONSTRAINT FK_ManagementPage_Role
GO
ALTER TABLE dbo.UserRole
	DROP CONSTRAINT FK_UserRoles_Role
GO
DROP TABLE dbo.Role
GO
EXECUTE sp_rename N'dbo.Tmp_Role', N'Role', 'OBJECT' 
GO
ALTER TABLE dbo.Role ADD CONSTRAINT
	PK_Role PRIMARY KEY CLUSTERED 
	(
	RoleID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.UserRole ADD CONSTRAINT
	FK_UserRoles_Role FOREIGN KEY
	(
	RoleID
	) REFERENCES dbo.Role
	(
	RoleID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.UserRole SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ManagementPage ADD CONSTRAINT
	FK_ManagementPage_Role FOREIGN KEY
	(
	RoleID
	) REFERENCES dbo.Role
	(
	RoleID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ManagementPage SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.FunctionalityRole ADD CONSTRAINT
	FK_FunctionalityRole_Role FOREIGN KEY
	(
	RoleID
	) REFERENCES dbo.Role
	(
	RoleID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.FunctionalityRole SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
