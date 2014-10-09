using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using System.Globalization;
using Concentrator.Objects;
using System.Data.Linq;
using Concentrator.Objects.Parse;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.DataAccess.UnitOfWork;

namespace Concentrator.Plugins.PriceComparison
{
  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="T">The type of the IContentProvider the subclass is expecting</typeparam>
  public abstract class BasePriceComparer<T> : ConcentratorPlugin
  {

    public BasePriceComparer()
    {

    }
    public abstract override string Name
    {
      get;
    }

    protected abstract int ConnectorID
    {
      get;
    }

    protected abstract int ProductCompareSourceID
    {
      get;
    }

    protected override void Process()
    {
      log.DebugFormat("Start Process competitorprices for {0}", Provider.ToString());
      SyncProducts(Provider);
      log.DebugFormat("Finish Process competitorprices for {0}", Provider.ToString());
    }

    protected abstract IContentRecordProvider<T> Provider
    {
      get;
    }

    protected abstract void SyncProducts(IContentRecordProvider<T> parser);

    protected virtual CultureInfo Cultures
    {
      get
      {
        CultureInfo info = new CultureInfo("nl-NL");
        info.NumberFormat.NumberGroupSeparator = ".";
        info.NumberFormat.NumberDecimalSeparator = ",";
        return info;
      }
    }



    private List<string> _existingContent;
    protected List<string> ExistingContent
    {
      get
      {
        if (this._existingContent == null || this._existingContent.Count() == 0)
        {
          this._existingContent = new List<string>();
          using (var unit = GetUnitOfWork())
          {
            this._existingContent = (from c in unit.Scope.Repository<Content>().GetAllAsQueryable()
                                     join va in unit.Scope.Repository<VendorAssortment>().GetAllAsQueryable()
                                     on c.ProductID equals va.ProductID
                                     where c.ConnectorID == this.ConnectorID
                                     select va.CustomItemNumber).Distinct().ToList();
          }
        }
        return this._existingContent;
      }
    }


    private List<ProductCompetitorMapping> _productCompetitors;
    protected List<ProductCompetitorMapping> ProductCompetitors
    {
      get
      {
        if (this._productCompetitors == null || this._productCompetitors.Count() == 0)
        {
          this._productCompetitors = new List<ProductCompetitorMapping>();
          using (var unit = GetUnitOfWork())
          {
            _productCompetitors = unit.Scope.Repository<ProductCompetitorMapping>().GetAll().ToList();
          }
        }
        return this._productCompetitors;
      }
    }

    private List<Content> _products;

    protected List<Content> ProductContent
    {
      get
      {
        if (this._products == null)
        {
          using (var unit = GetUnitOfWork())
          {
            this._products = unit.Scope.Repository<Content>().GetAll(p => p.ConnectorID == this.ConnectorID).ToList();
          }
        }
        return _products;
      }
    }

    protected abstract List<PriceCompareProperty> ColumnDefinitions
    {
      get;
    }


    #region Utility
    protected decimal? ParseDecimalValue(string prop, Dictionary<string, string> record)
    {
      string value = record.Where(c => c.Key == prop).First().Value.Replace("%", string.Empty).Replace("\"", string.Empty);
      if (!string.IsNullOrEmpty(value))
        return decimal.Parse(value, NumberStyles.Any, Cultures);
      return null;
    }

    protected int? ParseIntValue(string prop, Dictionary<string, string> record)
    {
      string value = record.Where(c => c.Key == prop).First().Value.Replace("%", string.Empty).Replace("\"", string.Empty);
      if (!string.IsNullOrEmpty(value))
        return int.Parse(value, NumberStyles.Any, Cultures);
      return null;
    }

    protected ProductCompetitorPrice SyncCompetitorPrice(string competitor, string price, int CompareProductID, IUnitOfWork unit, int productCompareSourceID)
    {
      competitor = competitor.Replace("\"", string.Empty).Trim();
      //competitor
      ProductCompetitorMapping pc = (from c in ProductCompetitors
                                     where c.Competitor.Trim() == competitor.Trim()
                                     select c).FirstOrDefault();
      if (pc == null)
      {
        pc = new ProductCompetitorMapping() { Competitor = competitor.Trim(), ProductCompareSourceID = productCompareSourceID, ProductCompetitorMappingID = -1 };
        unit.Scope.Repository<ProductCompetitorMapping>().Add(pc);
        unit.Save();
        ProductCompetitors.Add(pc);
      }
      //check price
      unit.Save();
      ProductCompetitorPrice pcp = (from c in unit.Scope.Repository<ProductCompetitorPrice>().GetAll()
                                    where
                                      c.CompareProductID == CompareProductID &&
                                      c.ProductCompetitorMapping.Competitor == competitor.Trim()
                                    select c).FirstOrDefault();

      unit.Save();
      if (pcp == null)
      {
        string VendorItemNumber = unit.Scope.Repository<ProductCompare>().GetSingle(c => c.CompareProductID == CompareProductID).VendorItemNumber;
        var Product = unit.Scope.Repository<Product>().GetSingle(p => p.VendorItemNumber == VendorItemNumber);
        int productID;
        if (Product != null)
        {
          productID = Product.ProductID;
        }
        else
        {
          Product = new Product();
          Product.BrandID = -1;
          Product.VendorItemNumber = VendorItemNumber;
          unit.Scope.Repository<Product>().Add(Product);
          unit.Save();
          productID = Product.ProductID;
        }
        pcp = new ProductCompetitorPrice()
                {
                  ProductCompetitorMappingID = pc.ProductCompetitorMappingID,
                  CompareProductID = CompareProductID,
                  Price = decimal.Parse(price, NumberStyles.Any, Cultures),
                  LastImport = DateTime.Now,
                  ProductID = productID
                      
                };
        unit.Scope.Repository<ProductCompetitorPrice>().Add(pcp);
        unit.Save();
      }
      else
      {
        if (pcp.Price != decimal.Parse(price, NumberStyles.Any, Cultures))
        {
          pcp = unit.Scope.Repository<ProductCompetitorPrice>().GetSingle(c => c.CompareProductID == CompareProductID &&
                   c.ProductCompetitorMapping.Competitor == competitor.Trim());

          pcp.Price = decimal.Parse(price, NumberStyles.Any, Cultures);

          unit.Save();
        }

      }
      ((IFunctionScope)unit.Scope).Repository().UpdateProductCompetitorPrice(pcp.CompareProductID.Value, pcp.ProductCompetitorMappingID);

      return pcp;

    }
    #endregion
  }
}
