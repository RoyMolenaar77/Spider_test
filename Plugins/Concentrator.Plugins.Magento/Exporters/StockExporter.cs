using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Connectors;
using AuditLog4Net.Adapter;
using Concentrator.Objects;
using Concentrator.Plugins.Magento.Helpers;
using Concentrator.Plugins.Magento.Models;
using System.Xml.Linq;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using Concentrator.Web.Services;
using System.Xml;
using Concentrator.Objects.AssortmentService;

namespace Concentrator.Plugins.Magento.Exporters
{
  public class StockExporter : BaseExporter
  {
    private static string OutputFolder = @"E:\magento\";
    private static string CacheFile = @"E:\magento\assortment-{0}-{1}.xml";

    private List<AssortmentStockPriceProduct> Assortment;
    Dictionary<string, List<int>> ProductsPerAttributeSet;
    protected bool UsesConfigurableProducts { get; private set; }
    private Dictionary<int, List<AssortmentStockPriceProduct>> ConfigurableProducts { get; set; }
    private HashSet<int> SimpleProducts { get; set; }

    public StockExporter(Connector connector, IAuditLogAdapter logger)
      : base(connector, logger)
    {

    }

    protected override void Process()
    {
      foreach (var language in Languages)
      {
        CurrentLanguage = language;

        var Soap = new AssortmentService();

        Service service = new Service(Connector.ConnectorID);
        Assortment = service.GetStockAndPrices();


        UsesConfigurableProducts = (from p in Assortment
                                    where
                                    (p.IsConfigurable)
                                    select p
                                        ).Any();


        var ProductList = Assortment.ToDictionary(x => x.ProductID, z => z);

        if (UsesConfigurableProducts)
        {
          var rpc = Assortment.Where(c => c.IsConfigurable).ToList();


          ConfigurableProducts = (from p in Assortment
                                  where p.IsConfigurable
                                  && p.RelatedProducts != null

                                  let children = (from rp in p.RelatedProducts
                                                  let relatedProductId = rp.RelatedProductID
                                                  where rp.IsConfigured
                                                  && ProductList.ContainsKey(relatedProductId)
                                                  select ProductList[relatedProductId]).ToList()

                                  where children.Count() > 0

                                  select new
                                  {
                                    Parent = p,
                                    Children = children
                                  }).ToDictionary(x => x.Parent.ProductID, y => y.Children.ToList());




          SimpleProducts = new HashSet<int>((from p in ConfigurableProducts
                                             select p.Value.Select(x => x.ProductID)
                                             ).SelectMany(x => x).Distinct());
        }

        Logger.Info("Processing stock for " + language.Language.Name);
        SyncProducts();
      }
    }

    private void SyncProducts()
    {
      bool doRetailStock = ((ConnectorSystemType)Connector.ConnectorType).Has(ConnectorSystemType.ExportRetailStock);

      XDocument retailStock = null;
      Dictionary<int, List<AssortmentRetailStock>> retailStockList = new Dictionary<int, List<AssortmentRetailStock>>();

      if (IsPrimaryLanguage && doRetailStock)
      {
        retailStockList = (from r in Assortment
                           select new
                           {
                             product_id = r.ProductID,
                             store_records = r.RetailStock
                           }).Distinct().ToDictionary(x => x.product_id, y => y.store_records);
      }

      Dictionary<string, int> stockStoreList = new Dictionary<string, int>();
      if (doRetailStock)
      {
        using (var helper = new AssortmentHelper(Connector.Connection, Version))
        {
          stockStoreList = helper.GetStockStoreList();
        }
      }

      var productsSource = (from p in Assortment
                            select p);

      List<AssortmentStockPriceProduct> products = productsSource.Where(x => !x.IsConfigurable).ToList();

      ProcessProductCollection(retailStockList, stockStoreList, products);

      using (var helper = new AssortmentHelper(Connector.Connection, Version))
      {
        var skuList = helper.GetSkuList();
        var skuInXmlList = (from p in productsSource
                            select p.ManufacturerID).ToList();

        var toDeactivate = skuList.Where(x => !skuInXmlList.Contains(x.Key));

        //exclude shipping costs
        toDeactivate = toDeactivate.Where(x => !x.Key.StartsWith("ShippingCostProductID_"));

        if (toDeactivate.Count() > 0)
        {
          SortedDictionary<string, eav_attribute> attributeList = helper.GetAttributeList(PRODUCT_ENTITY_TYPE_ID);

          foreach (var p in toDeactivate)
          {
            SetProductStatus(p.Value.entity_id, (int)PRODUCT_STATUS.DISABLED, helper, attributeList, StoreList[CurrentStoreCode].store_id);

            if (RequiresStockUpdate)
            {
              helper.SyncStock(new cataloginventory_stock_item()
              {
                product_id = p.Value.entity_id,
                qty = 0,
                is_in_stock = false
              });
            }
          }
        }
      }
    }

    private void ProcessProductCollection(Dictionary<int, List<AssortmentRetailStock>> retailStockList, Dictionary<string, int> stockStoreList, List<AssortmentStockPriceProduct> products)
    {
      int totalProcessed = 0;
      SortedDictionary<string, eav_attribute> attributeList;
      int totalRecords = products.Count();
      Logger.DebugFormat("Found {0} products", totalRecords);

      using (var helper = new AssortmentHelper(Connector.Connection, Version))
      {
        attributeList = helper.GetAttributeList(PRODUCT_ENTITY_TYPE_ID);
      }

      ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = 1 };

      Parallel.ForEach(Partitioner.Create(0, totalRecords), options, (range, loopState) =>
      {

        using (var helper = new AssortmentHelper(Connector.Connection, Version))
        {
          var skuList = helper.GetSkuList();

          for (int index = range.Item1; index < range.Item2; index++)
          {
            int productID = Convert.ToInt32(products[index].ProductID);

            IEnumerable<AssortmentRetailStock> productRetailStock = null;

            ProcessProduct(helper, products[index], skuList, attributeList, stockStoreList, productRetailStock);

            Interlocked.Increment(ref totalProcessed);
            if (totalProcessed % 100 == 0)
              Logger.DebugFormat(String.Format("Processed {0} of {1} records", totalProcessed, totalRecords));
          }
        }
      });
    }

    private void ProcessProduct(AssortmentHelper helper, AssortmentStockPriceProduct product,
        SortedDictionary<string, catalog_product_entity> skuList,
      SortedDictionary<string, eav_attribute> attributeList,
       Dictionary<string, int> stockStoreList,
      IEnumerable<AssortmentRetailStock> productRetailStock
      )
    {
      string sku = product.ManufacturerID;
      int productID = product.ProductID;
      string customProductId = product.CustomProductID;
      bool isNonAssortmentItem = (product.IsNonAssortmentItem);
      bool isConfigurable = (product.IsConfigurable);
      bool childrenOnStock = false;
      string type = isConfigurable ? "configurable" : "simple";

      catalog_product_entity entity = null;

      skuList.TryGetValue(sku, out entity);

      if (entity == null) return;

      if (isConfigurable && ConfigurableProducts.ContainsKey(productID))
      {
        var children = ConfigurableProducts[productID];

        childrenOnStock = (children.Any(x => x.Stock != null && x.Stock.InStock > 0));
      }

      int storeid = 0;
      if (MultiLanguage && !IsPrimaryLanguage)
        storeid = StoreList[CurrentStoreCode].store_id;

      int productStatus = (int)PRODUCT_STATUS.ENABLED;

      #region Stock

      if (IsPrimaryLanguage)
      {
        int? quantityOnHand = null;
        int? quantityToReceive = null;
        string stockStatus = "S";
        DateTime? promisedDeliveryDate = null;
        var stockElement = product.Stock;

        if (stockElement != null)
        {
          int tmp;

          quantityOnHand = stockElement.InStock;
          quantityToReceive = stockElement.QuantityToReceive;
          stockStatus = stockElement.StockStatus.Trim();

          DateTime dt;
          if (stockElement.PromisedDeliveryDate.HasValue)
            promisedDeliveryDate = stockElement.PromisedDeliveryDate.Value;
        }

        if (productRetailStock != null)
        {
          if (productRetailStock.Count() > 0)
          {
            foreach (var s in stockStoreList)
            {
              var store_record = productRetailStock.FirstOrDefault(x => x.VendorCode == s.Key);
              int qty = (store_record != null) ? store_record.InStock : 0;

              helper.SyncStoreStock(sku, s.Value, qty);
            }
          }
          else
          {
            helper.ClearStoreStock(sku);
            //no stores left with stock, clear all
          }
        }

        if (RequiresStockUpdate)
        {
          helper.SyncAttributeValue(attributeList["quantity_to_receive"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, quantityToReceive ?? 0, "int");
          helper.SyncAttributeValue(attributeList["promised_delivery_date"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, promisedDeliveryDate, "datetime");
          helper.SyncAttributeValue(attributeList["stock_status"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, stockStatus, "varchar");

          helper.SyncWebsiteProduct(StoreList[CurrentStoreCode].website_id, entity.entity_id);

          bool is_in_stock = quantityOnHand > 0 || (promisedDeliveryDate.HasValue && quantityToReceive > 0) || (productRetailStock != null && productRetailStock.Count() > 0)
                    || (!String.IsNullOrEmpty(stockStatus) && stockStatus != "O");

          if (isConfigurable)
          {
            if (childrenOnStock)
              is_in_stock = true;
            else
              is_in_stock = false;
          }

          helper.SyncStock(new cataloginventory_stock_item()
          {
            product_id = entity.entity_id,
            qty = quantityOnHand ?? 0,
            is_in_stock = is_in_stock
          });

          if (!is_in_stock)
            productStatus = (int)PRODUCT_STATUS.DISABLED;

          SetProductStatus(entity.entity_id, productStatus, helper, attributeList, 0);
        }
      }

      SetProductStatus(entity.entity_id, productStatus, helper, attributeList, StoreList[CurrentStoreCode].store_id);
      #endregion

      #region Price
      decimal? price = null;
      decimal? specialPrice = null;
      string commercialStatus = "O";

      int visibility = 4; //catalog, search
      if (isNonAssortmentItem || (UsesConfigurableProducts && SimpleProducts.Contains(productID)) || !product.Visible)
      {
          visibility = 1; // not visible individually       
      }

      ExtractPrice(product, ref price, ref specialPrice, ref commercialStatus);

      helper.SyncAttributeValue(attributeList["visibility"].attribute_id, PRODUCT_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, entity.entity_id, visibility, "int");
      helper.SyncAttributeValue(attributeList["price"].attribute_id, PRODUCT_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, entity.entity_id, price, "decimal");

      if (IsPrimaryLanguage)
      {
        helper.SyncAttributeValue(attributeList["visibility"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, visibility, "int");
        helper.SyncAttributeValue(attributeList["price"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, price, "decimal");
      }

      helper.SyncAttributeValue(attributeList["special_price"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, null, "decimal");

      if ((specialPrice ?? 0) > 0)
      {
        helper.SyncAttributeValue(attributeList["special_price"].attribute_id, PRODUCT_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, entity.entity_id, specialPrice, "decimal");
        if (Connector.ConnectorID == 11)
        {
          helper.SyncAttributeValue(attributeList["special_price"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, specialPrice, "decimal");
        }
      }
      else
        helper.DeleteAttributeValue(attributeList["special_price"].attribute_id, PRODUCT_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, entity.entity_id, "decimal");
      #endregion
    }

    private void ExtractPrice(AssortmentStockPriceProduct product, ref decimal? price, ref decimal? specialPrice, ref string commercialStatus)
    {
      var priceElement = product.Prices.FirstOrDefault();

      if (priceElement != null)
      {
        commercialStatus = priceElement.CommercialStatus.Trim();
        if (bool.Parse(Connector.ConnectorSettings.GetValueByKey("PriceInTax", "True")))
        {
          price = priceElement.UnitPrice *
                     ((priceElement.TaxRate / 100) + 1);
        }
        else
        {
          price = priceElement.UnitPrice;
        }

        if (priceElement.SpecialPrice.HasValue)
        {
          if (bool.Parse(Connector.ConnectorSettings.GetValueByKey("PriceInTax", "True")))
          {
            specialPrice = priceElement.SpecialPrice.Value *
                       ((priceElement.SpecialPrice.Value / 100) + 1);
          }
          else
          {
            specialPrice = priceElement.SpecialPrice.Value;
          }
        }
      }
    }

    private void SetProductStatus(int entity_id, int productStatus, AssortmentHelper helper, SortedDictionary<string, eav_attribute> attributeList, int store_id = 0)
    {
      bool ignoreProductStatusUpdates = Connector.ConnectorSettings.GetValueByKey<bool>("IgnoreProductStatusUpdates", false);

      if (!ignoreProductStatusUpdates)
      {
        helper.SyncAttributeValue(attributeList["status"].attribute_id, PRODUCT_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, entity_id, productStatus, "int");
      }
    }

  }
}
