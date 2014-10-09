INSERT INTO ManagementGroup ([Group],PortalID,DashboardName)
values ('Content',null,null)

INSERT INTO [Concentrator_staging].[dbo].[ManagementPage]
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
           ('Vendor free goods','Vendor free goods',1,'VendorFreeGoods','package',1,'vendor-free-goods',1,'ViewVendors'),
           ('Vendor accruels','Vendor accruels',1,'VendorAccruels','bolt',1,'vendor-accruels',1,'ViewVendors'),
('Product media','Product media',1,'ProductMedia','package',2,'product-media',1,'ViewProducts'),
('Content','Content',1,'Content','books',10,'content-item',1,'Default')




