using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Objects.SqlClient;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

using PetaPoco;

namespace Concentrator.ui.Management.Controllers
{
  using Filters;
  using Objects;
  using Objects.DataAccess.UnitOfWork;
  using Objects.Environments;
  using Objects.Logic;
  using Objects.Lucene;
  using Objects.Models.Attributes;
  using Objects.Models.Connectors;
  using Objects.Models.Contents;
  using Objects.Models.Faq;
  using Objects.Models.Localization;
  using Objects.Models.Management;
  using Objects.Models.Media;
  using Objects.Models.Orders;
  using Objects.Models.Prices;
  using Objects.Models.Products;
  using Objects.Models.Slurp;
  using Objects.Models.Users;
  using Objects.Models.Vendors;
  using Objects.Services;
  using Objects.Services.ServiceInterfaces;
  using Objects.Web;
  using Objects.Web.Models;
  using Models;
  using Models.Anychart;
  using Web.Shared;
  using Web.Shared.Controllers;
  using Web.Shared.Models;

  public class ProductController : BaseController
  {
    public class CommercialOverviewModel
    {
      public String CustomItemNumber
      {
        get;
        set;
      }

      public String CommercialStatus
      {
        get;
        set;
      }

      public Int32 MinimumQuantity
      {
        get;
        set;
      }

      public Decimal? CostPrice
      {
        get;
        set;
      }

      public Decimal? UnitPrice
      {
        get;
        set;
      }

      public Decimal? SpecialPrice
      {
        get;
        set;
      }

      public Decimal? TaxRate
      {
        get;
        set;
      }

      public Int32 VendorAssortmentID
      {
        get;
        set;
      }

      public Int32 VendorID
      {
        get;
        set;
      }
    }

    [RequiresAuthentication(Functionalities.GetVendorPrice)]
    public ActionResult GetCommercialOverview(Int32 productID, Int32 vendorID, Int32 relatedProductTypeID)
    {
      //using (var database = new Database(Connection, Database.MsSqlClientProvider))
      using (var unit = GetUnitOfWork())
      {
        var vendorAssortments = unit.Scope
          .Repository<RelatedProduct>()
          .Include(relatedProduct => relatedProduct.RProduct)
          .GetAll(relatedProduct
            => relatedProduct.ProductID == productID
            && relatedProduct.RelatedProductTypeID == relatedProductTypeID
            && relatedProduct.IsActive
            && relatedProduct.IsConfigured)
          .SelectMany(relatedProduct => relatedProduct.RProduct.VendorAssortments)
          .ToArray();

        var result =
          from vendorAssortment in vendorAssortments
          from vendorPrice in vendorAssortment.VendorPrices
          orderby vendorAssortment.VendorID, vendorAssortment.CustomItemNumber
          select new CommercialOverviewModel
          {
            CommercialStatus = vendorPrice.CommercialStatus,
            CustomItemNumber = vendorAssortment.CustomItemNumber,
            MinimumQuantity = vendorPrice.MinimumQuantity,

            CostPrice = vendorPrice.CostPrice,
            UnitPrice = vendorPrice.Price,
            SpecialPrice = vendorPrice.SpecialPrice,
            TaxRate = vendorPrice.TaxRate,

            VendorAssortmentID = vendorAssortment.VendorAssortmentID,
            VendorID = vendorAssortment.VendorID
          };

        return List(result.AsQueryable());
      }
    }

    [RequiresAuthentication(Functionalities.UpdateVendorPrice)]
    public ActionResult UpdateCommercialOverview(Int32 _VendorAssortmentID, Int32 _MinimumQuantity, Decimal? CostPrice, Decimal? UnitPrice, Decimal? SpecialPrice, Decimal? TaxRate)
    {
      using (var unit = GetUnitOfWork())
      {
        var vendorPrice = unit.Scope
          .Repository<VendorPrice>()
          .GetSingle(vp => vp.VendorAssortmentID == _VendorAssortmentID && vp.MinimumQuantity == _MinimumQuantity);

        if (vendorPrice != null)
        {
          if (CostPrice.HasValue)
          {
            vendorPrice.CostPrice = CostPrice;
          }

          if (UnitPrice.HasValue)
          {
            vendorPrice.Price = UnitPrice;
          }

          if (SpecialPrice.HasValue)
          {
            vendorPrice.SpecialPrice = SpecialPrice;
          }

          if (TaxRate.HasValue)
          {
            vendorPrice.TaxRate = TaxRate;
          }
        }

        unit.Save();
      }

      return Success(String.Empty);
    }

    [RequiresAuthentication(Functionalities.GetProduct)]
    public ActionResult GetList(bool? missingDescription, ContentFilter filter, bool? IsConfigurable)
    {
      var vendorIDs = new List<int>();

      if (filter.RemainderIdentification != null)
      {
        vendorIDs = filter.RemainderIdentification.Split(',').Select(x => int.Parse(x)).ToList();
      }

      return List((unit) =>
      {
        var productList = (from p in unit.Service<Product>().GetAll()
                           let a = p.VendorAssortments.Select(x => x.VendorID)
                           let desc = p.ProductDescriptions.Where(c => c.LanguageID == Client.User.LanguageID).FirstOrDefault().ShortContentDescription
                           where (missingDescription.HasValue ? missingDescription == desc.Equals("") : true) &&

                           (IsConfigurable.HasValue ? p.IsConfigurable == IsConfigurable.Value : true) &&


                           ((filter.VendorIdentification.HasValue ? a.Contains(filter.VendorIdentification.Value) : true) ||
                           a.Any(x => vendorIDs.Contains(x)))
                           select new
                           {
                             p.ProductID,
                             p.ProductDescriptions.Where(c => c.LanguageID == Client.User.LanguageID).FirstOrDefault().ProductName,
                             ProductDescription = desc,
                             Barcode = p.ProductBarcodes.Select(c => c.Barcode).FirstOrDefault(),
                             BrandName = p.Brand.Name,
                             p.BrandID,
                             VendorID = p.SourceVendorID,
                             p.VendorItemNumber,
                             p.IsConfigurable,
                             CustomItemNumber = p.VendorAssortments.Where(x => x.ProductID == p.ProductID).Select(x => x.CustomItemNumber).FirstOrDefault()
                           });

        if (filter.PublishedProduct.HasValue)
        {
          return (from p in productList
                  join pp in unit.Service<ContentProductGroup>().GetAll(x => x.IsCustom == true) on p.ProductID equals pp.ProductID
                  select p);
        }
        else if (filter.UnpublishedProduct.HasValue)
        {
          return (from p in productList
                  join pp in unit.Service<ContentProductGroup>().GetAll(x => x.IsCustom == false) on p.ProductID equals pp.ProductID
                  select p);
        }
        else
        {
          return productList;
        }
      });
    }

    [RequiresAuthentication(Functionalities.GetMatchedProducts)]
    public ActionResult GetMatchedProducts(int productID, bool IsSearched = false)
    {
      using (var unit2 = GetUnitOfWork())
      {
        int typeID = unit2.Service<RelatedProductType>().Get(c => c.IsConfigured).RelatedProductTypeID;

        var product = unit2.Service<Product>().Get(c => c.ProductID == productID);

        var configuredAttributes = product.ProductAttributeMetaDatas.Select(c => c.AttributeID);
        var relatedProducts = product.RelatedProductsSource.Where(c => c.RelatedProductTypeID == typeID).Select(c => c.RProduct).ToList();

        bool checkConfigured = product.IsConfigurable;

        //TODO : refactor into dynamic 

        int colorAttributeID = 0;
        var colorAttribute = unit2.Service<ProductAttributeMetaData>().Get(c => c.AttributeCode.ToLower().Equals("color"));
        if (colorAttribute == null)
        {
          //override with attribute option
          int.TryParse(ConfigurationManager.AppSettings["Management.ColorAttributeID"], out colorAttributeID);
        }

        int sizeAttributeID = 0;
        var sizeAttribute = unit2.Service<ProductAttributeMetaData>().Get(c => c.AttributeCode.ToLower().Equals("size"));
        if (sizeAttribute == null)
        {
          //override with attribute option
          int.TryParse(ConfigurationManager.AppSettings["Management.SizeAttributeID"], out sizeAttributeID);
        }

        var results = (from p in relatedProducts
                       from pr in p.VendorAssortments.SelectMany(c => c.VendorPrices)
                       let colorAttributeValue = p.ProductAttributeValues.FirstOrDefault(c => c.AttributeID == colorAttributeID)
                       let sizeAttributeValue = p.ProductAttributeValues.FirstOrDefault(c => c.AttributeID == sizeAttributeID)
                       let color = colorAttributeValue == null ? string.Empty : colorAttributeValue.Value
                       let size = sizeAttributeValue == null ? string.Empty : sizeAttributeValue.Value
                       select new
                       {
                         p.ProductID,

                         Color = color,
                         Size = size,
                         CustomItemNumber = p.VendorItemNumber,
                         pr.VendorAssortment.Vendor.VendorID,
                         pr.Price,
                         pr.CostPrice
                       }).AsQueryable();

        var result = results.Filter(Request);

        int total = result.Count();
        var paged = GetPagedResult(result);

        return Json(new { results = paged, total, JsonRequestBehavior.AllowGet });
      }
    }

    [RequiresAuthentication(Functionalities.UpdateAllMatchedProducts)]
    public ActionResult UpdateAllMatchedProducts(int _ProductID, int _Index, string Description, int productID, int _StoreID)
    {
      using (var unit = GetUnitOfWork())
      {
        //update alle producten in concentproductMatch
        var relatedProducts = unit.Service<RelatedProduct>().GetAll(c => c.ProductID == productID).Select(c => c.RelatedProductID).ToList();

        foreach (var r in relatedProducts)
        {
          var prod = unit.Service<ContentProductMatch>().Get(x => x.ProductID == r && x.StoreID == _StoreID);
          if (prod == null)
            continue;
          prod.Description = Description;
          unit.Save();
        }

        return Json(new
        {
          Success = true
        });
      }
    }

    [RequiresAuthentication(Functionalities.GetProduct)]
    public ActionResult GetModels(int productID)
    {
      return List((unit) =>
            from b in unit.Service<RelatedProduct>().GetAll(c => c.ProductID == productID && c.RelatedProductTypeID == (int)RelatedProductTypeEnum.Model)
            let desc = b.RProduct.ProductDescriptions.FirstOrDefault(c => c.LanguageID == Client.User.LanguageID)
            let actualDesc = desc == null ? b.RProduct.ProductDescriptions.FirstOrDefault() : desc
            select new
            {
              ModelName = desc.ProductName,
              ModelDescription = desc.ShortContentDescription,
              desc.Product.VendorItemNumber
            }
        );
    }

    [RequiresAuthentication(Functionalities.DeleteProduct)]
    public ActionResult DeleteProductGroupVendor(int _ProductGroupVendorID, int productID, int _VendorID, int _VendorAssortmentID)
    {
      using (var unit = GetUnitOfWork())
      {
        var assortments = unit.Service<VendorAssortment>().GetAll(x => x.VendorAssortmentID == _VendorAssortmentID).ToList();

        assortments.ForEach((assortment, idx) =>
        {
          //var prod = unit.Service<ProductGroupVendor>().Get(c => c.ProductGroupVendorID == _ProductGroupVendorID);
          var pgv = assortment.ProductGroupVendors.FirstOrDefault(c => c.ProductGroupVendorID == _ProductGroupVendorID);
          assortment.ProductGroupVendors.Remove(pgv);
        });
        unit.Save();
      }

      return Success("Product group vendor successfully removed");
    }

    [RequiresAuthentication(Functionalities.DeleteProduct)]
    public ActionResult Delete(int _ProductID)
    {
      Delete<ConnectorPublication>(c => c.ProductID == _ProductID);
      Delete<ContentLedger>(c => c.ProductID == _ProductID);
      Delete<ContentPrice>(c => c.ProductID == _ProductID);
      Delete<ContentProduct>(c => c.ProductID == _ProductID);
      Delete<ContentProductGroup>(c => c.ProductID == _ProductID);
      Delete<ContentVendorSetting>(c => c.ProductID == _ProductID);
      Delete<MissingContent>(c => c.ConcentratorProductID == _ProductID);
      Delete<OrderLine>(c => c.ProductID == _ProductID);
      Delete<ProductAttributeValue>(c => c.ProductID == _ProductID);
      Delete<ProductCompetitorPrice>(c => c.ProductID == _ProductID);
      Delete<FaqProduct>(c => c.ProductID == _ProductID);
      Delete<ProductImage>(c => c.ProductID == _ProductID);
      Delete<ProductMatch>(c => c.ProductID == _ProductID);
      Delete<ProductMedia>(c => c.ProductID == _ProductID);
      Delete<ProductReview>(c => c.ProductID == _ProductID);
      Delete<SlurpQueue>(c => c.ProductID == _ProductID);
      Delete<SlurpSchedule>(c => c.ProductID == _ProductID);
      Delete<VendorAssortment>(c => c.ProductID == _ProductID);
      Delete<VendorFreeGood>(c => c.ProductID == _ProductID);
      Delete<VendorPriceRule>(c => c.ProductID == _ProductID);
      Delete<VendorStock>(c => c.ProductID == _ProductID);
      //Delete<RelatedProduct>(c => c.RelatedProductID == _ProductID);
      Delete<RelatedProduct>(c => c.ProductID == _ProductID);
      return Delete<Product>(c => c.ProductID == _ProductID);
    }

    [RequiresAuthentication(Functionalities.CreateProduct)]
    public ActionResult CreateProductGroupVendor(int productID, string VendorProductGroupCode1, string VendorProductGroupCode2,
                                                 string VendorProductGroupCode3, string VendorProductGroupCode4, string VendorProductGroupCode5,
                                                 string BrandCode, string VendorName, int ProductGroupID)
    {
      using (var unit = GetUnitOfWork())
      {
        var m = unit.Service<ProductGroupVendor>().Get(
                (x => x.ProductGroupID == ProductGroupID
                               && ((x.VendorName == null && VendorName == null) || x.VendorName.Equals(VendorName))
                      &&
                      (((x.VendorProductGroupCode1 == null && VendorProductGroupCode1 == null) || x.VendorProductGroupCode1 == VendorProductGroupCode1)
                        || ((x.VendorProductGroupCode2 == null && VendorProductGroupCode2 == null) || x.VendorProductGroupCode2 == VendorProductGroupCode2)
                        || ((x.VendorProductGroupCode3 == null && VendorProductGroupCode3 == null) || x.VendorProductGroupCode3 == VendorProductGroupCode3)
                        || ((x.VendorProductGroupCode4 == null && VendorProductGroupCode4 == null) || x.VendorProductGroupCode4 == VendorProductGroupCode4)
                        || ((x.VendorProductGroupCode5 == null && VendorProductGroupCode5 == null) || x.VendorProductGroupCode5 == VendorProductGroupCode5)
                    )
                    && ((x.BrandCode == null && BrandCode == null) || x.BrandCode == BrandCode)
                    )
          );

        if (m == null)
        {
          m = new ProductGroupVendor()
          {
            VendorID = int.Parse(ConfigurationManager.AppSettings["ConcentratorVendorID"]),
            VendorAssortments = new List<VendorAssortment>()
          };
          unit.Service<ProductGroupVendor>().Create(m);
        }

        TryUpdateModel(m);

        var vendors = unit.Service<VendorAssortment>().GetAll(c => c.ProductID == productID);

        vendors.ForEach((x, idx) =>
        {
          m.VendorAssortments.Add(x);
        });

        unit.Save();
      }
      return Success("Product group vendor successfully added");
    }

    [RequiresAuthentication(Functionalities.GetProduct)]
    public ActionResult AdvancedSearch(string query)
    {
      AdvancedSearchIncludesModel includes = new AdvancedSearchIncludesModel();
      SetSpecialPropertyValues(includes);

      using (var unit = GetUnitOfWork())
      {
        var results = (from p in ((IProductService)unit.Service<Product>()).SearchProducts(Client.User.LanguageID, query, includes.includeDescriptions, includes.includeBrands, includes.includeIdentifiers, includes.includeProductGroups)
                       select new
                       {
                         p.ProductName,
                         p.ProductID,
                         p.imagepath
                       }).ToList();

        return Json(new
        {
          total = results.Count(),
          results = results.Skip(int.Parse(Request["start"])).Take(int.Parse(Request["limit"])).ToList()
        });
      }
    }

    [RequiresAuthentication(Functionalities.GetProduct)]
    public ActionResult Search(string query)
    {
      using (var unit = GetUnitOfWork())
      {
        var connectorID = Client.User.ConnectorID;
        var settings = unit
          .Service<ConnectorVendorManagementContent>()
          .GetAll(c => !connectorID.HasValue || connectorID.Value == c.ConnectorID)
          .ToArray();

        var allSettings =
          from a in unit.Service<Connector>().GetAll().Select(c => c.ConnectorID).ToArray()
          from v in unit.Service<Vendor>().GetAll().Select(c => c.VendorID).ToArray()
          select new ConnectorVendorManagementContent
          {
            VendorID = v,
            ConnectorID = a,
            IsDisplayed = false
          };

        if (settings.Any())
        {
          if (settings.Any(setting => setting.IsDisplayed))
          {
            foreach (var setting in allSettings)
            {
              if (settings.Any(p => p.ConnectorID == setting.ConnectorID && p.VendorID == setting.VendorID && p.IsDisplayed))
              {
                setting.IsDisplayed = true;
              }
            }
          }
          else
          {
            foreach (var setting in allSettings)
            {
              if (!settings.Any(p => p.ConnectorID == setting.ConnectorID && p.VendorID == setting.VendorID))
              {
                setting.IsDisplayed = true;
              }
            }
          }
        }
        else
        {
          foreach (var setting in allSettings)
          {
            setting.IsDisplayed = true;
          }
        }

        var indexer = new LuceneIndexer();

        if (settings.Any())
        {
          var result =
            from p in indexer.GetSearchResultsFromIndex(query)
            join va in unit.Service<VendorAssortment>().GetAll() on p.ProductID equals va.ProductID
            let setting = allSettings.FirstOrDefault(c => c.ConnectorID == connectorID.Value && c.VendorID == va.VendorID)
            where setting != null && setting.IsDisplayed
            select new
            {
              p.ProductID,
              ProductDescription = !string.IsNullOrEmpty(p.ProductName)
                ? p.ProductName
                : !string.IsNullOrEmpty(p.ProductDescription)
                  ? p.ProductDescription
                  : p.VendorItemNumber
            };

          return List(result.AsQueryable());
        }
        else
        {
          var result = indexer.GetSearchResultsFromIndex(query).Select(searchIndexModel => new
          {
            searchIndexModel.ProductID,
            ProductDescription = string.Format("{0} ({1})", searchIndexModel.VendorItemNumber, searchIndexModel.ProductName.Cap(20, true))
          });

          return List(result.AsQueryable());
        }
      }
    }

    [RequiresAuthentication(Functionalities.GetProduct)]
    public ActionResult SearchPerVendor(int ProductID)
    {
      return null;
    }

    [RequiresAuthentication(Functionalities.GetProduct)]
    public ActionResult GetName(int productID)
    {
      return Json(new { result = GetObject<ProductDescription>(c => c.ProductID == productID && c.LanguageID == (int)LanguageTypes.English).ProductName });
    }

    [RequiresAuthentication(Functionalities.GetProduct)]
    public ActionResult GetCalculatedPrice(int productID)
    {
      using (var unit = GetUnitOfWork())
      {
        var products = ((IContentPriceService)unit.Service<ContentPrice>()).GetCalculatedPrice(productID);

        return Json(new
        {
          success = true,
          results = products,
          total = products.Count
        });
      }
    }

    [RequiresAuthentication(Functionalities.GetProduct)]
    public ActionResult GetListByConnector()
    {
      int connectorID = Client.User.ConnectorID.Value;

      return List((unit) =>
                    from c in unit.Service<Content>().GetAll(c => c.ConnectorID == connectorID)
                    let p = c.Product
                    let desc = p.ProductDescriptions.Where(l => l.LanguageID == Client.User.LanguageID).FirstOrDefault().ShortContentDescription
                    select new
                    {
                      p.ProductID,
                      ProductName = p.ProductDescriptions.Where(l => l.LanguageID == Client.User.LanguageID).FirstOrDefault().ProductName,
                      ProductDescription = desc,
                      Barcode = p.ProductBarcodes.Select(l => l.Barcode).FirstOrDefault(),
                      BrandName = p.Brand.Name,
                      p.BrandID,
                      VendorID = p.SourceVendorID,
                      p.VendorItemNumber,
                      Configurable = p.IsConfigurable
                    });
    }

    [RequiresAuthentication(Functionalities.GetProduct)]
    public ActionResult GetListByVendor(bool? missingDescription)
    {

      return List((unit) =>
            from p in unit.Service<Product>().GetAll()
            let desc = p.ProductDescriptions.Where(c => c.LanguageID == Client.User.LanguageID).FirstOrDefault().ShortContentDescription
            where (missingDescription.HasValue ? missingDescription == desc.Equals("") : true)

            select new
            {
              p.ProductID,
              ProductName = p.ProductDescriptions.Where(c => c.LanguageID == Client.User.LanguageID).FirstOrDefault().ProductName,
              ProductDescription = desc,
              Barcode = p.ProductBarcodes.Select(c => c.Barcode).FirstOrDefault(),
              BrandName = p.Brand.Name,
              p.BrandID,
              VendorID = p.SourceVendorID,
              p.VendorItemNumber,
              CustomItemNumber = p.VendorAssortments.Where(x => x.ProductID == p.ProductID).Select(x => x.CustomItemNumber).FirstOrDefault()
            }
        );
    }

    [RequiresAuthentication(Functionalities.GetProduct)]
    public ActionResult GetProductDetails(int productID, bool isSearched = false)
    {
      using (var unit = GetUnitOfWork())
      {
        var product = unit.Service<Product>().Get(c => c.ProductID == productID);
        var media = product.ProductMedias.OrderBy(c => c.Sequence).FirstOrDefault();
        var mediaPath = media != null ? media.MediaPath : string.Empty;
        var mediaUrl = media != null ? media.MediaUrl : string.Empty;
        var productDesc = product.ProductDescriptions.FirstOrDefault(c => !string.IsNullOrEmpty(c.ProductName));
        var ProductName = productDesc != null ? productDesc.ProductName : string.Empty;
        var ShortContentDesc = productDesc != null ? productDesc.ShortContentDescription : string.Empty;
        var AveragePrice = product.VendorAssortments.SelectMany(c => c.VendorPrices.Select(x => x.Price)).Average();
        var CostPrice = product.VendorAssortments.SelectMany(c => c.VendorPrices.Select(x => x.CostPrice)).Average();

        var result = new
        {
          ProductID = isSearched
            ? product.ContentProductMatches.Select(c => c.Description).FirstOrDefault() ?? product.ProductID.ToString()
            : product.ProductID.ToString(),
          ProductName = isSearched
            ? product.ContentProductMatches.FirstOrDefault().Try(c => c.Description, ProductName)
            : ProductName,
          product.IsConfigurable,
          product.IsBlocked,
          Client.User.LanguageID,
          MediaPath = mediaPath,
          mediaUrl,
          ShortContentDesc,
          VendorItemNumber = product.VendorItemNumber,
          AveragePrice,
          CostPrice
        };

        return Json(new
        {
          success = true,
          product = result
        });
      }
    }

    [RequiresAuthentication(Functionalities.GetProductDescription)]
    public ActionResult GetPriceProgression(AdvancedPricingPortalFilter filter, bool? cp, int productID)
    {
      MergeSession(filter, AdvancedPricingPortalFilter.SessionKey);

      try
      {
        var series = new List<Serie>();

        using (var unit = GetUnitOfWork())
        {
          var productpricepoints = (from d in unit.Service<ContentLedger>().GetAll(x => x.LedgerDate >= (filter.FromDate != null ? filter.FromDate : DateTime.MinValue)
                            && x.ProductID == filter.ProductID && x.LedgerDate <= (filter.UntilDate != null ? filter.UntilDate : DateTime.MaxValue)
                            && x.UnitPrice.HasValue)
                                    select d).ToList().OrderBy(x => x.LedgerDate).Select(x => new PieChartPoint(x.LedgerDate.ToString("dd/MM"), x.UnitPrice.Value, "Blue", null)).ToList();

          var competitordata = (from d in unit.Service<ProductCompetitorPrice>().GetAll(x => (x.LastModificationTime.HasValue ? x.LastModificationTime.Value : x.CreationTime) >= (filter.FromDate != null ? filter.FromDate : DateTime.MinValue)
                            && x.ProductID == filter.ProductID && (x.LastModificationTime.HasValue ? x.LastModificationTime.Value : x.CreationTime) <= (filter.UntilDate != null ? filter.UntilDate : DateTime.MaxValue))
                                select d).ToList();

          var competitorpoints = (from d in competitordata
                                  group d by d.ProductCompetitorMapping.Competitor into grouped
                                  select new
                                  {
                                    Competitor = grouped.Key,
                                    Data = grouped.OrderBy(x => x.LastModificationTime).Select(x => new PieChartPoint(x.LastModificationTime.HasValue ? x.LastModificationTime.Value.ToString("dd/MM") : x.CreationTime.ToString("dd/MM"), x.Price, "", null))
                                  });

          if (cp.HasValue && cp.Value)
            competitorpoints.ForEach((x, idx) =>
            {
              series.Add(new Serie(new List<Point>(x.Data), x.Competitor, "Default"));
            });

          series.Add(new Serie(new List<Point>(productpricepoints), "Price Progression", "Default"));

        }
        return View("Anychart/DefaultLineChart", new AnychartComponentModel(series, "DateTime"));
      }
      catch (Exception ex)
      {
        return Failure("Unable to retrieve the price progression", ex);
      }
    }

    [RequiresAuthentication(Functionalities.GetProductDescription)]
    public ActionResult GetSalesProgression(AdvancedPricingPortalFilter filter)
    {
      MergeSession(filter, AdvancedPricingPortalFilter.SessionKey);

      try
      {
        var series = new List<Serie>();

        using (var unit = GetUnitOfWork())
        {
          var salesprogresspoints = (from m in unit.Service<OrderLine>().GetAll(x => x.Order.ReceivedDate >= (filter.FromDate != null ? filter.FromDate : DateTime.MinValue)
                            && x.ProductID == filter.ProductID && x.Order.ReceivedDate <= (filter.UntilDate != null ? filter.UntilDate : DateTime.MaxValue)
                            && x.Price.HasValue)
                                     select m).ToList().OrderBy(x => x.Quantity).Select(x => new PieChartPoint(x.Quantity.ToString(), x.Price.Value, "Blue", null)).ToList();

          var logic = ((IProductService)unit.Service<Product>()).FillPriceInformation(Client.User.ConnectorID.Value);
          var Price = logic.CalculatePrice(filter.ProductID).OrderBy(x => x.MinimumQuantity).FirstOrDefault();
          var costPrice = Price != null ? (Price.CostPrice.HasValue ? Price.CostPrice.Value : 0) : 0;

          var profitmarginpoints = (from m in unit.Service<OrderLine>().GetAll(x => x.Order.ReceivedDate >= (filter.FromDate != null ? filter.FromDate : DateTime.MinValue)
                                    && x.ProductID == filter.ProductID && x.Order.ReceivedDate <= (filter.UntilDate != null ? filter.UntilDate : DateTime.MaxValue)
                                    && x.Price.HasValue)
                                    join va in unit.Service<CalculatedPriceView>().GetAll() on m.ProductID equals va.ProductID
                                    where va.CostPrice != null
                                    select new { m, costPrice }).ToList().OrderBy(x => x.m.Quantity).Select(x => new PieChartPoint(x.m.Quantity.ToString(), ((decimal?)x.m.Price.Value - costPrice), "Blue", null)).ToList();

          series.Add(new Serie(new List<Point>(salesprogresspoints), "Sales Progression", "Default"));
          series.Add(new Serie(new List<Point>(profitmarginpoints), "Profit", "Default"));

          return View("Anychart/DefaultLineChart", new AnychartComponentModel(series));
        }
      }
      catch (Exception ex)
      {
        return Failure("Unable to retrieve the sales progression", ex);
      }
    }

    [RequiresAuthentication(Functionalities.GetProductDescription)]
    public ActionResult GetSalesCount(AdvancedPricingPortalFilter filter)
    {
      MergeSession(filter, AdvancedPricingPortalFilter.SessionKey);

      try
      {
        var series = new List<Serie>();

        using (var unit = GetUnitOfWork())
        {
          var allSalesCountPoints = unit.Service<OrderLine>().GetAll(x => x.Order.ReceivedDate >= (filter.FromDate != null ? filter.FromDate : DateTime.MinValue)
                        && x.ProductID == filter.ProductID && x.Order.ReceivedDate <= (filter.UntilDate != null ? filter.UntilDate : DateTime.MaxValue)
                        && x.Price.HasValue);

          var salescountpoints = (from m in allSalesCountPoints
                                  let TotalQuantity = allSalesCountPoints.Where(o => o.Order.ReceivedDate == m.Order.ReceivedDate).Sum(c => c.Quantity)
                                  select new { m, TotalQuantity }).ToList().OrderBy(x => x.m.Order.ReceivedDate).Select(x => new PieChartPoint(x.m.Order.ReceivedDate.ToString("dd/MM hh:mm"), x.TotalQuantity, "Blue", null)).ToList();

          series.Add(new Serie(new List<Point>(salescountpoints), "Sales Progression", "Default"));

          return View("Anychart/DefaultLineChart", new AnychartComponentModel(series));
        }
      }
      catch (Exception ex)
      {
        return Failure("Unable to retrieve the sales progression", ex);
      }
    }

    [RequiresAuthentication(Functionalities.CreateProduct)]
    public ActionResult Create()
    {
      return Create<Product>((unit, model) =>
      {
        var concVendorID = int.Parse(ConfigurationManager.AppSettings["ConcentratorVendorID"]);

        model.SourceVendorID = concVendorID;

        var customItemNumber = Request.Try(c => c["CustomItemNumber"], string.Empty);
        var va = unit.Service<VendorAssortment>().Get(x => x.CustomItemNumber == customItemNumber && x.VendorID == concVendorID);

        if (va != null)
        {
          throw new Exception("Product already exists");
        }

        va = new VendorAssortment()
        {
          Product = model,
          CustomItemNumber = Request.Try(c => c["CustomItemNumber"], string.Empty),
          ShortDescription = Request["Description"],
          VendorID = concVendorID
        };

        unit.Service<VendorAssortment>().Create(va);

        //.VendorAssortments.InsertOnSubmit(va);
        var productGroupID = int.Parse(Request["ProductGroupID"]);
        var productGroupVendor = unit.Service<ProductGroupVendor>().Get(c => c.ProductGroupID == productGroupID);
        if (productGroupVendor == null)
        {
          productGroupVendor = new ProductGroupVendor()
          {
            ProductGroupID = productGroupID,
            VendorID = concVendorID

          };
          unit.Service<ProductGroupVendor>().Create(productGroupVendor);
        }

        va.ProductGroupVendors = new List<ProductGroupVendor>();
        va.ProductGroupVendors.Add(productGroupVendor);

        //context.VendorProductGroupAssortments.InsertOnSubmit(new VendorProductGroupAssortment() { VendorAssortment = va, ProductGroupVendor = productGroupVendor });

        unit.Service<ProductDescription>().Create(new ProductDescription
        {
          Product = model,
          ShortContentDescription = Request["ProductDescription"],
          ProductName = Request["ProductName"],
          LanguageID = Client.User.LanguageID,
          VendorID = concVendorID
        });


        var attrService = unit.Service<ProductAttributeMetaData>();
        var attrValueService = unit.Service<ProductAttributeValue>();
        var allMandatory = attrService.GetAll(c => c.Mandatory);

        foreach (var att in allMandatory)
        {
          attrValueService.Create(new ProductAttributeValue()
          {
            AttributeID = att.AttributeID,
            LanguageID = 1,
            Product = model,
            Value = att.DefaultValue
          });
        };

      }, includesInResult: new List<string>() { "ProductID" });
      //}, includeInResponse: new List<string>() { "ProductID" });
    }

    [RequiresAuthentication(Functionalities.CreateProduct)]
    public ActionResult CreateFull(bool isSimple, string CustomItemNumber, string VendorItemNumber, int BrandID, int ProductGroupID, int VendorID)
    {

      try
      {
        using (var unit = GetUnitOfWork())
        {

          //product
          Product p = new Product()
          {
            BrandID = BrandID,
            VendorItemNumber = VendorItemNumber
          };

          unit.Service<Product>().Create(p);

          //vendor assortment
          VendorAssortment va = new VendorAssortment()
          {
            Product = p,
            VendorID = VendorID,
            CustomItemNumber = CustomItemNumber,
            IsActive = true
          };

          unit.Service<VendorAssortment>().Create(va);

          //descriptions. For every language -> three keys -> name, short description, long description
          var languages = GetPostedLanguagesMultiple();

          va.ShortDescription = languages.FirstOrDefault().Value[1];
          va.LongDescription = languages.FirstOrDefault().Value[2];

          //product group vendor
          var pgRepo = unit.Service<ProductGroupVendor>();
          ProductGroupVendor vendorPF = pgRepo.Get(c => c.ProductGroupID == ProductGroupID);
          if (vendorPF == null)
          {

            vendorPF = new ProductGroupVendor()
            {
              VendorID = VendorID,
              ProductGroupID = ProductGroupID,
              VendorAssortments = new List<VendorAssortment>()
            };

          }

          vendorPF.VendorAssortments.Add(va);

          var descService = unit.Service<ProductDescription>();
          var languageService = unit.Service<Language>();

          //product descriptions
          foreach (var lang in languages)
          {
            var language = languageService.Get(c => c.Name == lang.Key);

            var desc = new ProductDescription()
            {
              ModelName = lang.Value[0],
              ProductName = lang.Value[0],
              LongContentDescription = lang.Value[2],
              LongSummaryDescription = lang.Value[2],
              ShortContentDescription = lang.Value[1],
              ShortSummaryDescription = lang.Value[1],
              Product = p,
              VendorID = VendorID,
              Language = language
            };

            descService.Create(desc);
          }

          //price and stock for simple products
          if (isSimple)
          {
            VendorPrice vp = new VendorPrice()
            {
              MinimumQuantity = Request.Form["MinimumQuantity"].Try(c => int.Parse(c), 0),
              Price = Request.Form["UnitPrice"].Try(c => decimal.Parse(c), 0),
              CostPrice = Request.Form["CostPrice"].Try(c => decimal.Parse(c), 0),
              BasePrice = Request.Form["UnitPrice"].Try(c => decimal.Parse(c), 0),
              BaseCostPrice = Request.Form["UnitPrice"].Try(c => decimal.Parse(c), 0),
              VendorAssortment = va,
              LastUpdated = DateTime.Now,
              ConcentratorStatusID = Request.Form["ConcentratorPriceStatus"].Try(c => int.Parse(c), 0)
            };
            unit.Service<VendorPrice>().Create(vp);

            VendorStock vs = new VendorStock()
            {
              QuantityOnHand = Request.Form["QuantityOnStock"].Try(c => int.Parse(c), 0),
              VendorID = VendorID,
              Product = p,
              VendorStockTypeID = Request.Form["VendorStockType"].Try(c => int.Parse(c), 0),
              ConcentratorStatusID = Request.Form["ConcentratorStatusID"].Try(c => int.Parse(c), 0)
            };

            unit.Service<VendorStock>().Create(vs);
          }
          else
          {


            VendorPrice vp = new VendorPrice()
            {
              MinimumQuantity = 0,//Request.Form["MinimumQuantity"].Try(c => int.Parse(c), 0),
              Price = 0,//Request.Form["UnitPrice"].Try(c => decimal.Parse(c), 0),
              CostPrice = 0,//Request.Form["CostPrice"].Try(c => decimal.Parse(c), 0),
              BasePrice = 0,//Request.Form["UnitPrice"].Try(c => decimal.Parse(c), 0),
              BaseCostPrice = 0,//Request.Form["UnitPrice"].Try(c => decimal.Parse(c), 0),              
              VendorAssortment = va,
              LastUpdated = DateTime.Now,
              ConcentratorStatusID = Request.Form["ConcentratorPriceStatus"].Try(c => int.Parse(c), 0)
            };
            unit.Service<VendorPrice>().Create(vp);

            VendorStock vs = new VendorStock()
            {
              QuantityOnHand = 1,//Request.Form["QuantityOnStock"].Try(c => int.Parse(c), 0),
              VendorID = VendorID,
              Product = p,
              VendorStockTypeID = Request.Form["VendorStockType"].Try(c => int.Parse(c), 0),
              ConcentratorStatusID = Request.Form["ConcentratorStatusID"].Try(c => int.Parse(c), 0)
            };
            unit.Service<VendorStock>().Create(vs);
          }

          if (!isSimple)
          {

            p.IsConfigurable = true;
            //configurable products

            var skus = Request.Form.GetValues("SKU");
            var sizes = Request.Form.GetValues("Size");
            var colors = Request.Form.GetValues("Color");
            var stocks = Request.Form.GetValues("Stock");
            var pricesCost = Request.Form.GetValues("CostPrice");
            var pricesUnit = Request.Form.GetValues("UnitPrice");
            var minimumQtys = Request.Form.GetValues("MinimumQuantity");
            var priceStatuses = Request.Form.GetValues("ConcentratorPriceStatus");
            var stocktStatuses = Request.Form.GetValues("ConcentratorStatusID");
            var vendorStockStatuses = Request.Form.GetValues("VendorStockType");

            var attributeColor = unit.Service<ProductAttributeName>().Get(c => c.Name == "Color").ProductAttributeMetaData;
            var sizeAttribute = unit.Service<ProductAttributeName>().Get(c => c.Name == "Size").ProductAttributeMetaData;
            var typeID = unit.Service<RelatedProductType>().Get(c => c.IsConfigured == true).RelatedProductTypeID;
            p.ProductAttributeMetaDatas = new List<ProductAttributeMetaData>();

            p.ProductAttributeMetaDatas.Add(sizeAttribute);
            p.ProductAttributeMetaDatas.Add(attributeColor);

            for (var i = 0; i < skus.Length; i++)
            {
              //product
              Product pSimple = new Product()
              {
                BrandID = BrandID,
                VendorItemNumber = skus[i]
              };

              //TODO
              RelatedProduct rp = new RelatedProduct()
              {
                SourceProduct = p,
                RProduct = pSimple,
                VendorID = VendorID,
                IsConfigured = true,
                RelatedProductTypeID = typeID
              };

              unit.Service<RelatedProduct>().Create(rp);


              ProductAttributeValue valColor = new ProductAttributeValue()
              {
                ProductAttributeMetaData = attributeColor,
                Product = pSimple,
                Value = colors[i]
              };

              unit.Service<ProductAttributeValue>().Create(valColor);


              ProductAttributeValue valSize = new ProductAttributeValue()
              {
                ProductAttributeMetaData = sizeAttribute,
                Product = pSimple,
                Value = sizes[i]
              };

              unit.Service<ProductAttributeValue>().Create(valSize);


              unit.Service<Product>().Create(p);

              //vendor assortment
              VendorAssortment vaSku = new VendorAssortment()
              {
                Product = pSimple,
                VendorID = VendorID,
                CustomItemNumber = skus[i],
                IsActive = true
              };



              unit.Service<VendorAssortment>().Create(vaSku);

              VendorPrice vpSimple = new VendorPrice()
            {
              MinimumQuantity = minimumQtys[i].Try(c => int.Parse(c), 0),//Request.Form["MinimumQuantity"].Try(c => int.Parse(c), 0),
              Price = pricesUnit[i].Try(c => decimal.Parse(c, 0)),//Request.Form["UnitPrice"].Try(c => decimal.Parse(c), 0),
              CostPrice = pricesCost[i].Try(c => decimal.Parse(c, 0)),//Request.Form["CostPrice"].Try(c => decimal.Parse(c), 0),
              BasePrice = pricesUnit[i].Try(c => decimal.Parse(c, 0)),//Request.Form["UnitPrice"].Try(c => decimal.Parse(c), 0),
              BaseCostPrice = pricesCost[i].Try(c => decimal.Parse(c, 0)),//Request.Form["UnitPrice"].Try(c => decimal.Parse(c), 0),
              ConcentratorStatusID = priceStatuses[i].Try(c => int.Parse(c, 0)),
              VendorAssortment = vaSku
            };
              unit.Service<VendorPrice>().Create(vpSimple);

              VendorStock vsSimple = new VendorStock()
              {
                QuantityOnHand = stocks[i].Try(c => int.Parse(c, 0)),//Request.Form["QuantityOnStock"].Try(c => int.Parse(c), 0),
                VendorID = VendorID,
                Product = pSimple,
                VendorStockTypeID = vendorStockStatuses[i].Try(c => int.Parse(c, 0)),
                ConcentratorStatusID = stocktStatuses[i].Try(c => int.Parse(c, 0)),
              };
              unit.Service<VendorStock>().Create(vsSimple);


              vaSku.ShortDescription = languages.FirstOrDefault().Value[1];
              vaSku.LongDescription = languages.FirstOrDefault().Value[2];

              //product group vendor

              ProductGroupVendor vendorPFSku = pgRepo.Get(c => c.ProductGroupID == ProductGroupID);
              if (vendorPFSku == null)
              {

                vendorPFSku = new ProductGroupVendor()
                {
                  VendorID = VendorID,
                  ProductGroupID = ProductGroupID,
                  VendorAssortments = new List<VendorAssortment>()
                };

              }

              vendorPFSku.VendorAssortments.Add(vaSku);



              //product descriptions
              foreach (var lang in languages)
              {
                var language = languageService.Get(c => c.Name == lang.Key);

                var desc = new ProductDescription()
                {
                  ModelName = lang.Value[0],
                  ProductName = lang.Value[0],
                  LongContentDescription = lang.Value[2],
                  LongSummaryDescription = lang.Value[2],
                  ShortContentDescription = lang.Value[1],
                  ShortSummaryDescription = lang.Value[1],
                  Product = pSimple,
                  VendorID = VendorID,
                  Language = language
                };

                descService.Create(desc);
              }

              var unitPricesValues = Request.Form.GetValues("UnitPrice").Select(c => c.Try(l => decimal.Parse(l, CultureInfo.InvariantCulture), 0)).ToArray();
              var costPricesValues = Request.Form.GetValues("CostPrice").Select(c => c.Try(l => decimal.Parse(l, CultureInfo.InvariantCulture), 0)).ToArray();
              var stockValues = Request.Form.GetValues("Stock").Select(c => c.Try(l => int.Parse(l, CultureInfo.InvariantCulture), 0)).ToArray();
              var minimumQuantityValues = Request.Form.GetValues("MinimumQuantity").Select(c => c.Try(l => int.Parse(l, CultureInfo.InvariantCulture), 0)).ToArray();

            }


          }
          unit.Save();
        }

        return Success("Product added");
      }
      catch (Exception e)
      {
        return Failure("Something went wrong ", e);
      }
    }

    [RequiresAuthentication(Functionalities.DeleteProductGroupMapping)]
    public ActionResult RemoveFromProductGroupMapping(int _ConnectorID, int _ProductID, int ProductGroupMappingID, bool? deleteFromChildConnectors)
    {
      if (!deleteFromChildConnectors.HasValue) deleteFromChildConnectors = true;

      return Delete<ContentProductGroup>(c => c.ConnectorID == _ConnectorID && c.ProductID == _ProductID && c.ProductGroupMappingID == ProductGroupMappingID,
      action: (unit, model) =>
      {
        if (deleteFromChildConnectors.HasValue && deleteFromChildConnectors.Value)
        {
          var connectors = unit.Service<Connector>().GetAll().ToList().Where(c => c.ParentConnectorID.HasValue && c.ParentConnectorID.Value == _ConnectorID).ToList();

          foreach (var child in connectors)
          {
            unit.Service<ContentProductGroup>().Delete(c => c.ConnectorID == child.ConnectorID && c.ProductID == _ProductID && c.ProductGroupMappingID == ProductGroupMappingID);
          }
        }
      });
    }

    [RequiresAuthentication(Functionalities.CreateConnectorPublication)]
    public ActionResult AddToConnectorPublication(string vendorItemNumber, bool propagate, int? connectorID)
    {
      if (!connectorID.HasValue) connectorID = Client.User.ConnectorID.Value;

      try
      {
        using (var unit = GetUnitOfWork())
        {
          var connectorIDs = unit.Service<Connector>().GetAll(c => c.ConnectorID == connectorID.Value || c.ParentConnectorID == connectorID.Value).Select(c => c.ConnectorID).ToList();

          foreach (var conID in connectorIDs)
          {
            ((IProductService)unit.Service<Product>()).AddToConnectorPublication(vendorItemNumber, conID, true);
          }
        }
        return Success("Successfully blocked product from export");
      }
      catch (Exception e)
      {
        return Failure("Something went wrong", e);
      }
    }

    [RequiresAuthentication(Functionalities.DeleteConnectorPublication)]
    public ActionResult RemoveFromConnectorPublication(string vendorItemNumber, bool propagate, int? connectorID)
    {
      if (!connectorID.HasValue) connectorID = Client.User.ConnectorID.Value;

      try
      {
        using (var unit = GetUnitOfWork())
        {
          var connectorIDs = unit.Service<Connector>().GetAll(c => c.ConnectorID == connectorID.Value || c.ParentConnectorID == connectorID.Value).Select(c => c.ConnectorID).ToList();

          foreach (var conID in connectorIDs)
          {
            ((IProductService)unit.Service<Product>()).RemoveFromConnectorPublication(vendorItemNumber, conID, true);
          }
        }
        return Success("Successfully unblocked product from export");
      }
      catch (Exception e)
      {
        return Failure("Something went wrong", e);
      }
    }

    [RequiresAuthentication(Functionalities.CreateProduct)]
    public ActionResult UpdateProductVisibility(string vendorItemNumber, bool visible)
    {
      return Update<Product>(c => c.VendorItemNumber == vendorItemNumber, properties: new string[] { "Visible" });
    }

  }
}
