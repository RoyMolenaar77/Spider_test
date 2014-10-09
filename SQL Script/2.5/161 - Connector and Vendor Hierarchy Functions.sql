IF OBJECT_ID('[dbo].[GetConnectorAncestors]') IS NOT NULL
  DROP FUNCTION [dbo].[GetConnectorAncestors]
GO

CREATE FUNCTION [dbo].[GetConnectorAncestors]
(
  @ConnectorID       INT,
  @PathSeparator  NVARCHAR(MAX) = ' / '
)
RETURNS @Connectors TABLE
(
  [ConnectorID]  INT NOT NULL, 
  [Level]     INT NOT NULL,
	[Path]		  NVARCHAR(MAX)
)
AS
BEGIN
  DECLARE @List TABLE
  (
    [Index]       INT IDENTITY,
    [ConnectorID]	  INT NOT NULL,
    [ConnectorName]  NVARCHAR(MAX)
  )

  DECLARE @CurrentID INT, @ParentID INT, @CurrentLevel INT = 0, @Index INT
  DECLARE @CurrentName NVARCHAR(MAX), @Path NVARCHAR(MAX)

  SELECT  @CurrentID = [ConnectorID], @ParentID = [ParentConnectorID], @CurrentName = [Name]
  FROM    [dbo].[Connector]
  WHERE   [ConnectorID] = @ConnectorID

  INSERT @List ([ConnectorID], [ConnectorName])
  VALUES (@CurrentID, @CurrentName)
	
  WHILE @ParentID IS NOT NULL
  BEGIN
    SELECT  @CurrentID = [ConnectorID], @ParentID = [ParentConnectorID], @CurrentName = [Name]
    FROM    [dbo].[Connector]
    WHERE   [ConnectorID] = @ParentID
    
    INSERT @List ([ConnectorID], [ConnectorName])
    VALUES (@CurrentID, @CurrentName)
  END
  
  WHILE (SELECT COUNT(*) FROM @List) > 0
  BEGIN
    SELECT TOP 1  @CurrentID = [ConnectorID], @CurrentName = [ConnectorName], @Index = [Index]
    FROM          @List
    ORDER BY      [Index] DESC
    
    IF @Path IS NULL
      SET @Path = @CurrentName
    ELSE
      SET @Path = @Path + @PathSeparator + @CurrentName

    INSERT  @Connectors
    SELECT  [ConnectorID], @CurrentLevel, @Path
    FROM    @List
    WHERE   [ConnectorID] = @CurrentID

    DELETE FROM @List WHERE [Index] = @Index

    SET @CurrentLevel = @CurrentLevel + 1
  END

  RETURN
END
GO

IF OBJECT_ID('[dbo].[GetConnectorDescendants]') IS NOT NULL
  DROP FUNCTION [dbo].[GetConnectorDescendants]
GO

CREATE FUNCTION [dbo].[GetConnectorDescendants]
(
  @ConnectorID       INT,
  @BaseLevel      INT = 0,
  @PathSeparator  NVARCHAR(MAX) = ' / '
)
RETURNS @Connectors TABLE
(
  [ConnectorID]  INT NOT NULL, 
  [Level]     INT NOT NULL,
  [Path]      NVARCHAR(MAX)
)
AS
BEGIN
  DECLARE @Queue TABLE
  (
    [ConnectorID]  INT NOT NULL, 
    [Level]     INT NOT NULL,
    [Path]      NVARCHAR(MAX)
  )
  
  DECLARE @CurrentID INT, @ParentID INT, @CurrentLevel INT = @BaseLevel
  DECLARE @CurrentName NVARCHAR(MAX)

  SELECT  @CurrentID = [ConnectorID], @ParentID = [ParentConnectorID], @CurrentName = [Name]
  FROM    [dbo].[Connector]
  WHERE   [ConnectorID] = @ConnectorID

  INSERT @Queue VALUES (@CurrentID, @CurrentLevel, @CurrentName)
	
  WHILE (SELECT COUNT(*) FROM @Queue) > 0
  BEGIN
    SELECT TOP 1 @CurrentID = [ConnectorID], @CurrentLevel = [Level], @CurrentName = [Path] FROM @Queue

    INSERT INTO @Connectors VALUES (@CurrentID, @CurrentLevel, @CurrentName)

    DELETE TOP (1) FROM @Queue
    
    INSERT @Queue SELECT [ConnectorID], @CurrentLevel + 1, @CurrentName + @PathSeparator + [Name] FROM [dbo].[Connector] WHERE [ParentConnectorID] = @CurrentID
  END

  RETURN
END
GO

IF OBJECT_ID('[dbo].[FK_Vendor_ParentVendor]') IS NOT NULL
  ALTER TABLE [dbo].[Vendor] DROP CONSTRAINT [FK_Vendor_Vendor]

IF OBJECT_ID('[dbo].[FK_Vendor_ParentVendor]') IS NULL
BEGIN
  ALTER TABLE [dbo].[Vendor] WITH CHECK ADD CONSTRAINT [FK_Vendor_ParentVendor] FOREIGN KEY([ParentVendorID]) REFERENCES [dbo].[Vendor] ([VendorID])
  ALTER TABLE [dbo].[Vendor] CHECK CONSTRAINT [FK_Vendor_ParentVendor]
END
GO

IF OBJECT_ID('[dbo].[GetVendorAncestors]') IS NOT NULL
  DROP FUNCTION [dbo].[GetVendorAncestors]
GO

CREATE FUNCTION [dbo].[GetVendorAncestors]
(
  @VendorID       INT,
  @PathSeparator  NVARCHAR(MAX) = ' / '
)
RETURNS @Vendors TABLE
(
  [VendorID]  INT NOT NULL, 
  [Level]     INT NOT NULL,
	[Path]		  NVARCHAR(MAX)
)
AS
BEGIN
  DECLARE @List TABLE
  (
    [Index]       INT IDENTITY,
    [VendorID]	  INT NOT NULL,
    [VendorName]  NVARCHAR(MAX)
  )

  DECLARE @CurrentID INT, @ParentID INT, @CurrentLevel INT = 0, @Index INT
  DECLARE @CurrentName NVARCHAR(MAX), @Path NVARCHAR(MAX)

  SELECT  @CurrentID = [VendorID], @ParentID = [ParentVendorID], @CurrentName = [Name]
  FROM    [dbo].[Vendor]
  WHERE   [VendorID] = @VendorID

  INSERT @List ([VendorID], [VendorName])
  VALUES (@CurrentID, @CurrentName)
	
  WHILE @ParentID IS NOT NULL
  BEGIN
    SELECT  @CurrentID = [VendorID], @ParentID = [ParentVendorID], @CurrentName = [Name]
    FROM    [dbo].[Vendor]
    WHERE   [VendorID] = @ParentID
    
    INSERT @List ([VendorID], [VendorName])
    VALUES (@CurrentID, @CurrentName)
  END
  
  WHILE (SELECT COUNT(*) FROM @List) > 0
  BEGIN
    SELECT TOP 1  @CurrentID = [VendorID], @CurrentName = [VendorName], @Index = [Index]
    FROM          @List
    ORDER BY      [Index] DESC
    
    IF @Path IS NULL
      SET @Path = @CurrentName
    ELSE
      SET @Path = @Path + @PathSeparator + @CurrentName

    INSERT  @Vendors
    SELECT  [VendorID], @CurrentLevel, @Path
    FROM    @List
    WHERE   [VendorID] = @CurrentID

    DELETE FROM @List WHERE [Index] = @Index

    SET @CurrentLevel = @CurrentLevel + 1
  END

  RETURN
END
GO

IF OBJECT_ID('[dbo].[GetVendorDescendants]') IS NOT NULL
  DROP FUNCTION [dbo].[GetVendorDescendants]
GO

CREATE FUNCTION [dbo].[GetVendorDescendants]
(
  @VendorID       INT,
  @BaseLevel      INT = 0,
  @PathSeparator  NVARCHAR(MAX) = ' / '
)
RETURNS @Vendors TABLE
(
  [VendorID]  INT NOT NULL, 
  [Level]     INT NOT NULL,
  [Path]      NVARCHAR(MAX)
)
AS
BEGIN
  DECLARE @Queue TABLE
  (
    [VendorID]  INT NOT NULL, 
    [Level]     INT NOT NULL,
    [Path]      NVARCHAR(MAX)
  )
  
  DECLARE @CurrentID INT, @ParentID INT, @CurrentLevel INT = @BaseLevel
  DECLARE @CurrentName NVARCHAR(MAX)

  SELECT  @CurrentID = [VendorID], @ParentID = [ParentVendorID], @CurrentName = [Name]
  FROM    [dbo].[Vendor]
  WHERE   [VendorID] = @VendorID

  INSERT @Queue VALUES (@CurrentID, @CurrentLevel, @CurrentName)
	
  WHILE (SELECT COUNT(*) FROM @Queue) > 0
  BEGIN
    SELECT TOP 1 @CurrentID = [VendorID], @CurrentLevel = [Level], @CurrentName = [Path] FROM @Queue

    INSERT INTO @Vendors VALUES (@CurrentID, @CurrentLevel, @CurrentName)

    DELETE TOP (1) FROM @Queue
    
    INSERT @Queue SELECT [VendorID], @CurrentLevel + 1, @CurrentName + @PathSeparator + [Name] FROM [dbo].[Vendor] WHERE [ParentVendorID] = @CurrentID
  END

  RETURN
END
GO