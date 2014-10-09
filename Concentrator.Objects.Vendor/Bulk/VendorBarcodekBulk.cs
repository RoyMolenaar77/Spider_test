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
  public class VendorBarcodeBulk : VendorImportLoader<ConcentratorDataContext>
  {
    #region SQL strings

    private int VendorID { get; set; }
    private int DefaultVendorID { get; set; }
    private int BarcodeType { get; set; }

    private string __vendorBarcodeImportTableName = "[VendorTemp].[VendorImport_VendorBarcode_{0}]";

    private string __vendorBarcodeTableQuery = @"CREATE TABLE {0} (
	[VendorID] [int] NOT NULL,
  [CustomItemNumber] [nvarchar](50) NOT NULL,
  [Barcode] [nvarchar](50) NOT NULL,
  [Type] [int] NOT NULL
) ON [PRIMARY]";


    #endregion

    IEnumerable<VendorImportBarcode> _barcodes = null;
    public VendorBarcodeBulk(IEnumerable<VendorImportBarcode> barcodes, int vendorID, int barcodeType)
    {
      _barcodes = barcodes;

      VendorID = vendorID;
      BarcodeType = barcodeType;
      __vendorBarcodeImportTableName = string.Format(__vendorBarcodeImportTableName, vendorID);
      __vendorBarcodeTableQuery = string.Format(__vendorBarcodeTableQuery, __vendorBarcodeImportTableName);
    }

    public override void Init(ConcentratorDataContext context)
    {
      base.Init(context);

      try
      {
        context.ExecuteStoreCommand(__vendorBarcodeTableQuery);

        using (GenericCollectionReader<VendorImportBarcode> reader = new GenericCollectionReader<VendorImportBarcode>(_barcodes))
        {
          BulkLoad(__vendorBarcodeImportTableName, 500, reader);
        }
      }
      catch
      {
        _log.Error("Error execture bulk copy");
      }
    }

    public override void Sync(ConcentratorDataContext context)
    {
      #region barcodes
      Log.DebugFormat("Start merge Barcodes for vendor {0}", VendorID);
      string mergeVendorBarcodes = (String.Format(@"merge productbarcode pb
using (
select distinct productid,pb.VendorID,barcode,pb.type
from {0} pb
inner join Vendor v on v.ParentVendorID = pb.VendorID or v.VendorID = pb.VendorID
inner join VendorAssortment va on va.CustomItemNumber = pb.CustomItemNumber and v.VendorID = va.VendorID
) as ass on ass.productid = pb.productid and ass.barcode = pb.barcode
WHEN MATCHED and pb.vendorid is null or pb.vendorid <> {1}
then update set pb.vendorid = ass.vendorid
WHEN NOT Matched by target
THEN
INSERT (productid,barcode,vendorid,barcodetype)
values (ass.productid,ass.barcode,ass.vendorid,ass.type)
WHEN NOT Matched by source and pb.vendorid = {1} and pb.BarcodeType = {2} and pb.productid in (select distinct productid from VendorAssortment va
 inner join Vendor v on va.VendorID = v.VendorID
 where v.ParentVendorID = {1} or v.VendorID = {1})
THEN delete;", __vendorBarcodeImportTableName, VendorID, BarcodeType));
      context.ExecuteStoreCommand(mergeVendorBarcodes);

//      string newBrands = (String.Format(@"
//INSERT INTO BrandVendor (BrandID,VendorBrandCode,VendorID)
//select distinct -1,Vendorbrandcode,defaultvendorid
//from {0}
//where brandid is null", __vendorStockImportTableName));
//      context.ExecuteStoreCommand(newBrands);
      Log.DebugFormat("Finish merge Barcodes for vendor {0}", VendorID);
      #endregion
    }

    protected override void TearDown(ConcentratorDataContext context)
    {
      context.ExecuteStoreCommand(String.Format("DROP TABLE {0}", __vendorBarcodeImportTableName));
      //File.Delete(LocalCacheLocation);
    }

    public class VendorImportBarcode
    {
      public int VendorID { get; set; }
      public string CustomItemNumber { get; set; }
      public string Barcode { get; set; }
      public int Type { get; set; }
    }
  }
}
