using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using Concentrator.Objects.Extensions;
using Concentrator.Objects.Ftp;

using Concentrator.Objects.ZipUtil;
using Concentrator.Objects.Utility;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Attributes;

namespace Concentrator.Plugins.VSN
{
  public class ProductImportFull : VSNProductImportBase
  {
    private const string _name = "VSN Content Import Plugin (Full)";

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


      var ftp = new FtpManager(config["VSNFtpUrl"].Value, "pub3/", config["VSNUser"].Value, config["VSNPassword"].Value, false, true, log);


      using (var unit = GetUnitOfWork())
      {
        //DataLoadOptions options = new DataLoadOptions();
        //options.LoadWith<Product>(p => p.ProductImages);
        //options.LoadWith<Product>(p => p.ProductDescription);
        //options.LoadWith<Product>(p => p.ProductBarcodes);
        //options.LoadWith<Product>(p => p.VendorAssortment);
        //options.LoadWith<VendorAssortment>(va => va.VendorPrice);
        //options.LoadWith<VendorAssortment>(va => va.VendorStock);

        //options.LoadWith<ProductAttributeMetaData>(p => p.ProductAttributeValues);
        //options.LoadWith<ProductGroupVendor>(x=>x.VendorProductGroupAssortments);
        //options.LoadWith<VendorAssortment>(x => x.VendorPrice);
        //options.LoadWith<VendorAssortment>(va => va.VendorStock);
        
        //ctx.LoadOptions = options;
        ProductStatusVendorMapper mapper = new ProductStatusVendorMapper(unit.Scope.Repository<VendorProductStatus>(), VendorID);
        ProductGroupSyncer syncer = new ProductGroupSyncer(VendorID, unit.Scope.Repository<ProductGroupVendor>());

        BrandVendor brandVendor = null;
        List<ProductAttributeMetaData> attributes;
        SetupBrandAndAttributes(unit, out attributes, out brandVendor);

//#if DEBUG
//        if (File.Exists("ExportProduct.xml"))
//        {
//          DataSet ds2 = new DataSet();
//          ds2.ReadXml("ExportProduct.xml");

//          ProcessProductsTable(ds2.Tables[0], brandVendor.BrandID, attributes, syncer, mapper);
//        }
//#endif
        var inactiveAss = unit.Scope.Repository<VendorAssortment>().GetAll(x => x.VendorID == VendorID).ToDictionary(x => x.VendorAssortmentID, y => y);

        using (var prodFile = ftp.OpenFile("XMLExportProduct.zip"))
        {
          using (var zipProc = new ZipProcessor(prodFile.Data))
          {
            foreach (var file in zipProc)
            {
              using (file)
              {
#if DEBUG
                using( System.IO.FileStream fs =new FileStream(file.FileName, FileMode.OpenOrCreate))
                {
                  file.Data.WriteTo(fs);
                  fs.Close();
                }
                
#endif
                using (DataSet ds = new DataSet())
                {
                  ds.ReadXml(file.Data);

                  ProcessProductsTable(ds.Tables[0], brandVendor.BrandID, attributes, syncer, mapper, inactiveAss);
                }
              }
            }
          }
        }

        inactiveAss.Values.ForEach((x, idx) =>
        {
          x.IsActive = false;
        });
        unit.Save();
      }
      log.AuditComplete("Finished full VSN product import", "VSN Product Import");
    }

    protected override void SyncProducts()
    {
      Process();
    }
  }
}
