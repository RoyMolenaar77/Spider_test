DECLARE @intErrorCode INT

BEGIN TRAN

IF NOT EXISTS(SELECT * FROM MasterGroupMappingSetting WHERE Name = 'Group')
BEGIN

INSERT INTO MasterGroupMappingSetting ([Group], [Type], [Name])
VALUES
('Magento Setting',	'option', 'Group')

INSERT INTO MasterGroupMappingSettingOption (MasterGroupMappingSettingID, Value)
VALUES
( (SELECT TOP 1 MasterGroupMappingSettingID FROM MasterGroupMappingSetting WHERE Name = 'Group'), 'Acties'),
( (SELECT TOP 1 MasterGroupMappingSettingID FROM MasterGroupMappingSetting WHERE Name = 'Group'), 'Shop Collectie'),
( (SELECT TOP 1 MasterGroupMappingSettingID FROM MasterGroupMappingSetting WHERE Name = 'Group'), 'Inspiratie')

END

IF NOT EXISTS(SELECT * FROM MasterGroupMappingSetting WHERE Name = 'Retain Products')
BEGIN

INSERT INTO MasterGroupMappingSetting ([Group], [Type], [Name])
VALUES
('General Settings',	'boolean', 'Retain Products')

END
  PRINT 'Added Default MasterGroupMapping Settings'

  SELECT @intErrorCode = @@ERROR
  IF (@intErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while adding default MasterGroup<appingSettings'

  ROLLBACK TRAN
END