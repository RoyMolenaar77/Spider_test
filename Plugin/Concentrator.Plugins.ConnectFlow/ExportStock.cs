using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Configuration;
using System.Xml.Linq;
using System.Globalization;
using Concentrator.Objects;
using Concentrator.Web.ServiceClient.AssortmentService;
using System.Reflection;
using log4net;
using Concentrator.Objects.ConcentratorService;
using System.IO;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Ftp;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.Plugins.ConnectFlow
{
  [ConnectorSystem(4)]
  public class ExportStock : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "ConnectFlow Stock Exporter"; }
    }

    protected override void Process()
    {
      var path = Path.Combine(GetConfiguration().AppSettings.Settings["ExportPath"].Value);

      bool networkDrive = false;
      bool.TryParse(GetConfiguration().AppSettings.Settings["IsNetworkDrive"].Value, out networkDrive);

      if (networkDrive)
      {
        string userName = GetConfiguration().AppSettings.Try(x => x.Settings["NetworkUserName"].Value, string.Empty);
        string passWord = GetConfiguration().AppSettings.Try(x => x.Settings["NetworkPassword"].Value, string.Empty);

        NetworkDrive oNetDrive = new NetworkDrive();
        try
        {
          oNetDrive.LocalDrive = "H:";
          oNetDrive.ShareName = path;

          //oNetDrive.MapDrive("diract", "D1r@ct379");
          if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(passWord))
            oNetDrive.MapDrive(userName, passWord);
          else
            oNetDrive.MapDrive();

          path = "H:";
        }
        catch (Exception err)
        {
          log.Error("Invalid network drive", err);
        }
        oNetDrive = null;
      }

      var baseConnector = base.Connectors.Where(x => ((ConnectorType)x.ConnectorType).Has(ConnectorType.ShopAssortment)).FirstOrDefault();
      List<string> added = new List<string>();

      var stockPath = Path.Combine(path, "SAPStock2CF/GDATEN");

      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);

      string DC = baseConnector.ConnectorSettings.GetValueByKey("DC", string.Empty);
      if (string.IsNullOrEmpty(DC))
        throw new Exception(string.Format("No DC set for connector {0} in settings", baseConnector.Name));

      string fileName = string.Format("{0}.sapvrd", DC);

      string filePath = Path.Combine(stockPath, fileName);

      using (StreamWriter sw = new StreamWriter(filePath))
      {

        foreach (Connector connector in base.Connectors.Where(x => ((ConnectorType)x.ConnectorType).Has(ConnectorType.ShopAssortment)))
        {
          #region Stock

          log.DebugFormat("Start Process ConnectFlow stock export for {0}", connector.Name);
          AssortmentServiceSoapClient soap = new AssortmentServiceSoapClient();

          try
          {
            XDocument products = new XDocument(soap.GetStockAssortment(connector.ConnectorID, null));
            Dictionary<string, string> productTranslation = ConnectFlowUtility.GetCrossProducts(products);

            var csv = (from p in products.Root.Elements("Product")
                       select new
                       {
                         ProductID = ConnectFlowUtility.GetAtricleNumber(p.Attribute("ProductID").Value, productTranslation, true),
                         Stock = p.Element("Stock").Attribute("InStock").Value
                       }).ToList();

            csv.ForEach(x =>
            {
              if (!added.Contains(x.ProductID))
              {
                var sapArtNr = ConnectFlowUtility.GetAtricleNumber(x.ProductID, productTranslation, true).Trim();
                int artNr = 0;
                sw.WriteLine(string.Format("{0},{1},{2},{3},{4}", DC, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HHmmss"), (int.TryParse(sapArtNr, out artNr) ? artNr.ToString().PadLeft(18, '0') : sapArtNr.PadRight(18)), x.Stock.ToString().PadLeft(13, '0'))); //1001,20110701,020005,000000000000081846,0000000000000
                added.Add(x.ProductID);
              }
            });


            //var retailProducts = (from r in products.Root.Elements("Product")
            //                      select new
            //                      {
            //                        sapArtNr = ConnectFlowUtility.GetAtricleNumber(r.Attribute("ProductID").Value, productTranslation, true).Trim(),
            //                        RetailStock = r.Element("Stock").Element("Retail").Elements("RetailStock")
            //                      }).Distinct().ToList();

            //Dictionary<string, List<CFstock>> Shops = new Dictionary<string, List<CFstock>>();
            //Dictionary<string, IEnumerable<XElement>> records = new Dictionary<string, IEnumerable<XElement>>();

            //using (var unit = GetUnitOfWork())
            //{
            //  var shopNumbers = (from v in unit.Scope.Repository<Vendor>().GetAll()
            //                     join vs in unit.Scope.Repository<VendorSetting>().GetAll() on v.VendorID equals vs.VendorID
            //                     where vs.SettingKey == "ShopNumber"
            //                     select new
            //                     {
            //                       VendorCode = v.BackendVendorCode,
            //                       ShopNumber = vs.Value
            //                     }).ToDictionary(x => x.VendorCode, x => x.ShopNumber);

            //  foreach (var retail in retailProducts)
            //  {
            //    if (!records.ContainsKey(retail.sapArtNr))
            //      records.Add(retail.sapArtNr, retail.RetailStock.ToList());
            //  }

            //  int totalProducts = records.Keys.Count;
            //  int couterProduct = 0;
            //  int logCount = 0;
            //  foreach (var art in records.Keys)
            //  {
            //    couterProduct++;
            //    logCount++;
            //    if (logCount == 100)
            //    {
            //      log.DebugFormat("'Magento retailstock products Processed : {0}/{1} for connector {2}", couterProduct, totalProducts, connector.Name);
            //      logCount = 0;
            //    }

            //    foreach (var p in records[art])
            //    {
            //      if (shopNumbers.ContainsKey(p.Attribute("VendorCode").Value))
            //      {
            //        CFstock s = new CFstock()
            //        {
            //          ShopNumber = shopNumbers[p.Attribute("VendorCode").Value],
            //          SapNr = art,
            //          Stock = int.Parse(p.Attribute("InStock").Value)
            //        };

            //        if (Shops.ContainsKey(s.ShopNumber))
            //          Shops[s.ShopNumber].Add(s);
            //        else
            //          Shops.Add(s.ShopNumber, new List<CFstock>() { s });
            //      }
            //    }
            //  }
            //}

            //foreach (var shop in Shops.Keys)
            //{
            //  var rec = Shops[shop];
            //  var shopNr = rec.FirstOrDefault().ShopNumber;
            //  fileName = string.Format("{0}.sapvrd", shopNr);

            //  filePath = Path.Combine(stockPath, fileName);

            //  using (StreamWriter sw = new StreamWriter(filePath))
            //  {
            //    rec.ForEach(x =>
            //    {
            //      var sapArtNr = x.SapNr;
            //      int artNr = 0;
            //      sw.WriteLine(string.Format("{0},{1},{2},{3},{4}", shopNr, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HHmmss"), (int.TryParse(sapArtNr, out artNr) ? artNr.ToString().PadLeft(18, '0') : sapArtNr.PadRight(18)), x.Stock.ToString().PadLeft(13, '0'))); //1001,20110701,020005,000000000000081846,0000000000000
            //    });
            //  }
            //}


          }
          catch (Exception ex)
          {
            log.Error("FTP upload ConnectFlow stockfile failed for connector" + connector.ConnectorID, ex);
          }

          log.DebugFormat("Finish Process ConnectFlow stock import for {0}", connector.Name);

          #endregion

        }
      }

      string readyFile = Path.Combine(path, "SAPStock2CF/Aggretor.Ready");

      using (StreamWriter sw = new StreamWriter(readyFile))
      {
        sw.Write("Trigger");
      }
    }
  }

  public class CFstock
  {
    public string SapNr { get; set; }
    public string ShopNumber { get; set; }
    public int Stock { get; set; }
  }
}
