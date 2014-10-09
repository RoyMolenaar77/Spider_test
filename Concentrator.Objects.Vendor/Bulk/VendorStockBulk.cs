using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using System.Xml;
using System.IO;
using System.Data;
using System.Net;
using Concentrator.Objects.DataAccess.EntityFramework;
using Concentrator.Objects.Vendors.Base;
using System.Data.Objects;
using Concentrator.Objects.Models.Vendors;
using System.Xml.Serialization;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Products;

namespace Concentrator.Objects.Vendors.Bulk
{
  public class VendorStockBulk : VendorImportLoader<ConcentratorDataContext>
  {
    private int VendorID { get; set; }
    private int DefaultVendorID { get; set; }

    IEnumerable<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock> _stock = null;
    private string _vendorStockImportTableName = "[VendorTemp].[VendorStockImport_VendorStock_{0}]";
    private string _vendorStockTableQuery = @"CREATE TABLE {0} (
	                                            [VendorID] [int] NOT NULL,
	                                            [DefaultVendorID] [int] NOT NULL,
                                              [CustomItemNumber] [nvarchar](255) NOT NULL,
	                                            [QuantityOnHand] [int] NOT NULL,
	                                            [StockType] [nvarchar](50) NULL, 
                                              [StockStatus] [nvarchar](50) NULL) 
                                              ON [PRIMARY]";


    public VendorStockBulk(IEnumerable<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock> assortment, int vendorID, int? defaultVendorid)
    {
      DefaultVendorID = defaultVendorid.HasValue ? defaultVendorid.Value : vendorID;
      VendorID = vendorID;
      _stock = assortment;

      _vendorStockImportTableName = string.Format(_vendorStockImportTableName, vendorID);
      _vendorStockTableQuery = string.Format(_vendorStockTableQuery, _vendorStockImportTableName);
    } 

    public override void Init(ConcentratorDataContext context)
    {
      base.Init(context);

      try
      {
        context.ExecuteStoreCommand(_vendorStockTableQuery);

        using (GenericCollectionReader<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock> reader = new GenericCollectionReader<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock>(_stock))
        {
          BulkLoad(_vendorStockImportTableName, 1000, reader);
        }
      }
      catch (Exception e)
      {
        _log.Error("Error execture bulk copy", e);
      }
    }

    public override void Sync(ConcentratorDataContext context)
    {
      Log.DebugFormat("Start merge vendorstock for vendor {0}", VendorID);

      ///Update the vendorIDs of the temp table. Neccessary for performance.
      string updateVendorIDOfTable = string.Format("update {0} set vendorid = {1}, defaultvendorid = {2}", _vendorStockImportTableName, VendorID, DefaultVendorID);
      context.ExecuteStoreCommand(updateVendorIDOfTable);


      string addStockStatus = String.Format(@"Merge vendorproductstatus vps
Using (select distinct defaultvendorID,stockstatus from {0}) vvps
on vps.vendorID = vvps.defaultvendorID and vps.vendorstatus = vvps.stockstatus
WHEN Not Matched By Target
  THEN
      Insert (vendorID,vendorstatus,concentratorstatusID)
      values (vvps.defaultvendorID,vvps.stockstatus,-1);", _vendorStockImportTableName);
      context.ExecuteStoreCommand(addStockStatus);

      string addVendorStrockImportField = String.Format("ALTER TABLE {0} ADD ImportID int identity(1,1) PRIMARY KEY;", _vendorStockImportTableName);
      context.ExecuteStoreCommand(addVendorStrockImportField);

      string removeDuplicateImportVendorStocks = String.Format(@"delete T1
from {0} T1, {0} T2
where T1.customitemnumber = T2.customitemnumber
and T1.StockType = T2.StockType
and T1.ImportID > T2.ImportID", _vendorStockImportTableName);
      context.ExecuteStoreCommand(removeDuplicateImportVendorStocks);

      context.ExecuteStoreCommand(string.Format(@"alter table {0} add productid int null", _vendorStockImportTableName));

      context.ExecuteStoreCommand(string.Format(@"update vs
	  set vs.productid = va.productid
	  from {0} vs , vendorassortment va 
	  where vs.CustomItemNumber = va.CustomItemNumber", _vendorStockImportTableName));

      string vendorStockMerge = (String.Format(@"MERGE 
vendorStock vp
USING
  (select vvp.productid,vvp.vendorid,vvp.quantityOnHand,pvs.concentratorstatusID,vst.vendorstocktypeID,vvp.stockstatus
from {0} vvp
left join VendorProductStatus pvs on pvs.vendorstatus = vvp.stockstatus and pvs.vendorid = vvp.DefaultVendorID and pvs.concentratorstatusID > 0
left join VendorStockTypes vst on vst.stocktype = vvp.stocktype
where vvp.productid is not null
) vvp
ON vp.productID = vvp.productID and vp.vendorstocktypeID = vvp.vendorstocktypeID and vp.vendorID = vvp.vendorID
WHEN Matched 
THEN UPDATE SET 
	vp.quantityOnHand = vvp.quantityOnHand,
	vp.ConcentratorStatusID = vvp.concentratorstatusID,
	vp.stockstatus = vvp.stockstatus
WHEN Not Matched By Target
  THEN
      Insert (ProductID,VendorID,QuantityOnHand,VendorStockTypeID,StockStatus,ConcentratorStatusID)
      values (vvp.productID,vvp.vendorid,vvp.quantityOnHand,vvp.vendorstocktypeid,vvp.stockstatus,vvp.concentratorstatusID);", _vendorStockImportTableName));

      context.ExecuteStoreCommand(vendorStockMerge);
      Log.DebugFormat("Finish merge vendorStock for vendor {0}", VendorID);
    }

    protected override void TearDown(ConcentratorDataContext context)
    {
      try { context.ExecuteStoreCommand(String.Format("DROP TABLE {0}", _vendorStockImportTableName)); }
      catch { }
    }
  }
}
