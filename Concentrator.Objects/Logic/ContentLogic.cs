using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Data.Linq;
using Concentrator.Objects.Enumerations;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Complex;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Utility;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Prices;

namespace Concentrator.Objects.Logic
{
  public class ContentLogic
  {
    private Dictionary<int, List<Vendor>> vendorList = new Dictionary<int, List<Vendor>>();
    private List<VendorStockResult> vendorStocks = null;
    private List<CalculatedPriceView> priceRules;
    private List<VendorStockResult> vendorRetailStocks;
    private List<CustomItemNumberResult> customItemNumbers;
    private int _connectorID;
    private XElement _xdoc;
    private IScope _scope;

    /// <summary>
    /// First call FillPriceInformation
    /// </summary>
    public List<CalculatedPriceView> CalculatedPriceView
    {
      get { return priceRules; }
    }


    public ContentLogic(PetaPoco.Database db, int connectorID = -1)
    {
      if (connectorID != -1)
      {
        _connectorID = connectorID;

        var contentProducts = db.Query<ContentProduct>(@"SELECT * FROM CONTENTPRODUCT WHERE ConnectorID = @0", connectorID).Select(c => c.VendorID).Distinct().ToList(); /// .GetAll(v => v.ConnectorID == connectorID).Select(x => x.VendorID).Distinct().ToList();

        _xdoc = new XElement("Vendors",
                                  (from v in contentProducts
                                   select new XElement("VendorID", v)).ToList());
      }
    }

    public ContentLogic(IScope unit, int connectorID = -1)
    {
      _scope = unit;

      if (connectorID != -1)
      {
        _connectorID = connectorID;

        var contentProducts = unit.Repository<ContentProduct>().GetAll(v => v.ConnectorID == connectorID).Select(x => x.VendorID).Distinct().ToList();

        _xdoc = new XElement("Vendors",
                                  (from v in contentProducts
                                   select new XElement("VendorID", v)));
      }
    }

    public void FillPriceInformation(IEnumerable<CalculatedPriceView> records)
    {
      priceRules = records.ToList();
    }

    public void FillCustomItemNumbers()
    {
      var funcRepo = ((IFunctionScope)_scope).Repository();
      customItemNumbers = funcRepo.GetCustomItemNumbers(_connectorID).ToList();

    }

    public void FillRetailStock()
    {
      vendorRetailStocks = ((IFunctionScope)_scope).Repository().GetVendorRetailStock(_xdoc.ToString(), _connectorID).ToList();
    }
    public void FillRetailStock(PetaPoco.Database db)
    {
      vendorRetailStocks = db.Query<VendorStockResult>("EXECUTE sp_FetchVendorRetailStock @0, @1", _xdoc.ToString(), _connectorID).ToList();
    }

    private Dictionary<int, List<Vendor>> GetContentRuleVendors(int connectorID)
    {
      Dictionary<int, List<Vendor>> vendorList = new Dictionary<int, List<Vendor>>();

      var scope = _scope;
      var repoAssortment = scope.Repository<VendorAssortment>();
      var connectorRules = scope.Repository<ContentProduct>().GetAll(r => r.ConnectorID == connectorID);

      bool assortmentLoaded = false;

      foreach (ContentProduct rule in connectorRules.OrderBy(x => x.ProductContentIndex))
      {
        if (!assortmentLoaded)
          assortmentLoaded = rule.IsAssortment;

        #region content logic
        if (rule.ProductID.HasValue)
        {
          #region Single Product

          if (assortmentLoaded && !rule.IsAssortment)
          {
            // exclude product
            vendorList.Remove(rule.ProductID.Value);
          }
          else
          {
            var vass = (from a in repoAssortment.GetAllAsQueryable()
                        where a.VendorID == rule.VendorID
                        && a.ProductID == rule.ProductID.Value
                        && a.IsActive
                        select a.Vendor).ToList();

            vendorList.Add(rule.ProductID.Value, vass);
          }


          #endregion
        }
        else
        {
          //include scenario

          // base collection
          List<VendorAssortment> vassList = new List<VendorAssortment>();

          var vendorAssortment = (from a in repoAssortment.GetAllAsQueryable()
                                  where a.VendorID == rule.VendorID
                                  && a.IsActive
                                  select a);

          if (!rule.BrandID.HasValue && !rule.ProductGroupID.HasValue)
          {
            vassList.AddRange(vendorAssortment);
          }
          else
          {
            if (rule.BrandID.HasValue)
            {
              vassList.AddRange(vendorAssortment.Where(x => x.Product.BrandID == rule.BrandID));
            }

            if (rule.ProductGroupID.HasValue)
            {
              vassList.AddRange(
                vendorAssortment.Where(
                  x =>
                  x.ProductGroupVendors.Any(
                    vpga =>
                    vpga.ProductGroupID == rule.ProductGroupID.Value &&
                    vpga.VendorID == rule.VendorID)));
            }
          }

          foreach (var ass in vassList)
          {
            if (assortmentLoaded && !rule.IsAssortment)
            {
              vendorList.Remove(ass.ProductID);
            }
            else
            {
              if (vendorList.ContainsKey(ass.ProductID))
              {
                if (vendorList[ass.ProductID].Where(x => x.VendorID == ass.VendorID).Count() < 1)
                  vendorList[ass.ProductID].Add(ass.Vendor);
              }
              else
              {
                List<Vendor> vList = new List<Vendor>();
                vList.Add(ass.Vendor);
                vendorList.Add(ass.ProductID, vList);
              }
            }
          }
        }
        #endregion
      }
      return vendorList;
    }

    public string GetVendorProductID(int productID, int vendorID)
    {
      var product = _scope.Repository<VendorAssortment>().GetSingle(x => x.VendorID == vendorID && x.ProductID == productID);

      if (product == null)
      {
        var productMatch = (from pm in _scope.Repository<ProductMatch>().GetAllAsQueryable()
                            join ppm in _scope.Repository<ProductMatch>().GetAllAsQueryable() on pm.ProductMatchID equals ppm.ProductMatchID
                            where pm.ProductID == productID
                            && ppm.ProductID != productID
                            && ppm.isMatched
                            && ppm.Product.VendorAssortments.Any(x => x.VendorID == vendorID)
                            select pm).FirstOrDefault();

        if (productMatch != null)
          product = productMatch.Product.VendorAssortments.FirstOrDefault(x => x.VendorID == vendorID);
      }

      return product != null ? product.CustomItemNumber : string.Empty;
    }

    public string GetVendorItemNumber(Product product, string customItemNumber, int vendorID)
    {
      var assortmentRepository = _scope.Repository<VendorAssortment>();

      if (product != null)
      {
        var va = assortmentRepository.GetSingle(x => (x.VendorID == vendorID || (x.Vendor.ParentVendorID.HasValue && x.Vendor.ParentVendorID.Value == vendorID)) && x.ProductID == product.ProductID); //&& x.IsActive);

        if (va != null)
          return va.CustomItemNumber;

        va = VendorUtility.GetMatchedVendorAssortment(assortmentRepository, vendorID, product.ProductID);

        if (va != null)
          return va.CustomItemNumber;
      }

      return customItemNumber;
    }

    public decimal CalculatePrice(int productID, int quantity, Connector connector, PriceRuleType ruleType)
    {
      return GetCalculationPriceAndRule(productID, quantity, connector, ruleType).Value;
    }

    /// <summary>
    /// Returns calculated price and rounded price
    /// </summary>
    /// <param name="productID"></param>
    /// <param name="formula"></param>
    /// <param name="connectorID"></param>
    /// <returns></returns>
    public KeyValuePair<decimal, decimal> RoundPrice(int productID, string formula, int connectorID)
    {
      var f = new PriceRoundingFormulaParser(formula);

      var connector = _scope.Repository<Connector>().GetSingle(c => c.ConnectorID == connectorID);

      var calculatedPrice = CalculatePrice(productID, 1, connector, PriceRuleType.UnitPrice);
      var costPrice = CalculatePrice(productID, 1, connector, PriceRuleType.CostPrice);
      decimal price = calculatedPrice;
      if (costPrice == 0)
      {
        var vendorPrices = _scope.Repository<VendorAssortment>().GetSingle(c => c.ProductID == productID).VendorPrices;
        vendorPrices.ThrowIfNull("This product contains no vendor prices.");
        costPrice = vendorPrices.FirstOrDefault(c => c.MinimumQuantity == 0 || c.MinimumQuantity == 1).Price.Try(c => c.Value, 0);
      }
      costPrice.ThrowIf(c => c == 0, "The cost price of this product is 0");

      if (f.IsMargin)
      {
        var margin = (calculatedPrice - costPrice) / (costPrice / 100);
        //apply formula and round
        var condition = f.Formula.Replace("m", margin.ToString());

        var isTrue = (Boolean)EvalHelper.Eval(condition);
        if (isTrue)
        {
          //round price
          int p = (int)calculatedPrice;
          price = (decimal)p + f.RoundValue;
        }
      }
      else
      {
        //apply formula to fixed value && round
        var condition = f.Formula.Replace("x", calculatedPrice.ToString());
        condition = condition.Replace("*", ((int)calculatedPrice).ToString()); //replace * with whole number
        var isTrue = (Boolean)EvalHelper.Eval(condition);
        if (isTrue)
        {
          //round price
          int p = (int)calculatedPrice;
          price = (decimal)p + f.RoundValue;
        }
      }
      return new KeyValuePair<decimal, decimal>(calculatedPrice, price);

    }

    public KeyValuePair<int, decimal> GetCalculationPriceAndRule(int productID, int quantity, Connector connector, PriceRuleType ruleType)
    {

      //TODO: Add load options

      var repoContent = _scope.Repository<Content>();
      var repoAssortment = _scope.Repository<VendorAssortment>();

      var vendorAssortment = _scope.Repository<VendorAssortment>().GetAll(va => va.ProductID == productID).ToList();


      var contentProduct = repoContent.GetSingle(x => x.ProductID == productID && x.ConnectorID == connector.ConnectorID);

      if (contentProduct == null)
        return new KeyValuePair<int, decimal>(0, 0);

      var connectorPriceRules = (from cp in _scope.Repository<ContentPrice>().GetAllAsQueryable()
                                 join cpg in _scope.Repository<ContentProductGroup>().GetAllAsQueryable(x => x.MasterGroupMappingID != null)

                                 on cp.ConnectorID equals cpg.ConnectorID


                                 where (cp.ProductID.HasValue && cp.ProductID.Value == cpg.ProductID) &&
                                 cp.ConnectorID == connector.ConnectorID
                                 && cp.PriceRuleType == (int)ruleType
        && ((!cp.BrandID.HasValue || cp.BrandID.Value == contentProduct.Product.BrandID)
            || (!cp.ProductID.HasValue || cp.ProductID.Value == productID)
            || cpg.MasterGroupMapping.ProductGroupID == cp.ProductGroupID)
                                 select cp).OrderBy(x => x.ContentPriceRuleIndex).ToList();

      var vendorPrices = (from p in repoContent.GetAllAsQueryable()
                          join ass in repoAssortment.GetAllAsQueryable()
                          on new { p.ContentProduct.VendorID, p.ProductID } equals new { ass.VendorID, ass.ProductID }
                          where p.ProductID == productID && p.ConnectorID == connector.ConnectorID
                          select ass).SelectMany(x => x.VendorPrices);

      if (vendorPrices.Count() < 1)
        return new KeyValuePair<int, decimal>(0, 0);

      List<VendorPrice> price = vendorPrices.ToList();

      if (connectorPriceRules.Count() > 0)
      {
        foreach (var pRule in connectorPriceRules)
        {
          if (pRule.MinimumQuantity.HasValue)
          {
            price = price.Where(x => x.MinimumQuantity == pRule.MinimumQuantity.Value).ToList();
          }

          foreach (var p in price.Where(x => x.Price.HasValue))
          {
            if (pRule.FixedPrice.HasValue)
              p.Price = pRule.FixedPrice.Value;
            else
            {
              switch (ruleType)
              {
                case PriceRuleType.UnitPrice:
                  if (pRule.Margin == "%")
                  {
                    if (pRule.UnitPriceIncrease.HasValue)
                      p.Price = p.Price * pRule.UnitPriceIncrease.Value;

                    if (pRule.CostPriceIncrease.HasValue && p.CostPrice.HasValue)
                      p.Price = p.CostPrice * pRule.CostPriceIncrease;
                  }
                  else if (pRule.Margin == "+")
                  {
                    if (pRule.UnitPriceIncrease.HasValue)
                      p.Price = p.Price + pRule.UnitPriceIncrease.Value;

                    if (pRule.CostPriceIncrease.HasValue && p.CostPrice.HasValue)
                      p.Price = p.CostPrice + pRule.CostPriceIncrease;
                  }
                  break;
                case PriceRuleType.CostPrice:
                  if (pRule.Margin == "%")
                  {
                    if (pRule.UnitPriceIncrease.HasValue)
                      p.CostPrice = p.Price * pRule.UnitPriceIncrease.Value;

                    if (pRule.CostPriceIncrease.HasValue && p.CostPrice.HasValue)
                      p.CostPrice = p.CostPrice * pRule.CostPriceIncrease;
                  }
                  else if (pRule.Margin == "+")
                  {
                    if (pRule.UnitPriceIncrease.HasValue)
                      p.CostPrice = p.Price + pRule.UnitPriceIncrease.Value;

                    if (pRule.CostPriceIncrease.HasValue && p.CostPrice.HasValue)
                      p.CostPrice = p.CostPrice + pRule.CostPriceIncrease;
                  }
                  break;
              }
            }

            p.CalculatedPriceRuleID = pRule.ContentPriceRuleID;
          }
        }
      }

      var vendorList = (from vl in _scope.Repository<PreferredConnectorVendor>().GetAllAsQueryable()
                        where vl.ConnectorID == connector.ConnectorID
                        orderby vl.isPreferred descending
                        select vl).ToList();

      switch (ruleType)
      {
        case PriceRuleType.UnitPrice:
          var returnPrice = (from vass in price
                             join vl in vendorList on vass.VendorAssortment.VendorID equals vl.VendorID
                             where vass.Price != null
                             orderby vl.isPreferred descending
                             select vass).FirstOrDefault();

          if (returnPrice != null)
          {

            return new KeyValuePair<int, decimal>(returnPrice.CalculatedPriceRuleID, returnPrice.Price.Value);
          }
          break;
        case PriceRuleType.CostPrice:
          var returnCostPrice = (from vass in price
                                 join vl in vendorList on vass.VendorAssortment.VendorID equals vl.VendorID
                                 where vass.CostPrice != null
                                 orderby vl.isPreferred descending
                                 select vass).FirstOrDefault();

          if (returnCostPrice != null)
            return new KeyValuePair<int, decimal>(returnCostPrice.CalculatedPriceRuleID, returnCostPrice.CostPrice.Value);
          break;
      }
      return new KeyValuePair<int, decimal>(0, 0);

    }

    public List<VendorPriceResult> CalculatePrice(int productID)
    {
      List<VendorPriceResult> price = new List<VendorPriceResult>();

      price = (from vass in priceRules
               where vass.ProductID == productID
               select new VendorPriceResult
               {
                 Price = vass.PriceEx,
                 ConcentratorStatusID = vass.ConcentratorStatusID,
                 CommercialStatus = vass.CommercialStatus,
                 CostPrice = vass.CostPrice,
                 MinimumQuantity = vass.MinimumQuantity,
                 TaxRate = vass.TaxRate,
                 SpecialPrice = vass.SpecialPrice
               }).ToList();


      return price;
      //#endregion


    }

    public string CustomItemNumberList(int productID, Connector connector)
    {
      string customItemNumber = (from v in customItemNumbers
                                 where v.productid == productID
                                 orderby v.isPreferred descending
                                 select v.customitemnumber).FirstOrDefault();

      return customItemNumber;

    }

    public ContentLogicStock Stock(int productID, Connector connector)
    {

      var productStock = vendorStocks.Where(x => x.ProductID == productID);

      int quantityAvailible = productStock.Sum(x => x.QuantityOnHand);
      DateTime? pdd = null;
      pdd = productStock.Min(x => x.PromisedDeliveryDate);

      int quantityToReceive = productStock.Sum(x => x.QuantityToReceive ?? 0);

      string stockstatus = productStock.Select(x => x.StockStatus).FirstOrDefault() ?? String.Empty;


      ContentLogicStock stock = new ContentLogicStock()
      {
        QuantityAvailible = quantityAvailible,
        PromisedDeliveryDate = pdd,
        QuantityToReceive = quantityToReceive,
        StockStatus = stockstatus,
        ConcentratorStockStatusID = productStock.Select(c => c.ConcentratorStatusID.HasValue ? c.ConcentratorStatusID.Value : 0).FirstOrDefault()
      };

      return stock;
    }

    public List<VendorStockResult> RetailStock(int productID, Connector connector)
    {
      var retailstock = (from a in vendorRetailStocks
                         where a.ProductID == productID
                         select a).Distinct().ToList();
      return retailstock;
    }
  }

  public class ContentLogicStock
  {
    public int QuantityAvailible { get; set; }
    public DateTime? PromisedDeliveryDate { get; set; }
    public int? QuantityToReceive { get; set; }
    public string StockStatus { get; set; }
    public int ConcentratorStockStatusID { get; set; }
  }
}
