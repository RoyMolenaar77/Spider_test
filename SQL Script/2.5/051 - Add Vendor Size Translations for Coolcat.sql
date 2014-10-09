
declare @sourceVendorID int
declare @destinationVendorID int
declare @sourceSize nvarchar(15)
declare @destinationSize nvarchar(15)

set @sourceVendorID = 15
set @destinationVendorID = 15



set @sourceSize= '4302'
set @destinationSize = '126'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '4303'
set @destinationSize = '137'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '4304'
set @destinationSize = '149'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '4305'
set @destinationSize = '161'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '4306'
set @destinationSize = '173'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '1002'
set @destinationSize = '002'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '1004'
set @destinationSize = '003'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '1006'
set @destinationSize = '004'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '1008'
set @destinationSize = '005'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '1010'
set @destinationSize = '006'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '2003'
set @destinationSize = '026'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '2004'
set @destinationSize = '027'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '2005'
set @destinationSize = '028'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '2006'
set @destinationSize = '029'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '2007'
set @destinationSize = '030'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '2008'
set @destinationSize = '031'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '2009'
set @destinationSize = '032'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '2011'
set @destinationSize = '034'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '2012'
set @destinationSize = '036'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '2809'
set @destinationSize = '282'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '2811'
set @destinationSize = '292'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '2813'
set @destinationSize = '302'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '2815'
set @destinationSize = '312'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '2817'
set @destinationSize = '322'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '2818'
set @destinationSize = '324'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '2820'
set @destinationSize = '334'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '2822'
set @destinationSize = '344'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '2824'
set @destinationSize = '364'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '6103'
set @destinationSize = '070'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '6105'
set @destinationSize = '080'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '6106'
set @destinationSize = '085'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '6108'
set @destinationSize = '095'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '6002'
set @destinationSize = '520'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '6003'
set @destinationSize = '540'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '6004'
set @destinationSize = '560'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '6005'
set @destinationSize = '580'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '6006'
set @destinationSize = '600'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '0996'
set @destinationSize = '000'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,1)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       

























set @sourceSize= '122/128'
set @destinationSize = '122/128'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '134/140'
set @destinationSize = '134/140'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '146/152'
set @destinationSize = '146/152'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '158/164'
set @destinationSize = '158/164'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '170/176'
set @destinationSize = '170/176'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= 'XS'
set @destinationSize = 'XS'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= 'S'
set @destinationSize = 'S'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= 'M'
set @destinationSize = 'M'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= 'L'
set @destinationSize = 'L'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= 'XL'
set @destinationSize = 'XL'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '26'
set @destinationSize = '26'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '27'
set @destinationSize = '27'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '28'
set @destinationSize = '28'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '29'
set @destinationSize = '29'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '30'
set @destinationSize = '30'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '31'
set @destinationSize = '31'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '32'
set @destinationSize = '32'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '34'
set @destinationSize = '34'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '36'
set @destinationSize = '36'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '28/32'
set @destinationSize = '28/32'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '29/32'
set @destinationSize = '29/32'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '30/32'
set @destinationSize = '30/32'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '31/32'
set @destinationSize = '31/32'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '32/32'
set @destinationSize = '32/32'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '32/34'
set @destinationSize = '32/34'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '33/34'
set @destinationSize = '33/34'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '34/34'
set @destinationSize = '34/34'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '36/34'
set @destinationSize = '36/34'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '70'
set @destinationSize = '70'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '80'
set @destinationSize = '80'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '85'
set @destinationSize = '85'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '95'
set @destinationSize = '95'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '52'
set @destinationSize = '52'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '54'
set @destinationSize = '54'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '56'
set @destinationSize = '56'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '58'
set @destinationSize = '58'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '60'
set @destinationSize = '60'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       


set @sourceSize= '1 SIZE'
set @destinationSize = 'geen maat'
IF NOT EXISTS (SELECT * from [VendorTranslation] where [SourceVendorID] = @sourceVendorID AND [SourceVendorValue] = @sourceSize)
BEGIN
  INSERT INTO [dbo].[VendorTranslation] ([SourceVendorID],[SourceVendorValue],[DestinationVendorID],[DestinationVendorValue],[TranslationType])
    VALUES (@sourceVendorID,@sourceSize,@destinationVendorID,@destinationSize,2)
 PRINT cast(@sourceVendorID as varchar(10))  + ', ' + @sourceSize + ' added to database'
END          
ELSE
BEGIN
PRINT cast(@sourceVendorID as varchar(10)) + ', ' + @sourceSize + ' already added'
END       




