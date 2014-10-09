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
using System.Data.Objects;
using System.Xml.Serialization;
using Concentrator.Objects.Vendors.Base;

namespace Concentrator.Plugins.BAS
{
  public class VendorRetailStockBulk : VendorImportLoader<ConcentratorDataContext>
  {
    #region SQL strings

    private XmlDocument _assortmentXml;
    private int VendorID { get; set; }
    private int DefaultVendorID { get; set; }

    private string __vendorStockImportTableName = "[VendorTemp].[VendorImport_VendorRetailStock_{0}]";

    private string __vendorStockTableQuery = @"CREATE TABLE {0} (
	[VendorBackendCode] [nvarchar](50) NOT NULL,
	[QuantityOnHand] [int] NOT NULL,
	[VendorStockType] [nvarchar](50) NOT NULL,
	[defaultVendorID] [int] NULL,
  [CustomItemNumber] [nvarchar](50) NULL
) ON [PRIMARY]";


    #endregion

    IEnumerable<VendorImportRetailStock> _stockAssortment = null;
    public VendorRetailStockBulk(IEnumerable<VendorImportRetailStock> stockAssortment, int vendorID, int? defaultVendorid, string plugin)
    {
      _stockAssortment = stockAssortment;

      DefaultVendorID = defaultVendorid.HasValue ? defaultVendorid.Value : vendorID;

      VendorID = vendorID;
      __vendorStockImportTableName = string.Format(__vendorStockImportTableName, plugin + "_" + vendorID);
      __vendorStockTableQuery = string.Format(__vendorStockTableQuery, __vendorStockImportTableName);
    }

    public override void Init(ConcentratorDataContext context)
    {
      base.Init(context);

      try
      {
        context.ExecuteStoreCommand(__vendorStockTableQuery);

        using (GenericCollectionReader<VendorImportRetailStock> reader = new GenericCollectionReader<VendorImportRetailStock>(_stockAssortment))
        {
          BulkLoad(__vendorStockImportTableName, 500, reader);
        }
      }
      catch (Exception ex)
      {
        _log.Error("Error execture bulk copy");
      }
    }

    public override void Sync(ConcentratorDataContext context)
    {
      #region brands
      Log.DebugFormat("Start merge Stock for vendor {0}", VendorID);
      string mergeVendorStock = (String.Format(@"merge vendorstock vs
using(
select
p.productid,
v.vendorid,
min(vs.quantityonhand) as quantityonhand,
vst.vendorstocktypeid
from {0} vs
inner join vendormapping vm on vm.vendorid = vs.defaultvendorid
inner join vendor v on vm.mapvendorID = v.vendorid and v.backendvendorcode = vs.vendorbackendcode
inner join vendorassortment p on p.customitemnumber = vs.customitemnumber and p.vendorid = vs.defaultvendorid
inner join VendorStockTypes vst on vs.vendorstocktype = vst.stocktype
group by p.productid,v.vendorid,vst.vendorstocktypeid
) as vass
on vs.productid = vass.productid and vs.vendorid = vass.vendorid and vs.vendorstocktypeid = vass.vendorstocktypeid
WHEN MATCHED
then 
	update set 
		vs.quantityOnHand = vass.quantityOnHand
WHEN NOT Matched by target
THEN
INSERT ([ProductID],[VendorID],[QuantityOnHand],[VendorStockTypeID])
values (vass.[ProductID],vass.[VendorID],vass.[QuantityOnHand],vass.[VendorStockTypeID])
WHEN NOT Matched by source and vs.vendorid in (select distinct v.VendorID
from {0} vvbr
inner join Vendor v on v.backendvendorcode = vvbr.vendorbackendcode
inner join VendorMapping vm on vm.MapVendorID = v.VendorID and vm.VendorID = vvbr.defaultvendorid)
THEN delete;", __vendorStockImportTableName));
      context.ExecuteStoreCommand(mergeVendorStock);

      //      string newBrands = (String.Format(@"
      //INSERT INTO BrandVendor (BrandID,VendorBrandCode,VendorID)
      //select distinct -1,Vendorbrandcode,defaultvendorid
      //from {0}
      //where brandid is null", __vendorStockImportTableName));
      //      context.ExecuteStoreCommand(newBrands);
      Log.DebugFormat("Finish merge stock for vendor {0}", VendorID);
      #endregion
    }

    protected override void TearDown(ConcentratorDataContext context)
    {
      context.ExecuteStoreCommand(String.Format("DROP TABLE {0}", __vendorStockImportTableName));
      //File.Delete(LocalCacheLocation);
    }

    public class VendorImportRetailStock
    {
      public string VendorBackendCode { get; set; }

      public int QuantityOnHand { get; set; }

      public string VendorStockType { get; set; }

      public int defaultVendorID { get; set; }

      public string CustomItemNumber { get; set; }
    }
  }
}
