using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Plugins.PFA.Configuration;
using Concentrator.Plugins.PFA.Helpers;
using Concentrator.Plugins.PFA.Models;
using Concentrator.Plugins.PFA.Models.CC;
using Concentrator.Plugins.PFA.Objects.Helper;
using Concentrator.Plugins.PFA.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Concentrator.Plugins.PFA.Imports.Stock
{
  public class PFAStockImportCC : StockImportBase
  {
    private const string STOCK_ENABLED_STATUS = "ENA";
    private const string STOCK_DISABLED_STATUS = "NoStock";
    private List<int> _vendorIDs = new List<int> { 1, 13, 14 };
    private int _wehkampVendorID = 15;

    public override string Name
    {
      get { return "PFA Stock import CC"; }
    }

    protected override List<VendorStockCollectionModel> GetStock()
    {
      List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock> result = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock>();
      var pluginConfig = GetConfiguration();

      var connectionString = VendorHelper.GetConnectionStringForPFA(1);//todo vendorid
      var stockConnectionString = PFAConnectionHelper.GetCoolcatPFAVrdsConnection(pluginConfig);

      CoolcatPFARepository repository = new CoolcatPFARepository(connectionString, stockConnectionString, log);
      AssortmentHelper helper = new AssortmentHelper();
      XDocument seasonCodeRulesDoc = null;
      DateTime? datcolTimeStamp = null;

      bool datcolSuccessful = true;

      //if the vrds was not process, exit
      if (!IsVrdsValid(repository))
        return null;

      using (var unit = GetUnitOfWork())
      {
        seasonCodeRulesDoc = GetCategoryStockBlacklist(unit);
        datcolSuccessful = WasDatcolRunSuccessful(unit);


        var currentStockDate = helper.GetCurrentStockDate();
        var stockCM = repository.GetCMStock(currentStockDate);
        var stockTransfer = repository.GetTransferStock(currentStockDate);
        var stockWms = repository.GetWmsStock(currentStockDate);
        var validArtCodes = repository.GetValidItemNumbers();
        var stockWehkamp = repository.GetWehkampStock(currentStockDate);
        var countryCode = VendorHelper.GetCountryCode(1);
        log.Info(string.Format("About to process the stock of {0} artikels", validArtCodes.Count));
        int counter = 0;

        foreach (var itemNumber in validArtCodes)
        {
          counter++;
          if (counter % 100 == 0) log.Info(string.Format("Processed {0} products", counter));
          var productInfo = repository.GetGeneralProductInformation(itemNumber, countryCode);

          bool loadCMStock = true;

          if (seasonCodeRulesDoc != null)
          {
            if (IsGroupFiltered(itemNumber, productInfo.SeasonCode, seasonCodeRulesDoc))
            {
              loadCMStock = false;
            }
          }

          var artikelStockCM = (from p in stockCM
                                where p.ItemNumber.ToLower() == itemNumber.ToLower()
                                select p).ToList();

          var artikelStockWMS = (from p in stockWms
                                 where p.ItemNumber.ToLower() == itemNumber.ToLower()
                                 select p).ToList();

          var artikelStockWehkamp = (from p in stockWehkamp
                                     where p.ItemNumber.ToLower() == itemNumber.ToLower()
                                     select p).ToList();

          var artikelStockTransfer = (from p in stockTransfer
                                      where p.ItemNumber.ToLower() == itemNumber.ToLower()
                                      select p).ToList();

          result.Add(GetVendorStock(itemNumber, 0, CCStockType.Webshop));
          result.Add(GetVendorStock(itemNumber, 0, CCStockType.CM));
          result.Add(GetVendorStock(itemNumber, 0, CCStockType.Transfer));
          result.Add(GetVendorStock(itemNumber, 0, CCStockType.Wehkamp));
          result.Add(GetVendorStock(itemNumber, 0, CCStockType.Filiaal890));

          var validSkus = repository.GetValidSkus(itemNumber);

          foreach (var color in validSkus.GroupBy(c => c.ColorCode))
          {
            string colorLevel = string.Format("{0} {1}", itemNumber, color.Key);

            result.Add(GetVendorStock(colorLevel, 0, CCStockType.Webshop));
            result.Add(GetVendorStock(colorLevel, 0, CCStockType.CM));
            result.Add(GetVendorStock(colorLevel, 0, CCStockType.Transfer));
            result.Add(GetVendorStock(colorLevel, 0, CCStockType.Wehkamp));
            result.Add(GetVendorStock(colorLevel, 0, CCStockType.Filiaal890));
          }

          foreach (var sku in validSkus)
          {
            string skuNew = string.Empty;
            if (itemNumber == PfaCoolcatConfiguration.Current.ShipmentCostsProduct || itemNumber == PfaCoolcatConfiguration.Current.ReturnCostsProduct
                  || itemNumber == PfaCoolcatConfiguration.Current.KialaShipmentCostsProduct || itemNumber == PfaCoolcatConfiguration.Current.KialaReturnCostsProduct)
              skuNew = itemNumber;
            else
              skuNew = string.Format("{0} {1} {2}", itemNumber, sku.ColorCode, sku.SizeCode);

            int webshopStock = 0;
            int ceyenneStock = 0;
            int transferStock = 0;
            int wmsStock = 0;
            int wehkampStock = 0;

            transferStock = GetSkuStockResult(artikelStockTransfer, sku).Try(c => c.Quantity, 0);
            ceyenneStock = (int)(0.5 * GetSkuStockResult(artikelStockCM, sku).Try(c => c.Quantity, 0));
            wehkampStock = GetSkuStockResult(artikelStockWehkamp, sku).Try(c => c.Quantity, 0);

            if (ceyenneStock < 30)
              ceyenneStock = 0;

            wmsStock = GetSkuStockResult(artikelStockWMS, sku).Try(c => c.Quantity, 0);

            if (!datcolSuccessful)
            {
              //What do we do with the vendorid here?
              var w = unit.Scope.Repository<VendorStock>().GetAllAsQueryable(c => c.VendorID == 1 && c.VendorStockTypeID == 3 && c.Product.VendorItemNumber == skuNew).FirstOrDefault();

              wmsStock = 0;
              if (w != null)
                wmsStock = w.QuantityOnHand;
            }

            webshopStock = ceyenneStock + wmsStock + transferStock;

            if (!loadCMStock)
            {
              webshopStock = webshopStock - ceyenneStock;
              ceyenneStock = 0;
            }

            result.Add(GetVendorStock(skuNew, webshopStock, CCStockType.Webshop));
            result.Add(GetVendorStock(skuNew, ceyenneStock, CCStockType.CM));
            result.Add(GetVendorStock(skuNew, transferStock, CCStockType.Transfer));
            result.Add(GetVendorStock(skuNew, wehkampStock, CCStockType.Wehkamp));

            if (datcolSuccessful)
            {
              result.Add(GetVendorStock(skuNew, wmsStock, CCStockType.Filiaal890));
            }

          }
        }
      }

      List<VendorStockCollectionModel> groupedResult = new List<VendorStockCollectionModel>();

      //for the regular vendors add all stock locations without Wehkamp
      foreach (var vendor in _vendorIDs)
      {
        groupedResult.Add(new VendorStockCollectionModel()
        {
          VendorID = vendor,
          DefaultVendorID = 1,
          StockCollection = result.Where(c => c.StockType != CCStockType.Wehkamp.ToString()).ToList()
        });
      }


      var wehkampStockCollection = result.Where(c => c.StockType == CCStockType.Wehkamp.ToString()).ToList();

      //For vendor Wehkamp, add the stock the webshop location for exporting
      wehkampStockCollection.AddRange((from r in wehkampStockCollection
                                       select GetVendorStock(r.CustomItemNumber, r.QuantityOnHand, CCStockType.Webshop)).ToList());

      //wehkamp
      groupedResult.Add(new VendorStockCollectionModel()
      {
        VendorID = _wehkampVendorID,
        DefaultVendorID = 1,
        StockCollection = wehkampStockCollection
      });

      return groupedResult;
    }

    private Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock GetVendorStock(string itemNumber, int qty, CCStockType stockType, string stockStatus = "")
    {
      return new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock()
      {
        //VendorID = vendorID,
        //DefaultVendorID = parentVendorID,
        CustomItemNumber = itemNumber,
        QuantityOnHand = Math.Max(qty, 0), //Handle negative quantities
        StockType = stockType.ToString(),
        StockStatus = string.IsNullOrEmpty(stockStatus) ? (qty == 0 ? STOCK_DISABLED_STATUS : STOCK_ENABLED_STATUS) : stockStatus
      };
    }

    private StockResult GetSkuStockResult(List<StockResult> itemResults, SkuResult sku)
    {
      return itemResults.FirstOrDefault(c => c.ColorCode.ToLower() == sku.ColorCode.ToLower() && c.SizeCode.ToLower() == sku.SizeCode.ToLower());
    }

    private XDocument GetCategoryStockBlacklist(IUnitOfWork unit)
    {
      XDocument result = null;
      var categoryStockBlacklistSetting = unit.Scope.Repository<VendorSetting>().GetSingle(c => c.VendorID == 1 && c.SettingKey == "ImportRules");
      if (categoryStockBlacklistSetting != null)
        result = XDocument.Parse(categoryStockBlacklistSetting.Value);

      return result;
    }

    private bool WasDatcolRunSuccessful(IUnitOfWork unit)
    {
      var vendorDatcol = unit.Scope.Repository<Vendor>().GetSingle(c => c.VendorID == 1);
      var datcolStamp = vendorDatcol.VendorSettings.FirstOrDefault(c => c.SettingKey == "DatcolTimeStamp").Try<VendorSetting, DateTime?>(c => DateTime.Parse(c.Value), null);


      if ((datcolStamp.HasValue) && (DateTime.Now.Date.AddDays(-1) != datcolStamp.Value.Date && DateTime.Now.Date != datcolStamp.Value.Date))
        return false;

      return true;
    }

    private bool IsVrdsValid(CoolcatPFARepository repository)
    {
      var lastDate = repository.GetLastVrdsExecutionTime();

      if (lastDate.Year != DateTime.Now.Year || lastDate.Month != DateTime.Now.Month || lastDate.Date != DateTime.Now.Date)
        return false;

      return true;
    }

    private bool IsGroupFiltered(string art_number, string season_code, XDocument seasonCodeRulesDoc)
    {
      if (seasonCodeRulesDoc == null)
        return false;

      //check season code
      var element = seasonCodeRulesDoc.Root.Elements("Rule").Where(c => c.Element("Season").Value.ToLower() == season_code.ToLower()).FirstOrDefault();
      if (element != null)
      {
        var allMatches = element.Element("ProductGroups").Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (allMatches.Any(c => art_number.ToLower().StartsWith(c.ToLower())))
          return true;

      }
      return false;
    }
  }
}
