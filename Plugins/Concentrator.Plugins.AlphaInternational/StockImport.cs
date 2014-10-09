using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Concentrator.Objects;
using Concentrator.Objects.Ftp;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Vendors;
using System.Configuration;

namespace Concentrator.Plugins.AlphaInternational
{
  class StockImport : VendorBase
  {
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
      get { return "Alpha International Stock Import Plugin"; }
    }

    //private const int UnMappedID = -1;
    
    protected override void SyncProducts()
    {
      var config = GetConfiguration();

      FtpManager downloader = new FtpManager(
        config.AppSettings.Settings["AlphaFtpUrl"].Value,
        config.AppSettings.Settings["AlphaStockPath"].Value,
        config.AppSettings.Settings["AlphaUserName"].Value,
        config.AppSettings.Settings["AlphaPassword"].Value,
       false, true, log);//new FtpDownloader("test/");

      log.AuditInfo("Starting stock import process");

      using (var unit = GetUnitOfWork())
      {

        // log.DebugFormat("Found {0} files for Alpha process", downloader.Count());

        foreach (var file in downloader)
        {
          if (!Running)
            break;

          log.InfoFormat("Processing file: {0}", file.FileName);

          XDocument doc = null;
          using (file)
          {
            try
            {
              using (var reader = XmlReader.Create(file.Data))
              {
                reader.MoveToContent();
                doc = XDocument.Load(reader);
                if (ProcessXML(doc, unit.Scope))
                {
                  log.InfoFormat("Succesfully processed file: {0}", file.FileName);
                  downloader.Delete(file.FileName);
                }
                else
                {
                  log.InfoFormat("Failed to process file: {0}", file.FileName);
                  downloader.MarkAsError(file.FileName);
                }
              }
            }
            catch (Exception ex)
            {
              log.AuditError(String.Format("Failed to load xml for file: {0}", file.FileName), ex);
              downloader.MarkAsError(file.FileName);
              continue;
            }
          }
        }
        unit.Save();
      }
    }

    private bool ProcessXML(XDocument doc, IScope scope)
    {
      var repoAssortment = scope.Repository<VendorAssortment>();
      var repoStock = scope.Repository<VendorStock>();

      var stockData = (from data in doc.Elements("Products").Elements("Product")
                       select new
                                {
                                  CustomItemNumber = data.Attribute("id").Value,
                                  Stock = data.Element("Purchase").Element("Stock").Value
                                }).ToList();

      foreach (var productStock in stockData)
      {
        //get productID in vendorStockAssortment
        var productID = repoAssortment.GetSingle(vs => vs.CustomItemNumber == productStock.CustomItemNumber && vs.VendorID == VendorID).Try<VendorAssortment, int?>(c => c.ProductID, null);

        //if found
        if (productID.HasValue && productID.Value > 0)
        {
          var vendorStock = repoStock.GetSingle(st => st.ProductID == productID);

          try
          {
            if (vendorStock == null)
            {
              vendorStock = new VendorStock
              {
                VendorID = VendorID,
                ProductID = productID.Value,
                VendorStockTypeID = 1
              };
              repoStock.Add(vendorStock);
            }
            vendorStock.QuantityOnHand = Int32.Parse(productStock.Stock);
            vendorStock.ConcentratorStatusID = -1;


          }
          catch (Exception ex)
          {
            log.AuditError(string.Format("Error update stock for product {0}, error {1}", productID, ex.InnerException));
            return false;
          }
        }
      }
      return true;
    }
  }
}
