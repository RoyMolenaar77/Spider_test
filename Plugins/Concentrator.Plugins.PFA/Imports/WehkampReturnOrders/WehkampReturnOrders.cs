using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects;
using Concentrator.Objects.Models.Vendors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Concentrator.Plugins.PFA.Repos;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Products;
using PetaPoco;
using Concentrator.Plugins.PFA.Objects.Helper;

namespace Concentrator.Plugins.PFA.Imports.WehkampReturnOrders
{
  public class WehkampReturnOrders : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Wehkamp return order import"; }
    }

    protected override void Process()
    {
      var vendors = ReturnOrderRepository.GetVendorsForReturnOrders();


      foreach (var vendor in vendors)
      {
        var connectorID = ReturnOrderRepository.GetWehkampConnectorForVendor(vendor.VendorID);

        string returnOrderPath = VendorHelper.GetImportPathOfWehkampOrders(vendor.VendorID);// vendor.VendorSettings.GetValueByKey<string>("PathForWehkampReturnOrders", string.Empty);

        returnOrderPath.ThrowIfNullOrEmpty(new InvalidOperationException("Vendor " + vendor.Name + " doesn't have path for the WehkampReturnOrders"));
        var processedDirectory = Path.Combine(returnOrderPath, "Processed");

        if (!Directory.Exists(processedDirectory))
          Directory.CreateDirectory(processedDirectory);


        foreach (var file in Directory.GetFiles(returnOrderPath, "*.xlsx"))
        {
          try
          {
            var returnOrders = ReturnOrderRepository.GetReturnOrders(file);
            //create order
            //for each return -> create orderline 

            Order o = new Order()
            {
              OrderType = (int)OrderTypes.ReturnOrder,
              ConnectorID = connectorID,
              IsDispatched = false,
              Document = file,
              PaymentTermsCode = "Wehkamp Return",
              WebSiteOrderNumber = string.Format("R{0}", DateTime.Now.Ticks)
            };

            List<OrderLine> orderLines = new List<OrderLine>();
            foreach (var line in returnOrders)
            {
              foreach (var product in ReturnOrderRepository.GetAllSkusOfArtikelColor(line.ItemNumber, line.ColorCode))
              {
                if (!ReturnOrderRepository.OpenOrderLineExistsForProduct(product.ProductID))
                  orderLines.Add(new OrderLine()
                  {
                    ProductID = product.ProductID,
                    WareHouseCode = line.StoreNumber
                  });
              }
            }

            if (orderLines.Count > 0)
            {
              ReturnOrderRepository.CreateReturnOrder(o, orderLines);
            }

            FileInfo info = new FileInfo(file);
            info.MoveTo(Path.Combine(processedDirectory, string.Format("{0}-{1}.xslx", Path.GetFileNameWithoutExtension(file), DateTime.Now.ToString("yyyyMMdd_HHmmss"))));

            //File.Copy(file, Path.Combine(processedDirectory, file), true);
            //File.Delete(file);
          }
          catch (Exception e)
          {
            File.Move(file, file + ".err");

            //if (!File.Exists(logFile))
            //  File.Create(logFile);

            //using (StreamWriter sw = new StreamWriter(logFile))
            //{
            //  sw.Write(e.Message);
            //}
            log.AuditError("Something went wrong with reading the file", e);

          }
        }
      }
    }
  }
}
