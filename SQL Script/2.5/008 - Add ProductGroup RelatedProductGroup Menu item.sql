IF NOT exists (select * from [dbo].[ManagementPage] where [Name] = 'ProductGroup RelatedProductGroup' and roleid = 1)
  begin 
    INSERT INTO [dbo].[ManagementPage]
           ([Name]
           ,[Description]
           ,[RoleID]
           ,[JSAction]
           ,[Icon]
           ,[GroupID]
           ,[ID]
           ,[isVisible]
           ,[FunctionalityName])
     VALUES
           ('ProductGroup RelatedProductGroup'
           ,'ProductGroup Related ProductGroup'
           ,1
           ,'ProductGroupRelatedProductGroup'
           ,'package'
           ,2
           ,'product-group-related-product-group'
           ,1
           ,'GetProductGroupRelatedProductGroup')
    print 'Inserted ProductGroup Related ProductGroup'
  end
else
  begin
    print 'ProductGroup Related ProductGroup already added'
  end


GO


