using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Ftp;
using Concentrator.Objects;
using System.IO;
using System.Xml.Linq;
using System.Data;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.Plugins.VSN
{
  class SupplierNamesImport : ConcentratorPlugin
  {

    public override string Name
    {
      get { return "VSN SupplierNames Import Plugin"; }
    }

    protected override void Process()
    {
      #region Ftp connection


      var config = GetConfiguration().AppSettings.Settings;

      var ftpFile1 = new FtpManager(config["VSNFtpUrl"].Value, "pub4/ExportSupplierNames.xml",
              config["VSNUser"].Value, config["VSNPassword"].Value, false, true, log).FirstOrDefault();

      var ftpFile2 = new FtpManager(config["VSNFtpUrl"].Value, "pub4/ExportSupplier.xml",
              config["VSNUser"].Value, config["VSNPassword"].Value, false, true, log).FirstOrDefault();

      var vendorID = int.Parse(config["VendorID"].Value);

      #endregion
      //ExportStock.xml
      //ExportSupplier.xml
      //ExportSupplierNames.xml

      #region Data process
      using (var unit = GetUnitOfWork())
      {
        List<FtpManager.RemoteFile> files = new List<FtpManager.RemoteFile>();

        files.Add(ftpFile2);
        files.Add(ftpFile1);

        ProcessFile(files, unit, vendorID);
        unit.Save();
      }
      log.AuditComplete("Finished VSN SupplierNames Import Plugin", "VSN SupplierNames Import Plugin");

      #endregion


    }

    private void ProcessFile(List<FtpManager.RemoteFile> files, IUnitOfWork unit, int vendorID)
    {
      #region Fill DataSet
      using (DataSet ds = new DataSet())
      {
        ds.ReadXml(files[0].Data);
        using (DataSet ds2 = new DataSet())
        {
          ds2.ReadXml(files[1].Data);

          var ProductCodes = (from data in ds.Tables[0].Rows.Cast<DataRow>()
                              select new
                              {
                                ProductCode = data.Field<string>("ProductCode").Trim(),
                                SupplierCode = data.Field<string>("SupplierCode").Trim()
                              }).ToList();

          var SupplierDataSet = (from data in ds2.Tables[0].Rows.Cast<DataRow>()
                                 //let productCode = ProductCodes.FirstOrDefault(c => c.SupplierCode == data.Element("SupplierCode").Value)
                                 select new
                                 {
                                   //ProductCode = productCode,
                                   SupplierCode = data.Field<string>("SupplierCode").Trim(),
                                   SupplierName = data.Field<string>("SupplierName").Trim()
                                 }).Distinct().ToList();

      #endregion

          #region Process
          var brandVendorRepo = unit.Scope.Repository<BrandVendor>();
          var assortmentRepo = unit.Scope.Repository<VendorAssortment>();

          foreach (var set in SupplierDataSet)
          {
            var BrandVendor = brandVendorRepo.GetSingle(bv => bv.VendorID == vendorID && bv.VendorBrandCode == set.SupplierCode);

            if (BrandVendor == null)
            {
              brandVendorRepo.Add(new BrandVendor
              {
                BrandID = -1,
                VendorID = vendorID,
                VendorBrandCode = set.SupplierCode,
                Name = set.SupplierName
              });
              unit.Save();
            }
          }

          //var Vendor = ctx.Vendors.Where(v => v.VendorID == 39).FirstOrDefault();
          var Products = assortmentRepo.GetAll(p => p.VendorID == vendorID).ToDictionary(x => x.CustomItemNumber, x => x.Product);
          var vendors = brandVendorRepo.GetAll(x => x.VendorID == vendorID && x.BrandID > 0).ToDictionary(x => x.VendorBrandCode, x => x.BrandID);

          foreach (var prod in Products.Keys)
          {
            var supplierproduct = ProductCodes.Where(x => x.ProductCode == prod).FirstOrDefault();

            if (supplierproduct != null)
            {
              if (vendors.ContainsKey(supplierproduct.SupplierCode))
              {
                Products[prod].BrandID = vendors[supplierproduct.SupplierCode];
                unit.Save();
              }
            }
          }


          #endregion
        }
      }
    }
  }
}
