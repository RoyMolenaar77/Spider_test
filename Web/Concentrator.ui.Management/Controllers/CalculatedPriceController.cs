using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Concentrator.Web.Shared;
using Concentrator.Objects.Models.Users;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Web;
using Concentrator.Objects.Services.ServiceInterfaces;

namespace Concentrator.ui.Management.Controllers
{
  public class CalculatedPriceController : BaseController
  {
    //
    // GET: /CalculatedPrice/
    private List<int> _productList = new List<int>();

    private void GetMapping(ProductGroupMapping mapping)
    {
      var prods = mapping.ContentProductGroups.Select(x => x.ProductID);

      _productList.AddRange(prods);

      mapping.ChildMappings.ForEach((child, idx) =>
      {
        GetMapping(child);
      });
    }

    [RequiresAuthentication(Functionalities.GetCalculatedPrice)]
    public ActionResult GetAveragePositionList(int? brandID, int? vendorID, int? productGroupMappingID, int? productGroupID)
    {
      using (var unit = GetUnitOfWork())
      {

        var selectedMapping = unit.Service<ProductGroupMapping>().Get(x => x.ProductGroupMappingID == productGroupMappingID.Value);
        GetMapping(selectedMapping);

        var ComparePricePositionList = unit.Service<ContentPrice>().GetAll(c => c.ProductGroupID == productGroupID || (( (brandID.HasValue) ? c.BrandID == brandID.Value : false) ||( (vendorID.HasValue) ? c.VendorID == vendorID.Value : false)));

        var pricing = ((IProductService)unit.Service<Product>()).FillPriceInformation(Client.User.ConnectorID.Value);
        var CalculatedPriceViewList = pricing.CalculatedPriceView;
        var contentView = (from cv in unit.Service<ContentView>().GetAll(x => x.ConnectorID == Client.User.ConnectorID.Value && x.LanguageID == Client.User.LanguageID)
                           select new
                           {
                             cv.ProductID,
                             cv.ProductName,
                             cv.ShortContentDescription
                           }).ToList();
        var cs = unit.Service<ProductCompareSource>().GetAll(x => x.IsActive).ToList();

        var result = (from i in CalculatedPriceViewList
                      where (brandID.HasValue ? (i.BrandID == brandID.Value) : true)
                      && (vendorID.HasValue ? (i.VendorID == vendorID.Value) : true)
                      && _productList.Contains(i.ProductID)
                      let productName = contentView.FirstOrDefault(x => x.ProductID == i.ProductID)
                      select new
                      {
                        i.ProductID,
                        BrandID = brandID.HasValue ? brandID.Value : 0,
                        VendorID = vendorID.HasValue ? vendorID.Value : 0,
                        ProductGroupID = productGroupID.Value,
                        ProductDescription = productName != null ? (productName.ProductName ?? productName.ShortContentDescription) : i.VendorItemNumber,
                        i.minPriceInc,
                        i.maxPriceInc,
                        i.CurrentRank,
                        i.OwnPriceInc,
                        EditableRank = i.CurrentRank,
                        ProductCompareSourceID = i.ProductCompareSourceID,
                        Value = i.CompetitorSource,
                        i.ContentPriceLabel
                      }).ToList();

        var averageRank = result.Average(c => c.CurrentRank);

        var productsWithAverage = (from r in result
                                   let relevantContentPriceOnProduct = ComparePricePositionList.Where(c => c.ProductID == r.ProductID && c.CompareSourceID == r.ProductCompareSourceID).FirstOrDefault()
                                   let relevantContentPriceOnProductGroup = ComparePricePositionList.Where(c => (productGroupID.HasValue) ? c.ProductGroupID == productGroupID : false && c.CompareSourceID == r.ProductCompareSourceID).FirstOrDefault()
                                   let relevantContentPriceOnBrand = ComparePricePositionList.Where(c => (brandID.HasValue) ? c.BrandID == brandID : false && c.CompareSourceID == r.ProductCompareSourceID).FirstOrDefault()
                                   let relevantContentPriceOnVendor = ComparePricePositionList.Where(c => (vendorID.HasValue) ? c.VendorID == vendorID : false && c.CompareSourceID == r.ProductCompareSourceID).FirstOrDefault()
                                   let minRank = result.Min(c => c.CurrentRank)
                                   let maxRank = result.Max(c => c.CurrentRank)
                                   let GSR =
                                      (relevantContentPriceOnProductGroup != null && relevantContentPriceOnProductGroup.MinComparePricePosition != null)
                                      ? relevantContentPriceOnProductGroup.MinComparePricePosition
                                      : ((relevantContentPriceOnBrand != null && relevantContentPriceOnBrand.MinComparePricePosition != null)
                                      ? relevantContentPriceOnBrand.MinComparePricePosition
                                      : ((relevantContentPriceOnVendor != null && relevantContentPriceOnVendor.MinComparePricePosition != null)
                                      ? relevantContentPriceOnVendor.MinComparePricePosition
                                      : minRank))
                                   let GER = (relevantContentPriceOnProductGroup != null && relevantContentPriceOnProductGroup.MaxComparePricePosition != null)
                                      ? relevantContentPriceOnProductGroup.MaxComparePricePosition
                                      : ((relevantContentPriceOnBrand != null && relevantContentPriceOnBrand.MaxComparePricePosition != null)
                                      ? relevantContentPriceOnBrand.MaxComparePricePosition
                                      : ((relevantContentPriceOnVendor != null && relevantContentPriceOnVendor.MaxComparePricePosition != null)
                                      ? relevantContentPriceOnVendor.MaxComparePricePosition
                                      : maxRank))
                                   select new
                                   {
                                     r.ProductID,
                                     r.BrandID,
                                     r.VendorID,
                                     r.ProductGroupID,
                                     r.ProductDescription,
                                     r.minPriceInc,
                                     r.maxPriceInc,
                                     r.CurrentRank,
                                     r.OwnPriceInc,
                                     r.EditableRank,
                                     r.ProductCompareSourceID,
                                     r.Value,
                                     r.ContentPriceLabel,
                                     AverageRank = averageRank,
                                     ProductStartRank = (relevantContentPriceOnProduct != null && relevantContentPriceOnProduct.MinComparePricePosition != null)
                                       ? relevantContentPriceOnProduct.MinComparePricePosition : null,
                                     productEndRank = (relevantContentPriceOnProduct != null && relevantContentPriceOnProduct.MaxComparePricePosition != null)
                                       ? relevantContentPriceOnProduct.MaxComparePricePosition : null,
                                     GroupStartRank = GSR < GER ? GSR : null, // group start position
                                     GroupEndRank = GER > GSR ? GER : null // group end position
                                   }).AsQueryable();

        return List(productsWithAverage);
      }
    }

    [RequiresAuthentication(Functionalities.UpdateCalculatedPrice)]
    public ActionResult Update(
      int productGroupMappingID,
      int productGroupID,
      DateTime? startDate,
      DateTime? endDate,
      decimal? BottomMargin,
      int? MinComparePricePosition,
      int? MaxComparePricePosition,
      string ContentPriceLabel,
      int? brandID,
      int? vendorID,
      int? ContentPriceRuleIndex
      )
    {
      using (var unit = GetUnitOfWork())
      {
        var selectedMapping = unit.Service<ProductGroupMapping>().Get(x => x.ProductGroupMappingID == productGroupMappingID);
        GetMapping(selectedMapping);

        if (!vendorID.HasValue)
        {
          int productID = _productList.FirstOrDefault();
          vendorID = unit.Service<Content>().Get(x => x.ProductID == productID && x.ConnectorID == Client.User.ConnectorID.Value).ContentProduct.VendorID;
        }

        var contentPriceOfProductGroup = unit.Service<ContentPrice>().Get(c => c.ProductGroupID == productGroupID && c.ConnectorID == Client.User.ConnectorID);
        if (contentPriceOfProductGroup == null && (BottomMargin.HasValue || MinComparePricePosition.HasValue || MaxComparePricePosition.HasValue ))
        {
          contentPriceOfProductGroup = new ContentPrice();

          contentPriceOfProductGroup.ProductGroupID = productGroupID;
          contentPriceOfProductGroup.ContentPriceRuleIndex = (ContentPriceRuleIndex.HasValue) ? ContentPriceRuleIndex.Value : 0;
          contentPriceOfProductGroup.ContentPriceLabel = ContentPriceLabel;

          contentPriceOfProductGroup.VendorID = vendorID.Value;

          contentPriceOfProductGroup.ConnectorID = Client.User.ConnectorID.Value;

          contentPriceOfProductGroup.Margin = "%";
          contentPriceOfProductGroup.VendorID = vendorID.Value;
          contentPriceOfProductGroup.FromDate = (startDate.HasValue) ? startDate.Value.ToUniversalTime() : contentPriceOfProductGroup.FromDate;
          contentPriceOfProductGroup.ToDate = (endDate.HasValue) ? endDate.Value.ToUniversalTime() : contentPriceOfProductGroup.ToDate;
          contentPriceOfProductGroup.BottomMargin = (BottomMargin.HasValue) ? BottomMargin.Value : contentPriceOfProductGroup.BottomMargin;
          contentPriceOfProductGroup.MinComparePricePosition = (MinComparePricePosition.HasValue) ? MinComparePricePosition.Value : contentPriceOfProductGroup.MinComparePricePosition;
          contentPriceOfProductGroup.MaxComparePricePosition = (MaxComparePricePosition.HasValue) ? MaxComparePricePosition.Value : contentPriceOfProductGroup.MaxComparePricePosition;
          contentPriceOfProductGroup.BrandID = (brandID.HasValue) ? brandID.Value : contentPriceOfProductGroup.BrandID;
          contentPriceOfProductGroup.ContentPriceRuleIndex = (ContentPriceRuleIndex.HasValue) ? ContentPriceRuleIndex.Value : contentPriceOfProductGroup.ContentPriceRuleIndex;
          contentPriceOfProductGroup.PriceRuleType = 1;

          unit.Service<ContentPrice>().Create(contentPriceOfProductGroup);
          unit.Save();
        }
        else
        {
          contentPriceOfProductGroup.VendorID = vendorID.Value;
          contentPriceOfProductGroup.FromDate = (startDate.HasValue) ? startDate.Value.ToUniversalTime() : contentPriceOfProductGroup.FromDate;
          contentPriceOfProductGroup.ToDate = (endDate.HasValue) ? endDate.Value.ToUniversalTime() : contentPriceOfProductGroup.ToDate;
          contentPriceOfProductGroup.BottomMargin = (BottomMargin.HasValue) ? BottomMargin.Value : contentPriceOfProductGroup.BottomMargin;
          contentPriceOfProductGroup.MinComparePricePosition = (MinComparePricePosition.HasValue) ? MinComparePricePosition.Value : contentPriceOfProductGroup.MinComparePricePosition;
          contentPriceOfProductGroup.MaxComparePricePosition = (MaxComparePricePosition.HasValue) ? MaxComparePricePosition.Value : contentPriceOfProductGroup.MaxComparePricePosition;
          contentPriceOfProductGroup.BrandID = (brandID.HasValue) ? brandID.Value : contentPriceOfProductGroup.BrandID;
          contentPriceOfProductGroup.ContentPriceRuleIndex = (ContentPriceRuleIndex.HasValue) ? ContentPriceRuleIndex.Value : contentPriceOfProductGroup.ContentPriceRuleIndex;
          contentPriceOfProductGroup.ContentPriceLabel = ContentPriceLabel;
          contentPriceOfProductGroup.PriceRuleType = 1;
        }

        _productList.ForEach(productID =>
        {
          if (!vendorID.HasValue)
          {
            vendorID = unit.Service<Content>().Get(x => x.ProductID == productID && x.ConnectorID == Client.User.ConnectorID.Value).ContentProduct.VendorID;
          }

          contentPriceOfProductGroup = unit.Service<ContentPrice>().Get(c => c.ProductID == productID && c.ConnectorID == Client.User.ConnectorID);

          if (contentPriceOfProductGroup != null)
          {           
            contentPriceOfProductGroup.VendorID = vendorID.Value;
            contentPriceOfProductGroup.FromDate = (startDate.HasValue) ? startDate.Value.ToUniversalTime() : contentPriceOfProductGroup.FromDate;
            contentPriceOfProductGroup.ToDate = (endDate.HasValue) ? endDate.Value.ToUniversalTime() : contentPriceOfProductGroup.ToDate;
            contentPriceOfProductGroup.BottomMargin = (BottomMargin.HasValue) ? BottomMargin.Value : contentPriceOfProductGroup.BottomMargin;
            contentPriceOfProductGroup.MinComparePricePosition = (MinComparePricePosition.HasValue) ? MinComparePricePosition.Value : contentPriceOfProductGroup.MinComparePricePosition;
            contentPriceOfProductGroup.MaxComparePricePosition = (MaxComparePricePosition.HasValue) ? MaxComparePricePosition.Value : contentPriceOfProductGroup.MaxComparePricePosition;
            contentPriceOfProductGroup.BrandID = (brandID.HasValue) ? brandID.Value : contentPriceOfProductGroup.BrandID;
            contentPriceOfProductGroup.ContentPriceRuleIndex = (ContentPriceRuleIndex.HasValue) ? ContentPriceRuleIndex.Value : contentPriceOfProductGroup.ContentPriceRuleIndex;
            contentPriceOfProductGroup.ContentPriceLabel = ContentPriceLabel;
            contentPriceOfProductGroup.PriceRuleType = 1;
          }

          unit.Save();
        });
      }
      return Json(new
      {
        success = true
      });
    }

    [RequiresAuthentication(Functionalities.UpdateCalculatedPrice)]
    public ActionResult UpdateAveragePosition(int id, int EditableRank)
    {
      using (var unit = GetUnitOfWork())
      {
        var contentPrice = unit.Service<ContentPrice>().GetAll(c => c.ProductID == id && c.ConnectorID == Client.User.ConnectorID).FirstOrDefault();
        int vendorID = unit.Service<Content>().Get(x => x.ProductID == id && x.ConnectorID == Client.User.ConnectorID.Value).ContentProduct.VendorID;

        if (contentPrice == null)
        {
          contentPrice = new ContentPrice();
          //newContentPrice.CompareSourceID = ProductCompareSourceID;
          contentPrice.ContentPriceRuleIndex = 0;
          contentPrice.ProductID = id;
          contentPrice.Margin = "%";
          contentPrice.VendorID = vendorID;
          contentPrice.ConnectorID = Client.User.ConnectorID.Value;
          contentPrice.PriceRuleType = 1;
          unit.Service<ContentPrice>().Create(contentPrice);
        }
        contentPrice.ComparePricePosition = EditableRank;

        unit.Save();
      }

      return Json(new
      {
        success = true
      });
    }

    [RequiresAuthentication(Functionalities.GetCalculatedPrice)]
    public ActionResult GetAverageMarginList(int? brandID, int? vendorID, int? productGroupMappingID, int? productGroupID)
    {
      using (var unit = GetUnitOfWork())
      {
        var selectedMapping = unit.Service<ProductGroupMapping>().Get(x => x.ProductGroupMappingID == productGroupMappingID.Value);
        GetMapping(selectedMapping);

        var pricing = ((IProductService)unit.Service<Product>()).FillPriceInformation(Client.User.ConnectorID.Value);
        var CalculatedPriceViewList = pricing.CalculatedPriceView;
        var contentView = (from cv in unit.Service<ContentView>().GetAll(x => x.ConnectorID == Client.User.ConnectorID.Value && x.LanguageID == Client.User.LanguageID)
                           select new
                           {
                             cv.ProductID,
                             cv.ProductName,
                             cv.ShortContentDescription
                           }).ToList();

        var products = (from a in CalculatedPriceViewList
                        let productName = contentView.FirstOrDefault(x => x.ProductID == a.ProductID)
                        where (brandID.HasValue ? (a.BrandID == brandID.Value) : true)
                          && (vendorID.HasValue ? (a.VendorID == vendorID.Value) : true)
                          && _productList.Contains(a.ProductID)
                        select new
                        {
                          a.ProductID,
                          ProductDescription = productName != null ?
                           (productName.ProductName ?? productName.ShortContentDescription) : a.VendorItemNumber,
                          a.CostPrice,
                          PriceEx = a.PriceEx != null ? a.PriceEx : (Decimal)00.00,
                          Margin = a.BottomMargin
                        }).ToList();

        decimal? minMargin = products.Min(r => r.Margin);
        decimal? averageMargin = products.Average(cpv => cpv.PriceEx - cpv.CostPrice);

        var productsWithAverageAndMinMargin = (from r in products
                                               select new
                                               {
                                                 r.ProductID,
                                                 r.ProductDescription,
                                                 r.CostPrice,
                                                 r.PriceEx,
                                                 r.Margin,
                                                 MinMargin = minMargin,
                                                 AverageMargin = averageMargin
                                               }).AsQueryable();

        return List(productsWithAverageAndMinMargin);
      }
    }

    [RequiresAuthentication(Functionalities.UpdateCalculatedPrice)]
    public ActionResult UpdateAverageMargin(int id, decimal Margin, int? productGroupMappingID, int? productGroupID, int? brandID, int? vendorID)
    {
      using (var unit = GetUnitOfWork())
      {

        int VendorID = unit.Service<Content>().Get(x => x.ProductID == id && x.ConnectorID == Client.User.ConnectorID.Value).ContentProduct.VendorID;
        var ContentPrice = unit.Service<ContentPrice>().Get(c => c.ProductID == id);

        if (ContentPrice == null)
        {

          ContentPrice parentContentPrice = unit.Service<ContentPrice>().Get
            (c =>
              ((brandID.HasValue && brandID != 0) ? c.BrandID == brandID.Value : true)
              && ((productGroupID.HasValue && productGroupID != 0) ? c.ProductGroupID == productGroupID.Value : true)
              && c.ConnectorID == Client.User.ConnectorID.Value
              && c.VendorID == VendorID);

          if (parentContentPrice != null)
          {
            ContentPrice newContentPrice = new ContentPrice();
            newContentPrice.ProductID = id;
            newContentPrice.VendorID = parentContentPrice.VendorID;
            newContentPrice.ConnectorID = Client.User.ConnectorID.Value;
            newContentPrice.ContentPriceRuleIndex = parentContentPrice.ContentPriceRuleIndex;
            newContentPrice.ComparePricePosition = parentContentPrice.ComparePricePosition;
            newContentPrice.CompareSourceID = parentContentPrice.ComparePricePosition;
            newContentPrice.ContentPriceCalculationID = parentContentPrice.ContentPriceCalculationID;
            newContentPrice.CostPriceIncrease = parentContentPrice.CostPriceIncrease;
            newContentPrice.FixedPrice = parentContentPrice.FixedPrice;
            newContentPrice.FromDate = parentContentPrice.FromDate;
            newContentPrice.Margin = parentContentPrice.Margin;
            newContentPrice.MaxComparePricePosition = parentContentPrice.MaxComparePricePosition;
            newContentPrice.MinComparePricePosition = parentContentPrice.MinComparePricePosition;
            newContentPrice.MinimumQuantity = parentContentPrice.MinimumQuantity;
            newContentPrice.PriceRuleType = parentContentPrice.PriceRuleType;
            newContentPrice.BottomMargin = Margin;
            newContentPrice.UnitPriceIncrease = parentContentPrice.UnitPriceIncrease;
            newContentPrice.PriceRuleType = 1;
            unit.Service<ContentPrice>().Create(newContentPrice);
          }
          else
          {
            ContentPrice newContentPrice = new ContentPrice();
            newContentPrice.ContentPriceRuleIndex = 0;
            newContentPrice.ProductID = id;
            newContentPrice.BottomMargin = Margin;
            newContentPrice.Margin = "%";
            newContentPrice.VendorID = VendorID;
            newContentPrice.ConnectorID = Client.User.ConnectorID.Value;
            newContentPrice.PriceRuleType = 1;
            unit.Service<ContentPrice>().Create(newContentPrice);
          }
          unit.Save();
        }
        else
          ContentPrice.BottomMargin = Margin;

        unit.Save();

      }

      return GetAverageMarginList(brandID, vendorID, productGroupMappingID, productGroupID);

    }

    [RequiresAuthentication(Functionalities.CreateCalculatedPrice)]
    public ActionResult Create()
    {
      return Create<ContentPrice>();
    }

    [RequiresAuthentication(Functionalities.GetCalculatedPrice)]
    public ActionResult GetCompareData(int productID)
    {
      return List(context => (from i in context.Service<ProductCompetitorPrice>().GetAll()
                              where i.ProductID == productID
                              select new
                              {
                                i.ProductCompetitorPriceID,
                                i.ProductCompetitorMappingID,
                                Competitor = i.ProductCompetitorMapping.Competitor,
                                i.CompareProductID,
                                i.ProductID,
                                ProductDescription = i.Product.ProductDescriptions.Select(x => x.ShortContentDescription),
                                i.Price,
                                i.Stock
                              }).OrderBy(x => x.ProductCompetitorPriceID));
    }

    [RequiresAuthentication(Functionalities.GetCalculatedPrice)]
    public ActionResult GetWizardValues(int? productGroupID, int? brandID, int? vendorID)
    {
      return List(context => from i in context.Service<ContentPrice>().GetAll()
                             where i.ProductGroupID == productGroupID
                             select new
                             {
                               AverageMargin = i.Margin,
                               AveragePricePosition = i.MaxComparePricePosition
                             });
    }


    [RequiresAuthentication(Functionalities.GetCalculatedPrice)]
    public ActionResult GetActiveProductCompareSourceRadioButtons(int ProductID, int ProductCompareSourceID)
    {
      using (var unit = GetUnitOfWork())
      {
        var productCompareSources = unit.Service<ProductCompareSource>().GetAll(c => c.IsActive == true)
          .Select(c => new
          {
            InputValue = c.ProductCompareSourceID,
            BoxLabel = c.Source,
            Checked = c.ProductCompareSourceID == ProductCompareSourceID
          }).ToList();

        return Json(new
        {
          results = productCompareSources
        });
      }
    }


    [RequiresAuthentication(Functionalities.UpdateCalculatedPrice)]
    public ActionResult UpdateProductCompareSource(int ProductID, int ProductCompareSourceID, int? BrandID, int? ProductGroupID, int? EditableRank)
    {
      using (var unit = GetUnitOfWork())
      {
        var contentPrice = unit.Service<ContentPrice>().GetAll(c => c.ProductID == ProductID && c.ConnectorID == Client.User.ConnectorID).FirstOrDefault();
        int vendorID = unit.Service<Content>().Get(x => x.ProductID == ProductID && x.ConnectorID == Client.User.ConnectorID.Value).ContentProduct.VendorID;

        if (contentPrice == null)
        {
          contentPrice = new ContentPrice();
          contentPrice.ContentPriceRuleIndex = 0;
          contentPrice.ProductID = ProductID;
          contentPrice.Margin = "%";
          contentPrice.VendorID = vendorID;
          contentPrice.ConnectorID = Client.User.ConnectorID.Value;
          unit.Service<ContentPrice>().Create(contentPrice);
        }

        contentPrice.BrandID = null;
        contentPrice.ProductGroupID = null;
        //contentPrice.ProductGroupID = (ProductGroupID.HasValue) ? ProductGroupID : contentPrice.ProductGroupID;
        contentPrice.ComparePricePosition = EditableRank;
        contentPrice.PriceRuleType = 1;
        contentPrice.CompareSourceID = ProductCompareSourceID;

        unit.Save();
      }

      return Json(new
      {
        success = true
      });
    }

  }
}
