using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using System.Xml.Serialization;
using System.IO;
using Concentrator.Objects;
using Concentrator.Objects.Models.Slurp;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Microsoft.Practices.ServiceLocation;

namespace Concentrator.Plugins.Slurp
{
  public class SlurpItem
  {
    ILog log;
    IUnitOfWork unit;

    public int QueueID { get; private set; }
    public Type PluginType { get; private set; }
    public int ProductID { get; private set; }
    public int SleepTime { get; private set; }
    public SlurpItem(Type pluginType, int queueId, int sleepTime)
    {
      PluginType = pluginType;
      SleepTime = sleepTime;
      QueueID = queueId;
      log = LogManager.GetLogger(pluginType);
    }

    public SlurpResult Process()
    {
      log.InfoFormat("Start task for {0}", PluginType.Name);


      using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
      {

        var QueueItem = unit.Scope.Repository<SlurpQueue>().GetSingle(x => x.QueueID == QueueID);
        QueueItem.StartTime = DateTime.Now;

        ProductID = QueueItem.ProductID;

        SlurpResult result = new SlurpResult(QueueItem.Product.VendorItemNumber);
        try
        {
          var plugin = (ISlurpSite)Activator.CreateInstance(PluginType);

          result = plugin.Process(QueueItem.Product.VendorItemNumber);

          result.SiteName = PluginType.Name;

          #region Update Results

          if (result.ProductStatus == ProductStatus.Valid)
          {

            var mappingRepo = unit.Scope.Repository<ProductCompetitorMapping>();
            var priceRepo = unit.Scope.Repository<ProductCompetitorPrice>();

            var mappings = mappingRepo.GetAll(x => x.ProductCompareSourceID == QueueItem.ProductCompareSourceID).ToList();
            var prices = priceRepo.GetAll(x => x.ProductCompetitorMapping.ProductCompareSourceID == QueueItem.ProductCompareSourceID).ToList();


            foreach (var shop in result.Shops)
            {

              var mapping = mappings.SingleOrDefault(c => c.Competitor == shop.Name.Trim());
              if (mapping == null)
              {
                //new
                mapping = new ProductCompetitorMapping() { Competitor = shop.Name.Trim(), ProductCompareSourceID = QueueItem.ProductCompareSourceID, ProductCompetitorID = null };

                mappingRepo.Add(mapping);
                //                context.SubmitChanges();

                mappings.Add(mapping);

              }



              var newPrice = shop.TotalPrice ?? 0;
              var stockInfo = shop.Delivery.ToString();

              //price insert

              ProductCompetitorPrice price = prices.SingleOrDefault(p => p.ProductCompetitorMappingID == mapping.ProductCompetitorMappingID
                                      && p.ProductID == QueueItem.ProductID);
              if (price == null)
              {
                price = new ProductCompetitorPrice()
                {
                  ProductCompetitorMapping = mapping,
                  ProductID = QueueItem.ProductID,
                  Price = newPrice,
                  Stock = stockInfo,
                  CreationTime = DateTime.Now,
                  LastImport = DateTime.Now.AddDays(-30)
                };
                priceRepo.Add(price);

              }
              else
              {

                //ledger

                //check for diff
                if (price.Price != newPrice || price.Stock != stockInfo)
                {

                  ProductCompetitorLedger ledger = new ProductCompetitorLedger()
                  {
                    Price = price.Price,
                    Stock = price.Stock,
                    ProductCompetitorPriceID = price.ProductCompetitorPriceID,
                    CreationTime = DateTime.Now
                  };

                  price.Price = newPrice;
                  price.Stock = shop.Delivery.ToString();

                  unit.Scope.Repository<ProductCompetitorLedger>().Add(ledger);
                }

              }


            }
            
            CompleteItem(unit, QueueItem);
            //unit.Save();

          }
          else
          {
              CompleteItem(unit, QueueItem);
          }

          #endregion
          

          log.DebugFormat("Sleeping for {0} seconds.", SleepTime);
          Thread.Sleep(SleepTime * 1000);


        }
        catch (Exception ex)
        {
          log.Error("Error during processing", ex);
          result = new SlurpResult(QueueItem.Product.VendorItemNumber);
          result.SiteName = PluginType.Name;

          CompleteItem(unit, QueueItem);



        }
        finally
        {

          WriteLog(result);
        }





        return result;
      }


    }

    private void CompleteItem(IUnitOfWork unit, SlurpQueue QueueItem)
    {

      QueueItem.IsCompleted = true;
      QueueItem.CompletionTime = DateTime.Now;
      log.InfoFormat("Finished task for {0}", PluginType.Name);
      unit.Save();
    }

    XmlSerializer ser = new XmlSerializer(typeof(SlurpResult));
    private void WriteLog(SlurpResult result)
    {
      var fn = result.ManufacturerId.Trim().Replace("/", "_").Replace("\\", "_");

      string fileName = String.Format(@"c:\slurp\{1}-{0}.txt", fn, result.SiteName);

      using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
      {
        ser.Serialize(fs, result);
      }
    }

  }
}
