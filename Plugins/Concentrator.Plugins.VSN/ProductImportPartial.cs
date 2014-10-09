using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Linq;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using Concentrator.Objects.Ftp;
using Concentrator.Objects.ZipUtil;
using Concentrator.Objects.Extensions;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Utility;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Attributes;

namespace Concentrator.Plugins.VSN
{
  public class ProductImportPartial : VSNProductImportBase
  {
    private const string _name = "VSN Content Import Plugin (Partial)";

    protected override Configuration Config
    {
      get { return GetConfiguration(); }
    }

    protected override int VendorID
    {
      get { return int.Parse(Config.AppSettings.Settings["VendorID"].Value); }
    }

    protected override int DefaultVendorID
    {
      get { return int.Parse(Config.AppSettings.Settings["DefaultVendorID"].Value); }
    }

    public override string Name
    {
      get { return _name; }
    }

    public void Process()
    {
      var config = GetConfiguration().AppSettings.Settings;

      var ftp = new FtpManager(config["VSNFtpUrl"].Value, "pub3/",
        config["VSNUser"].Value, config["VSNPassword"].Value, false, true, log);

      using (var unit = GetUnitOfWork())
      {
        //DataLoadOptions options = new DataLoadOptions();
        //options.LoadWith<Product>(p => p.Media);
        //options.LoadWith<Product>(p => p.ProductDescription);
        //options.LoadWith<Product>(p => p.ProductBarcodes);
        //options.LoadWith<Product>(p => p.VendorAssortment);
        //options.LoadWith<VendorAssortment>(va => va.VendorPrice);
        //options.LoadWith<VendorAssortment>(va => va.VendorStock);
        //ctx.LoadOptions = options;

        ProductGroupSyncer syncer = new ProductGroupSyncer(VendorID, unit.Scope.Repository<ProductGroupVendor>());
        ProductStatusVendorMapper statusMapper = new ProductStatusVendorMapper(unit.Scope.Repository<VendorProductStatus>(), VendorID);

        BrandVendor brandVendor = null;
        List<ProductAttributeMetaData> attributes;
        SetupBrandAndAttributes(unit, out attributes, out brandVendor);

        using (var prodFile = ftp.OpenFile("7ExportProduct.xml"))
        {
          using (DataSet ds = new DataSet())
          {
            ds.ReadXml(prodFile.Data);

            ProcessProductsTable(ds.Tables[0], brandVendor.BrandID, attributes, syncer, statusMapper, null);
          }
        }
      }
      log.AuditComplete("Finished partial VSN product import", "VSN Product Import");
    }

    protected override void SyncProducts()
    {
      Process();
    }
  }
}
