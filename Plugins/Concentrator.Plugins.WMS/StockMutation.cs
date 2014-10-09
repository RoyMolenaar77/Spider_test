using System;
using System.Linq;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Vendors;
using System.IO;
using System.Xml.Linq;
using Concentrator.Objects.Models.Products;

namespace Concentrator.Plugins.WMS
{
  public class StockMutation : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "WMS stock mutations"; }
    }

    protected override void Process()
    {
      try
      {
        using (var unit = GetUnitOfWork())
        {
          var config = GetConfiguration();

          var vendorID = 0; //config.AppSettings.Settings["StockMutationVendorID"].Value;

          if (!int.TryParse(config.AppSettings.Settings["StockMutationVendorID"].Value, out vendorID))
            throw new Exception("Appsetting StockMutationVendorID missing in plugin config");

          var vendor = unit.Scope.Repository<Vendor>().GetSingle(x => x.VendorID == vendorID);

          var stockMutationPath = config.AppSettings.Settings["StockMutationPath"].Value;

          if (string.IsNullOrEmpty(stockMutationPath))
            throw new Exception("Empty stock mutation path for " + vendor.Name);

          int wmsStockLocation = int.Parse(config.AppSettings.Settings["WmsVendorStockTypeID"].Value);
          int transitStockLocation = int.Parse(config.AppSettings.Settings["TransitVendorStockTypeID"].Value);

          var cmStockType = unit.Scope.Repository<VendorStockType>().GetSingle(x => x.StockType == "CM");
          if (cmStockType == null)
            throw new Exception("Stocklocation CM does not exists, skip order process");

          var transferStockType = unit.Scope.Repository<VendorStockType>().GetSingle(x => x.StockType == "Transfer");
          if (transferStockType == null)
            throw new Exception("Stocklocation Transfer does not exists, skip order process");

          var webShopStockType = unit.Scope.Repository<VendorStockType>().GetSingle(x => x.StockType == "Webshop");
          if (webShopStockType == null)
            throw new Exception("Stocklocation Webshop does not exists, skip order process");

          Directory.GetFiles(stockMutationPath).ForEach((file, idx) =>
          {
            try
            {
              XDocument XmlDocument = XDocument.Parse(System.IO.File.ReadAllText(file));

              var mutations = (from r in XmlDocument.Root.Elements("Mutation")
                               select new
                               {
                                 VendorItemNumber = r.Element("ProductID").Value,
                                 TransitStock = bool.Parse(r.Element("TransitStock").Value),
                                 Quantity = int.Parse(r.Element("Quantity").Value)
                               });

              mutations.ForEach((mutation, ridx) =>
              {
                var wmsVendorstockRecord = unit.Scope.Repository<VendorStock>().GetSingle(x => x.Product.VendorItemNumber == mutation.VendorItemNumber && x.VendorStockTypeID == wmsStockLocation);

                if (wmsVendorstockRecord == null)
                {
                  var product = unit.Scope.Repository<Product>().GetSingle(x => x.VendorItemNumber == mutation.VendorItemNumber);

                  if (product != null)
                  {

                    wmsVendorstockRecord = new VendorStock()
                    {
                      VendorStockTypeID = wmsStockLocation,
                      VendorID = vendor.VendorID,
                      ProductID = product.ProductID,
                      QuantityOnHand = 0
                    };
                    unit.Scope.Repository<VendorStock>().Add(wmsVendorstockRecord);
                    unit.Save();

                  }
                  else
                    log.AuditInfo("Received mutation for non existing product");
                }

                if (wmsVendorstockRecord != null)
                {
                  wmsVendorstockRecord.QuantityOnHand = wmsVendorstockRecord.QuantityOnHand + mutation.Quantity;

                  var cmStock = unit.Scope.Repository<VendorStock>().GetSingle(x => x.Product.VendorItemNumber == mutation.VendorItemNumber && x.VendorStockTypeID == cmStockType.VendorStockTypeID);
                  var cmStockOnHand = 0;
                  if (cmStock != null)
                    cmStockOnHand = cmStock.QuantityOnHand;

                  var transitVendorstockRecord = unit.Scope.Repository<VendorStock>().GetSingle(x => x.Product.VendorItemNumber == mutation.VendorItemNumber && x.VendorStockTypeID == transferStockType.VendorStockTypeID);
                  if (mutation.TransitStock)
                  { //In transit stock mutations need to be moved from transit to wms location
                    //var transitVendorstockRecord = unit.Scope.Repository<VendorStock>().GetSingle(x => x.Product.VendorItemNumber == mutation.VendorItemNumber && x.VendorStockTypeID == transitStockLocation);
                    if (transitVendorstockRecord == null)
                    {
                      var product = unit.Scope.Repository<Product>().GetSingle(x => x.VendorItemNumber == mutation.VendorItemNumber);

                      if (product != null)
                      {
                        transitVendorstockRecord = new VendorStock()
                        {
                          VendorStockTypeID = transferStockType.VendorStockTypeID,
                          VendorID = vendor.VendorID,
                          ProductID = product.ProductID,
                          QuantityOnHand = 0
                        };
                        unit.Scope.Repository<VendorStock>().Add(transitVendorstockRecord);
                        unit.Save();
                      }
                      else
                        log.AuditInfo("Received mutation for non existing product");
                    }

                    if (transitVendorstockRecord != null && transitVendorstockRecord.QuantityOnHand > 0)
                    {
                      transitVendorstockRecord.QuantityOnHand = transitVendorstockRecord.QuantityOnHand - mutation.Quantity;
                    }
                  }

                  var transferStockOnHand = 0;
                  if (transitVendorstockRecord != null)
                    transferStockOnHand = transitVendorstockRecord.QuantityOnHand;

                  var webshopStock = unit.Scope.Repository<VendorStock>().GetSingle(x => x.Product.VendorItemNumber == mutation.VendorItemNumber && x.VendorStockTypeID == webShopStockType.VendorStockTypeID);
                  if (webshopStock == null)
                  {
                    var product = unit.Scope.Repository<Product>().GetSingle(x => x.VendorItemNumber == mutation.VendorItemNumber);

                    if (product != null)
                    {
                      webshopStock = new VendorStock()
                      {
                        VendorStockTypeID = wmsStockLocation,
                        VendorID = vendor.VendorID,
                        ProductID = product.ProductID,
                        QuantityOnHand = 0
                      };
                      unit.Scope.Repository<VendorStock>().Add(webshopStock);
                      unit.Save();
                    }
                    else
                      log.AuditInfo("Received mutation for non existing product");
                  }

                  webshopStock.QuantityOnHand = cmStockOnHand + transferStockOnHand + wmsVendorstockRecord.QuantityOnHand;
                }
              });

              unit.Save();
            }
            catch (Exception ex)
            {
              log.Error("Error process dispatch " + vendor.Name, ex);
            }

            FileInfo finf = new FileInfo(file);

            string archivePath = Path.Combine(stockMutationPath, "Archive");

            if (!Directory.Exists(archivePath))
              Directory.CreateDirectory(archivePath);
            var path = Path.Combine(archivePath, DateTime.Now.ToString("yyyyMMddHHmmss") + finf.Extension);

            if (File.Exists(path))
            {
              File.Delete(path);
            }

            File.Move(file, path);

          });
        }
      }
      catch (Exception ex)
      {
        log.AuditError("Error importing stock mutations WMS", ex);
      }
    }
  }
}
