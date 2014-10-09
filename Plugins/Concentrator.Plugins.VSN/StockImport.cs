using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using Concentrator.Objects.Ftp;
using Concentrator.Objects.ConcentratorService;
using System.Xml;
using System.Data;
using System.IO;
using System.Xml.Linq;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.Plugins.VSN
{
  class StockImport : ConcentratorPlugin
  {
    private int vendorID;

    public override string Name
    {
      get { return "VSN Stock import Plugin"; }
    }

    protected override void Process()
    {
      #region Ftp connection


      var config = GetConfiguration().AppSettings.Settings;

      var ftp = new FtpManager(config["VSNFtpUrl"].Value, "pub4/",
              config["VSNUser"].Value, config["VSNPassword"].Value, false, false, log);

      vendorID = int.Parse(config["VendorID"].Value);

      #endregion
      //ExportStock.xml
      //ExportSupplier.xml
      //ExportSupplierNames.xml

      #region Data process
      using (var unit = GetUnitOfWork())
      {
        using (var prodFile = ftp.OpenFile("ExportStock.xml"))
        {
          ProcessFile(prodFile, unit);
          unit.Save();
        }
      }
      log.AuditComplete("Finished VSN Stock import Plugin", "VSN Stock import Plugin");

      #endregion
    }

    private void ProcessFile(FtpManager.RemoteFile file, IUnitOfWork unit)
    {
      #region Fill DataSet
      //StreamReader reader = new StreamReader(file.Data);
      Dictionary<string, long> stockData = new Dictionary<string, long>();

      using (DataSet ds = new DataSet())
      {
        ds.ReadXml(file.Data);

        stockData = (from data in ds.Tables[0].Rows.Cast<DataRow>()
                     select new
                     {
                       ProductCode = data.Field<string>("ProductCode"),
                       Quantity = data.Field<long>("QuantityAvailable")
                     }).ToDictionary(x => x.ProductCode, x => x.Quantity);
      }

      #endregion

      #region Process

      var Vendor = unit.Scope.Repository<Vendor>().GetSingle(v => v.VendorID == vendorID);
      var vendorstocks = (from s in unit.Scope.Repository<VendorStock>().GetAll()
                          let assortment = s.Vendor.VendorAssortments.FirstOrDefault(c => c.ProductID == s.ProductID)
                          where s.VendorID == vendorID && assortment.IsActive == true
                          select new
                          {
                            CustomerItemNumber = assortment.CustomItemNumber,
                            Stock = s
                          }).ToList();

      int totalProducts = stockData.Count;
      int couterProduct = 0;
      int logCount = 0;
      log.DebugFormat("Start Stock processing for {0}, {1} Products", "VSN", totalProducts);

      foreach (var stock in stockData)
      {
        couterProduct++;
        logCount++;
        if (logCount == 1000)
        {
          log.DebugFormat("Stock Processed : {0}/{1} for Vendor {2}", couterProduct, totalProducts, "VSN");
          logCount = 0;
        }

        var vendorstock = vendorstocks.FirstOrDefault(x => x.CustomerItemNumber == stock.Key);

        if (vendorstock != null)
        {
          vendorstock.Stock.QuantityOnHand = (int)stockData[stock.Key];
          unit.Save();
        }
      }


      #endregion
    }
  }
}
