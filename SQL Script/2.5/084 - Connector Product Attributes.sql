IF OBJECT_ID('[dbo].[GetRepositoryOperationStatus_Created]', 'FN') IS NOT NULL
  DROP FUNCTION [dbo].[GetRepositoryOperationStatus_Created]
GO

CREATE FUNCTION [dbo].[GetRepositoryOperationStatus_Created]
(
)
RETURNS [INT]
AS
BEGIN
  RETURN 1
END
GO

IF OBJECT_ID('[dbo].[GetRepositoryOperationStatus_Deleted]', 'FN') IS NOT NULL
  DROP FUNCTION [dbo].[GetRepositoryOperationStatus_Deleted]
GO

CREATE FUNCTION [dbo].[GetRepositoryOperationStatus_Deleted]
(
)
RETURNS [INT]
AS
BEGIN
  RETURN 2
END
GO

IF OBJECT_ID('[dbo].[GetRepositoryOperationStatus_Updated]', 'FN') IS NOT NULL
  DROP FUNCTION [dbo].[GetRepositoryOperationStatus_Updated]
GO

CREATE FUNCTION [dbo].[GetRepositoryOperationStatus_Updated]
(
)
RETURNS [INT]
AS
BEGIN
  RETURN 3
END
GO

IF OBJECT_ID('[dbo].[SelectProductAttributeCode]', 'P') IS NOT NULL
  DROP PROCEDURE [dbo].[SelectProductAttributeCode]
GO

CREATE PROCEDURE [dbo].[SelectProductAttributeCode]
  @ProductAttributeID INT = NULL
AS
BEGIN
	SELECT  [AttributeID]
    ,     [AttributeCode]
  FROM    [dbo].[ProductAttributeMetaData]
  WHERE   [AttributeID] = ISNULL(@ProductAttributeID, [AttributeID])
END
GO

IF OBJECT_ID('[dbo].[ConnectorProductAttribute]', 'U') IS NULL
  CREATE TABLE [dbo].[ConnectorProductAttribute]
  (
    [ConnectorProductAttributeID] [int] IDENTITY(1,1) NOT NULL,
    [ConnectorID]                 [int] NOT NULL,
    [ProductAttributeID]          [int] NOT NULL,
    [ProductAttributeType]        [varchar](256) NOT NULL,
    [DefaultValue]                [nvarchar](max) NULL,
    [IsFilter]                    [bit] NOT NULL,

    CONSTRAINT [PK_ConnectorProductAttribute] PRIMARY KEY CLUSTERED 
    (
	    [ConnectorProductAttributeID] ASC
    ),
    CONSTRAINT [UK_ConnectorProductAttribute] UNIQUE NONCLUSTERED 
    (
	    [ConnectorID] ASC,
	    [ProductAttributeID] ASC,
	    [IsFilter] ASC
    ),
    CONSTRAINT [FK_ConnectorProductAttribute_Connector]         FOREIGN KEY([ConnectorID])        REFERENCES [dbo].[Connector] ([ConnectorID]),
    CONSTRAINT [FK_ConnectorProductAttribute_ProductAttribute]  FOREIGN KEY([ProductAttributeID]) REFERENCES [dbo].[ProductAttributeMetaData] ([AttributeID]),
  )
GO

IF OBJECT_ID('[dbo].[DeleteConnectorProductAttribute]', 'P') IS NOT NULL
  DROP PROCEDURE [dbo].[DeleteConnectorProductAttribute]
GO

CREATE PROCEDURE [dbo].[DeleteConnectorProductAttribute]
  @ConnectorProductAttributeID  INT
AS
BEGIN
  DELETE 
  FROM    [dbo].[ConnectorProductAttribute]
  WHERE   [ConnectorProductAttributeID] = @ConnectorProductAttributeID
END
GO

IF OBJECT_ID('[dbo].[InsertOrUpdateConnectorProductAttribute]', 'P') IS NOT NULL
  DROP PROCEDURE [dbo].[InsertOrUpdateConnectorProductAttribute]
GO

ALTER PROCEDURE [dbo].[InsertOrUpdateConnectorProductAttribute]
  @ConnectorID    INT,
  @AttributeID    INT,
  @AttributeType  VARCHAR(MAX),
  @DefaultValue   NVARCHAR(MAX),
  @IsFilter       BIT
AS
BEGIN
	SET NOCOUNT ON;

  IF EXISTS
  (
    SELECT ConnectorProductAttributeID
    FROM  [dbo].[ConnectorProductAttribute]
    WHERE [ConnectorID]         = @ConnectorID
      AND [ProductAttributeID]  = @AttributeID
      AND [IsFilter]            = @IsFilter 
  )
  BEGIN
    INSERT INTO [dbo].[ConnectorProductAttribute]
    ( [ConnectorID]
    , [ProductAttributeID]
    , [ProductAttributeType]
    , [DefaultValue]
    , [IsFilter]
    )
    VALUES
    ( @ConnectorID
    , @AttributeID
    , @AttributeType
    , @DefaultValue
    , @IsFilter
    )
    
    SELECT [dbo].[GetRepositoryOperationStatus_Created]() AS [Status], @@ROWCOUNT AS [Count]
  END
  ELSE
  BEGIN
    UPDATE  [dbo].[ConnectorProductAttribute]
    SET     [ProductAttributeType]  = ISNULL(@AttributeType,  [ProductAttributeType])
      ,     [DefaultValue]          = ISNULL(@DefaultValue,   [DefaultValue])
    WHERE   [ConnectorID]           = @ConnectorID
      AND   [ProductAttributeID]    = @AttributeID
      AND   [IsFilter]              = @IsFilter 
    
    SELECT [dbo].[GetRepositoryOperationStatus_Updated]() AS [Status], @@ROWCOUNT AS [Count]
  END
END
GO

IF OBJECT_ID('[dbo].[SelectConnectorProductAttribute]', 'P') IS NOT NULL
  DROP PROCEDURE [dbo].[SelectConnectorProductAttribute]
GO

CREATE PROCEDURE [dbo].[SelectConnectorProductAttribute]
  @ConnectorProductAttributeID  INT = NULL,
  @ConnectorID                  INT = NULL,
  @AttributeID                  INT = NULL,
  @IsFilter                     BIT = NULL
AS
BEGIN  
  SELECT  [ConnectorProductAttributeID]
    ,     [ConnectorID]
    ,     [ProductAttributeID]
    ,     [ProductAttributeType]
    ,     [DefaultValue]
    ,     [IsFilter]
  FROM    [dbo].[ConnectorProductAttribute]
  WHERE   [ConnectorProductAttributeID] = ISNULL(@ConnectorProductAttributeID,  [ConnectorProductAttributeID])
    AND   [ConnectorID]                 = ISNULL(@ConnectorID,                  [ConnectorID])
    AND   [ProductAttributeID]          = ISNULL(@AttributeID,                  [ProductAttributeID])
    AND   [IsFilter]                    = ISNULL(@IsFilter,                     [IsFilter])
END
GO

IF OBJECT_ID('[dbo].[ConnectorProductAttributeSetting]', 'U') IS NULL
  CREATE TABLE [dbo].[ConnectorProductAttributeSetting]
  (
	  [ConnectorProductAttributeID] [int]           NOT NULL,
	  [Code]                        [varchar](256)  NOT NULL,
	  [Type]                        [varchar](MAX)  NOT NULL,
	  [Value]                       [nvarchar](MAX) NOT NULL,

    CONSTRAINT [PK_ConnectorProductAttributeSetting] PRIMARY KEY CLUSTERED 
    (
	    [ConnectorProductAttributeID] ASC,
	    [Code] ASC
    ),
    CONSTRAINT [FK_ConnectorProductAttributeSetting_ConnectorProductAttribute] FOREIGN KEY([ConnectorProductAttributeID]) 
    REFERENCES [dbo].[ConnectorProductAttribute] ([ConnectorProductAttributeID]) ON DELETE CASCADE
  )
GO

IF OBJECT_ID('[dbo].[DeleteConnectorProductAttributeSetting]', 'P') IS NOT NULL
  DROP PROCEDURE [dbo].[DeleteConnectorProductAttributeSetting]
GO

CREATE PROCEDURE [dbo].[DeleteConnectorProductAttributeSetting]
  @ConnectorProductAttributeID  INT,
  @Code                         NVARCHAR(256) = NULL
AS
BEGIN
  DELETE 
  FROM    [dbo].[ConnectorProductAttributeSetting]
  WHERE   [ConnectorProductAttributeID] = @ConnectorProductAttributeID
    AND   [Code] = ISNULL(@Code, [Code])
END
GO

IF OBJECT_ID('[dbo].[InsertOrUpdateConnectorProductAttributeSetting]', 'P') IS NOT NULL
  DROP PROCEDURE [dbo].[InsertOrUpdateConnectorProductAttributeSetting]
GO

CREATE PROCEDURE [dbo].[InsertOrUpdateConnectorProductAttributeSetting]
  @ConnectorProductAttributeID  INT,
  @Code                         VARCHAR(256),
  @Type                         VARCHAR(MAX),
  @Value                        NVARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON;
  
  IF EXISTS
  (
    SELECT  * 
    FROM    [dbo].[ConnectorProductAttributeSetting]
    WHERE   [ConnectorProductAttributeID] = @ConnectorProductAttributeID AND [Code] = @Code
  )
  BEGIN
    UPDATE  [dbo].[ConnectorProductAttributeSetting]
    SET     [Type] = @Type, [Value] = @Value
    WHERE   [ConnectorProductAttributeID] = @ConnectorProductAttributeID AND [Code] = @Code
  END
  ELSE
  BEGIN
    INSERT INTO [dbo].[ConnectorProductAttributeSetting] ([ConnectorProductAttributeID], [Code], [Type], [Value])
    VALUES (@ConnectorProductAttributeID, @Code, @Type, @Value)
  END
END
GO

IF OBJECT_ID('[dbo].[SelectConnectorProductAttributeSetting]', 'P') IS NOT NULL
  DROP PROCEDURE [dbo].[SelectConnectorProductAttributeSetting]
GO

CREATE PROCEDURE [dbo].[SelectConnectorProductAttributeSetting]
  @ConnectorProductAttributeID  INT           = NULL,
  @Code                         NVARCHAR(256) = NULL
AS
BEGIN
  SELECT  [ConnectorProductAttributeID]
    ,     [Code]
    ,     [Type]
    ,     [Value]
  FROM    [dbo].[ConnectorProductAttributeSetting]
  WHERE   [ConnectorProductAttributeID] = ISNULL(@ConnectorProductAttributeID,  [ConnectorProductAttributeID])
    AND   [Code]                        = ISNULL(@Code,                         [Code])
END
GO

IF OBJECT_ID('[dbo].[ConnectorProductAttributeSettingTemplate]', 'U') IS NULL
  CREATE TABLE [dbo].[ConnectorProductAttributeSettingTemplate]
  (
    [ConnectorSystemID]   INT           NOT NULL,
    [Code]                VARCHAR(256)  NOT NULL,
    [Type]                VARCHAR(MAX)  NOT NULL,
    [Value]               NVARCHAR(MAX) NOT NULL,

    CONSTRAINT [PK_ConnectorProductAttributeSettingTemplate] PRIMARY KEY CLUSTERED 
    (
	    [ConnectorSystemID] ASC,
      [Code]              ASC
    ),

    CONSTRAINT [FK_ConnectorProductAttributeSettingTemplate_ConnectorSystem] FOREIGN KEY 
    (
      [ConnectorSystemID]
    ) 
    REFERENCES [dbo].[ConnectorSystem]
    (
      [ConnectorSystemID]
    )
    ON DELETE CASCADE
  )
GO

IF OBJECT_ID('[dbo].[SelectConnectorProductAttributeSettingTemplate]', 'P') IS NOT NULL
  DROP PROCEDURE [dbo].[SelectConnectorProductAttributeSettingTemplate]
GO

CREATE PROCEDURE [dbo].[SelectConnectorProductAttributeSettingTemplate]
  @ConnectorSystemID  INT           = NULL,
  @Code               VARCHAR(256)  = NULL
AS
BEGIN
  SELECT  [ConnectorSystemID]
    ,     [Code]
    ,     [Type]
    ,     [Value]
  FROM    [dbo].[ConnectorProductAttributeSettingTemplate]
  WHERE   [ConnectorSystemID] = ISNULL(@ConnectorSystemID,  [ConnectorSystemID])
    AND   [Code]              = ISNULL(@Code,               [Code])
END
GO