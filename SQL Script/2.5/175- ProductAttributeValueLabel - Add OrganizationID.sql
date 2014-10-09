BEGIN TRAN

  IF NOT EXISTS(SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID('dbo.ProductAttributeValueLabel') AND [name] = 'OrganizationID')
  BEGIN
  
   alter table ProductAttributeValueLabel add OrganizationID int not null default((1));

   alter table ProductAttributeValueLabel add constraint FK_PAVL_Organization foreign key (OrganizationID) references Organization(Id)

    Declare @DropCommand nvarchar(1000);

    select @DropCommand = 'Alter table ProductAttributeValueLabel drop constraint '+ constraint_Name from INFORMATION_SCHEMA.KEY_COLUMN_USAGE where table_name = 'ProductAttributeValueLabel' and objectproperty(object_id(constraint_name), 'IsPrimaryKey') = 1

    execute (@DropCommand);

    alter table productattributevaluelabel alter column connectorid int null

    alter table productattributevaluelabel add Id int not null identity(1,1) primary key


	  PRINT 'Added column [OrganizationID] to [dbo].[ProductAttributeValueLabel]'
  END
  ELSE
  BEGIN
    PRINT 'Column [OrganizationID] already added to table [dbo].[ProductAttributeValueLabel]'
  END

  DECLARE @ErrorCode INT = @@ERROR

  IF (@ErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN

PROBLEM:
  IF (@ErrorCode <> 0)
  BEGIN
	  ROLLBACK TRAN
	  PRINT 'Column [OrganizationID] could not be added to table [dbo].[ProductAttributeValueLabel]' 
  END