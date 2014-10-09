
IF EXISTS(SELECT * FROM sys.objects WHERE type = 'P' AND name = '[CreateWebshop_Sapph]')
         DROP PROCEDURE [CreateWebshop_Sapph]
GO

-- =============================================
-- Author:		Floris Teunissen
-- Create date: 11-06-2014
-- Description:	Stored Procedure for creating a new Sapph Webshop
-- =============================================
CREATE PROCEDURE [dbo].[CreateWebshop_Sapph] 
	-- Add the parameters for the stored procedure here

	-- Error Variable Parameter
	@ErrorVariable nvarchar(50) = 'ErrorVariable',

	-- Connector Parameters
	@ConnectorName nvarchar(50),
	@ConnectorType int = 27,
	@ConnectorSystemID int = 2,
	@ConcatenateBrandName int = 0,
	@ObsoleteProducts int = 0,
	@ZipCodes int = 0,
	@Selectors int = 0,
	@OverrideDescriptions int = 0,
	--BSK Identifier needs to be unique
	@BSKIdentifier int,
	@UseConcentratorProductID int = 0,
	@Connection nvarchar(max) = 'server=172.16.250.15;User Id=mage_sapph;password=phage25voE!;database=magento_sapph;Connect Timeout=30000;Default Command Timeout=30000;port=3306',
	@ImportCommercialText int = 0,
	@ConnectorIsActive int = 1,
	@AdministrativeVendorID int = 50,
	@OutboundUrl nvarchar(50) = 'http://10.172.26.1:1331',
	@ConnectorSystemType int = 789,
	@IgnoreMissingImage int = 1,
	@IgnoreMissingConcentratorDescription int = 0,

	-- Vendor Parameters
	@VendorID int,
	@VendorName nvarchar(50), 
	@VendorDescription nvarchar(200) = 'New Sapph Webshop',
	@VendorType int = 135,
	@ParentVendorID int = 50,
	@OrderDispatcherType nvarchar(100) = 'Concentrator.Objects.Ordering.Dispatch.TNTDispatcher',
	@IsActive int = 1,

	-- ConnectorSetting Parameters
	@ImageTriggerIndexing nvarchar(10) = 'True',
	@ListingAttributes nvarchar (50) = '85,86,84,81',
	@MagentoCategoriesWithPageLayoutShouldNotBeHid nvarchar(10) = 'True',
	@MagentoGetOptionSortFromOtherValueGroups nvarchar(10) = 'True',
	@MagentoVersion nvarchar(20) = 'Version_16',
	@MagentoWebsitePattern nvarchar(10) = 'BE',
	@MandatoryAttributes nvarchar(50) = '81,84,85,86',
	@NoConfigurableAttributesOverrides nvarchar(50) = '81,84',
	@NotSearchableAttributes nvarchar(50) = '41',
	@PriceInTax nvarchar(10) = 'False',
	@ProductAttributeMetaDataReIndex nvarchar(100) = '86=1,82=2,83=3,40=5',
	@Returned nvarchar(100) = 'http://sapph.diract-it.nl/refunds/refund/index/',
	@SCBUrl nvarchar(100) = 'http://sapph.diract-it.nl/storesmanager/shipment/processOrderLines',
	@TrgFilePath nvarchar(100) = '/content_staging/sapph/',
	@TriggerIndexing nvarchar(10) = 'True',
	@UseAxapta nvarchar(10) = 'True',
	@UseContentDescriptions nvarchar(10) = 'True',
	@UseShipmentCosts nvarchar(10) = 'True',
	@WebsiteCodeInCoreTable nvarchar(20)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @intErrorCode INT

	-- INSERT CONNECTOR
	BEGIN TRAN
	if not exists (select * from Connector where Name = @ConnectorName)
	BEGIN
	insert into Connector (Name, ConnectorType, ConnectorSystemID, ConcatenateBrandName, ObsoleteProducts, ZipCodes, Selectors, OverrideDescriptions, BSKIdentifier, UseConcentratorProductID, Connection, ImportCommercialText, IsActive, AdministrativeVendorID, ConnectorSystemType, IgnoreMissingImage, IgnoreMissingConcentratorDescription)
	values (@ConnectorName, @ConnectorType, @ConnectorSystemID, @ConcatenateBrandName, @ObsoleteProducts, @ZipCodes, @Selectors, @OverrideDescriptions, @BSKIdentifier, @UseConcentratorProductID, @Connection, @ImportCommercialText, @ConnectorIsActive, @AdministrativeVendorID, @ConnectorSystemType, @IgnoreMissingImage, @IgnoreMissingConcentratorDescription)
	END

	-- CHECK FOR ERRRORS
	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0) GOTO ConnectorPROBLEM
	COMMIT TRAN

	-- STOP STORED PROCEDURE ON ERRORS AND ROLLBACK TRANSACTION
	ConnectorPROBLEM:
	  IF (@intErrorCode <> 0) 
	  BEGIN
		PRINT 'Unexpected error occurred while adding new connector, fix errors and try again.'
		ROLLBACK TRAN
		RETURN
	END

	-- INSERT VENDOR
	SELECT @VendorID = 51

	BEGIN TRAN
	if not exists (select * from Vendor where Name = @VendorName)
	BEGIN
	insert into Vendor (VendorID, Name, [Description], VendorType, ParentVendorID, OrderDispatcherType, IsActive)
	values(@VendorID, @VendorName, @VendorDescription, @VendorType, @ParentVendorID, @OrderDispatcherType, @IsActive)
	END

	-- CHECK FOR ERRORS
	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0) GOTO VendorPROBLEM
	COMMIT TRAN

	-- STOP STORED PROCEDURE ON ERRORS AND ROLLBACK TRANSACTION
	VendorPROBLEM:
	  IF (@intErrorCode <> 0) 
	  BEGIN
		PRINT 'Unexpected error occurred while adding new vendor, fix errors and try again.'
		ROLLBACK TRAN
		RETURN
	END

	-- INSERT VENDORSETTINGS
	BEGIN TRAN

	-- GET LATEST CONNECTORID
	DECLARE @RelatedConnectorID int
	SELECT @RelatedConnectorID = max(ConnectorID) from Connector

	if not exists (select * from VendorSetting where VendorID = @VendorID and SettingKey = @RelatedConnectorID)
	BEGIN
	insert into VendorSetting (VendorID, SettingKey, Value)
	values(@VendorID, 'RelatedConnectorID', @RelatedConnectorID)
	END

	SELECT @intErrorCode = @@ERROR
	SELECT @ErrorVariable = 'RelatedConnectorID'
	IF (@intErrorCode <> 0) GOTO PROBLEM
	COMMIT TRAN

	-- INSERT CONNECTORSETTINGS
	BEGIN TRAN

	if not exists (select * from ConnectorSetting where ConnectorID = @RelatedConnectorID and SettingKey = 'ImageTriggerIndexing')
	BEGIN
	insert into ConnectorSetting(ConnectorID, SettingKey, Value)
	values(@RelatedConnectorID, 'ImageTriggerIndexing', @ImageTriggerIndexing)
	END

	SELECT @intErrorCode = @@ERROR
	SELECT @ErrorVariable = 'ImageTriggerIndexing'
	IF (@intErrorCode <> 0) GOTO PROBLEM
	COMMIT TRAN

	BEGIN TRAN

	if not exists (select * from ConnectorSetting where ConnectorID = @RelatedConnectorID and SettingKey = 'ListingAttributes')
	BEGIN
	insert into ConnectorSetting(ConnectorID, SettingKey, Value)
	values(@RelatedConnectorID, 'ListingAttributes', @ListingAttributes)
	END

	SELECT @intErrorCode = @@ERROR
	SELECT @ErrorVariable = 'ListingAttributes'
	IF (@intErrorCode <> 0) GOTO PROBLEM
	COMMIT TRAN
	
	BEGIN TRAN

	if not exists (select * from ConnectorSetting where ConnectorID = @RelatedConnectorID and SettingKey = 'MagentoCategoriesWithPageLayoutShouldNotBeHid')
	BEGIN
	insert into ConnectorSetting(ConnectorID, SettingKey, Value)
	values(@RelatedConnectorID, 'Magento.CategoriesWithPageLayoutShouldNotBeHid', @MagentoCategoriesWithPageLayoutShouldNotBeHid)
	END

	SELECT @intErrorCode = @@ERROR
	SELECT @ErrorVariable = 'MagentoCategoriesWithPageLayoutShouldNotBeHid'
	IF (@intErrorCode <> 0) GOTO PROBLEM
	COMMIT TRAN

	BEGIN TRAN

	if not exists (select * from ConnectorSetting where ConnectorID = @RelatedConnectorID and SettingKey = 'MagentoGetOptionSortFromOtherValueGroups')
	BEGIN
	insert into ConnectorSetting(ConnectorID, SettingKey, Value)
	values(@RelatedConnectorID, 'Magento.GetOptionSortFromOtherValueGroups', @MagentoGetOptionSortFromOtherValueGroups)
	END

	SELECT @intErrorCode = @@ERROR
	SELECT @ErrorVariable = 'MagentoGetOptionSortFromOtherValueGroups'
	IF (@intErrorCode <> 0) GOTO PROBLEM
	COMMIT TRAN

	BEGIN TRAN
	-- content stock
	insert into contentstock values (@VendorID, @RelatedConnectorID, 1, 1, getdate(), null, null)

	COMMIT TRAN

	BEGIN TRAN

	if not exists (select * from ConnectorSetting where ConnectorID = @RelatedConnectorID and SettingKey = 'MagentoVersion')
	BEGIN
	insert into ConnectorSetting(ConnectorID, SettingKey, Value)
	values(@RelatedConnectorID, 'MagentoVersion', @MagentoVersion)
	END

	SELECT @intErrorCode = @@ERROR
	SELECT @ErrorVariable = 'MagentoVersion'
	IF (@intErrorCode <> 0) GOTO PROBLEM
	COMMIT TRAN

	BEGIN TRAN

	if not exists (select * from ConnectorSetting where ConnectorID = @RelatedConnectorID and SettingKey = 'MagentoWebsitePattern')
	BEGIN
	insert into ConnectorSetting(ConnectorID, SettingKey, Value)
	values(@RelatedConnectorID, 'MagentoWebsitePattern', @MagentoWebsitePattern)
	END

	SELECT @intErrorCode = @@ERROR
	SELECT @ErrorVariable = 'MagentoWebsitePattern'
	IF (@intErrorCode <> 0) GOTO PROBLEM
	COMMIT TRAN

	BEGIN TRAN

	if not exists (select * from ConnectorSetting where ConnectorID = @RelatedConnectorID and SettingKey = 'MandantoryAttributes')
	BEGIN
	insert into ConnectorSetting(ConnectorID, SettingKey, Value)
	values(@RelatedConnectorID, 'MandantoryAttributes', @MandatoryAttributes)
	END

	SELECT @intErrorCode = @@ERROR
	SELECT @ErrorVariable = 'MandantoryAttributes'
	IF (@intErrorCode <> 0) GOTO PROBLEM
	COMMIT TRAN

	BEGIN TRAN

	if not exists (select * from ConnectorSetting where ConnectorID = @RelatedConnectorID and SettingKey = 'NoConfigurableAttributesOverrides')
	BEGIN
	insert into ConnectorSetting(ConnectorID, SettingKey, Value)
	values(@RelatedConnectorID, 'NoConfigurableAttributesOverrides', @NoConfigurableAttributesOverrides)
	END

	SELECT @intErrorCode = @@ERROR
	SELECT @ErrorVariable = 'NoConfigurableAttributesOverrides'
	IF (@intErrorCode <> 0) GOTO PROBLEM
	COMMIT TRAN

	BEGIN TRAN

	if not exists (select * from ConnectorSetting where ConnectorID = @RelatedConnectorID and SettingKey = 'NotSearchableAttributes')
	BEGIN
	insert into ConnectorSetting(ConnectorID, SettingKey, Value)
	values(@RelatedConnectorID, 'NotSearchableAttributes', @NotSearchableAttributes)
	END

	SELECT @intErrorCode = @@ERROR
	SELECT @ErrorVariable = 'NotSearchableAttributes'
	IF (@intErrorCode <> 0) GOTO PROBLEM
	COMMIT TRAN

	BEGIN TRAN

	if not exists (select * from ConnectorSetting where ConnectorID = @RelatedConnectorID and SettingKey = 'PriceInTax')
	BEGIN
	insert into ConnectorSetting(ConnectorID, SettingKey, Value)
	values(@RelatedConnectorID, 'PriceInTax', @PriceInTax)
	END

	SELECT @intErrorCode = @@ERROR
	SELECT @ErrorVariable = 'PriceInTax'
	IF (@intErrorCode <> 0) GOTO PROBLEM
	COMMIT TRAN

	BEGIN TRAN

	if not exists (select * from ConnectorSetting where ConnectorID = @RelatedConnectorID and SettingKey = 'ProductAttributeMetaDataReIndex')
	BEGIN
	insert into ConnectorSetting(ConnectorID, SettingKey, Value)
	values(@RelatedConnectorID, 'ProductAttributeMetaDataReIndex', @ProductAttributeMetaDataReIndex)
	END

	SELECT @intErrorCode = @@ERROR
	SELECT @ErrorVariable = 'ProductAttributeMetaDataReIndex'
	IF (@intErrorCode <> 0) GOTO PROBLEM
	COMMIT TRAN

	BEGIN TRAN

	if not exists (select * from ConnectorSetting where ConnectorID = @RelatedConnectorID and SettingKey = 'Returned')
	BEGIN
	insert into ConnectorSetting(ConnectorID, SettingKey, Value)
	values(@RelatedConnectorID, 'Returned', @Returned)
	END

	SELECT @intErrorCode = @@ERROR
	SELECT @ErrorVariable = 'Returned'
	IF (@intErrorCode <> 0) GOTO PROBLEM
	COMMIT TRAN

	BEGIN TRAN

	if not exists (select * from ConnectorSetting where ConnectorID = @RelatedConnectorID and SettingKey = 'SCBUrl')
	BEGIN
	insert into ConnectorSetting(ConnectorID, SettingKey, Value)
	values(@RelatedConnectorID, 'SCBUrl', @SCBUrl)
	END

	SELECT @intErrorCode = @@ERROR
	SELECT @ErrorVariable = 'SCBUrl'
	IF (@intErrorCode <> 0) GOTO PROBLEM
	COMMIT TRAN

	BEGIN TRAN

	if not exists (select * from ConnectorSetting where ConnectorID = @RelatedConnectorID and SettingKey = 'TrgFilePath')
	BEGIN
	insert into ConnectorSetting(ConnectorID, SettingKey, Value)
	values(@RelatedConnectorID, 'TrgFilePath', @TrgFilePath)
	END

	SELECT @intErrorCode = @@ERROR
	SELECT @ErrorVariable = 'TrgFilePath'
	IF (@intErrorCode <> 0) GOTO PROBLEM
	COMMIT TRAN

	BEGIN TRAN

	if not exists (select * from ConnectorSetting where ConnectorID = @RelatedConnectorID and SettingKey = 'TriggerIndexing')
	BEGIN
	insert into ConnectorSetting(ConnectorID, SettingKey, Value)
	values(@RelatedConnectorID, 'TriggerIndexing', @TriggerIndexing)
	END

	SELECT @intErrorCode = @@ERROR
	SELECT @ErrorVariable = 'TriggerIndexing'
	IF (@intErrorCode <> 0) GOTO PROBLEM
	COMMIT TRAN

	BEGIN TRAN

	if not exists (select * from ConnectorSetting where ConnectorID = @RelatedConnectorID and SettingKey = 'UseAxapta')
	BEGIN
	insert into ConnectorSetting(ConnectorID, SettingKey, Value)
	values(@RelatedConnectorID, 'UseAxapta', @UseAxapta)
	END

	SELECT @intErrorCode = @@ERROR
	SELECT @ErrorVariable = 'UseAxapta'
	IF (@intErrorCode <> 0) GOTO PROBLEM
	COMMIT TRAN

	BEGIN TRAN
	
	if not exists (select * from ConnectorSetting where ConnectorID = @RelatedConnectorID and SettingKey = 'UseContentDescriptions')
	BEGIN
	insert into ConnectorSetting(ConnectorID, SettingKey, Value)
	values(@RelatedConnectorID, 'UseContentDescriptions', @UseContentDescriptions)
	END

	SELECT @intErrorCode = @@ERROR
	SELECT @ErrorVariable = 'UseContentDescriptions'
	IF (@intErrorCode <> 0) GOTO PROBLEM
	COMMIT TRAN

	BEGIN TRAN

	if not exists (select * from ConnectorSetting where ConnectorID = @RelatedConnectorID and SettingKey = 'UseShipmentCosts')
	BEGIN
	insert into ConnectorSetting(ConnectorID, SettingKey, Value)
	values(@RelatedConnectorID, 'UseShipmentCosts', @UseShipmentCosts)
	END

	SELECT @intErrorCode = @@ERROR
	SELECT @ErrorVariable = 'UseShipmentCosts'
	IF (@intErrorCode <> 0) GOTO PROBLEM
	COMMIT TRAN

	BEGIN TRAN

	if not exists (select * from ConnectorSetting where ConnectorID = @RelatedConnectorID and SettingKey = 'WebsiteCodeInCoreTable')
	BEGIN
	insert into ConnectorSetting(ConnectorID, SettingKey, Value)
	values(@RelatedConnectorID, 'WebsiteCodeInCoreTable', @WebsiteCodeInCoreTable)
	END

	SELECT @intErrorCode = @@ERROR
	SELECT @ErrorVariable = 'WebsiteCodeInCoreTable'
	IF (@intErrorCode <> 0) GOTO PROBLEM
	COMMIT TRAN

	PROBLEM:
	  IF (@intErrorCode <> 0) 
	  BEGIN
		PRINT 'Unexpected error occurred while adding connectorsetting: ' + @ErrorVariable
		ROLLBACK TRAN
	END

END



GO


