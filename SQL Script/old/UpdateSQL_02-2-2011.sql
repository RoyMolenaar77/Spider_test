USE [Concentrator_staging]
GO
/****** Object:  StoredProcedure [dbo].[sp_GetMatchedProducts]    Script Date: 02/03/2011 16:58:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Concentrator
-- Create date: 
-- Description:	
-- =============================================
ALTER PROCEDURE [dbo].[sp_GetMatchedProducts] 
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

declare @matched table (
	ProductID int not null,
	VendorItemNumber nvarchar(100) null,
	BrandID int not null ,
	Barcode nvarchar(100) null,
	
	CProductID int not null,
	CVendorItemNumber nvarchar(100) null,
	CBrandID int not null ,
	CBarcode nvarchar(100) null,
	MatchPercentage int null
	
)

--rule 1
insert into @matched (ProductID, VendorItemNumber, BrandID, Barcode, CBarcode, CProductID, CBrandID, CVendorItemNumber, matchPercentage)
select p.ProductID, p.VendorItemNumber, p.BrandID, pb.Barcode, pb2.Barcode as CBarcode, pb2.ProductID as CProductID, pb2.BrandID as CBrandID, pb2.VendorItemNumber as CVendorItemNumber, 100
from Product p 
inner join ProductBarcode pb on p.ProductID = pb.ProductID
inner join (Select pb12.ProductID,pb12.Barcode,p12.BrandID,p12.VendorItemNumber from ProductBarcode pb12
			inner join product p12 on pb12.ProductID = p12.ProductID) pb2 on pb2.VendorItemNumber = p.VendorItemNumber AND PB2.ProductID != pb.ProductID and pb2.BrandID = p.BrandID

insert into @matched (ProductID, VendorItemNumber, BrandID, Barcode, CBarcode, CProductID, CBrandID, CVendorItemNumber, matchPercentage)
select p.ProductID, p.VendorItemNumber, p.BrandID, pb.Barcode, pb2.Barcode as CBarcode, pb2.ProductID as CProductID, pb2.BrandID as CBrandID, pb2.VendorItemNumber as CVendorItemNumber, -1
from Product p 
inner join ProductBarcode pb on p.ProductID = pb.ProductID
inner join (Select pb12.ProductID,pb12.Barcode,p12.BrandID,p12.VendorItemNumber from ProductBarcode pb12
			inner join product p12 on pb12.ProductID = p12.ProductID) pb2 on pb2.Barcode = pb.Barcode AND PB2.ProductID != pb.ProductID and '%'+pb2.VendorItemNumber+'%' like '%'+ p.VendorItemNumber + '%'

insert into @matched (ProductID, VendorItemNumber, BrandID, Barcode, CBarcode, CProductID, CBrandID, CVendorItemNumber, matchPercentage)
select p.ProductID, p.VendorItemNumber, p.BrandID, pb.Barcode, pb2.Barcode as CBarcode, pb2.ProductID as CProductID, pb2.BrandID as CBrandID, pb2.VendorItemNumber as CVendorItemNumber, 75
from Product p 
inner join ProductBarcode pb on p.ProductID = pb.ProductID
inner join (Select pb12.ProductID,pb12.Barcode,p12.BrandID,p12.VendorItemNumber from ProductBarcode pb12
			inner join product p12 on pb12.ProductID = p12.ProductID) pb2 on pb2.Barcode = pb.Barcode AND PB2.ProductID != pb.ProductID and pb2.BrandID = p.BrandID



insert into @matched (ProductID, VendorItemNumber, BrandID, Barcode, CBarcode, CProductID, CBrandID, CVendorItemNumber, matchPercentage)
select p.ProductID, p.VendorItemNumber, p.BrandID, pb.Barcode, pb2.Barcode as CBarcode, pb2.ProductID as CProductID, pb2.BrandID as CBrandID, pb2.VendorItemNumber as CVendorItemNumber, 50
from Product p 
inner join ProductBarcode pb on p.ProductID = pb.ProductID
inner join (Select pb12.ProductID,pb12.Barcode,p12.BrandID,p12.VendorItemNumber from ProductBarcode pb12
			inner join product p12 on pb12.ProductID = p12.ProductID) pb2 on pb2.Barcode = pb.Barcode AND PB2.ProductID != pb.ProductID and pb2.VendorItemNumber = p.VendorItemNumber and p.BrandID != pb2.BrandID 


MERGE @matched as m 
USING (
  select ProductID, CProductID, count(*) as CountDuplicate
from @matched
group by ProductID, CProductID
having COUNT(*) > 1
 ) as mm
ON (mm.productid = m.productid and mm.cproductid = m.cproductid ) --and mm.dupl = m.matchpercentage
WHEN NOT MATCHED BY SOURCE
  then delete
    ;

select distinct ProductID,VendorItemNumber, BrandID, Barcode, CProductID, CVendorItemNumber, CBrandID, CBarcode, MatchPercentage
from @matched




    
END



alter table productmatch add MatchPercentage int null


alter table productbarcode add VendorID int null
Alter table productbarcode add constraint FK_Vendor_ProductBarcode foreign key (VendorID) references vendor (vendorid)

alter table productmatch add CalculatedMatch bit null

alter table productmatch alter column vendorid int null


GO
CREATE NONCLUSTERED INDEX [VPN]
ON [dbo].[ProductMatch] ([CorrespondingProductID])
INCLUDE ([ProductID],[VendorItemNumber])
GO

