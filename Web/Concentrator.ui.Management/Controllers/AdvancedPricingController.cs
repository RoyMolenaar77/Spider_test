using System;
using System.Linq;
using System.Web.Mvc;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Contents;

namespace Concentrator.ui.Management.Controllers
{
  public class AdvancedPricingController : BaseController
  {

    public ActionResult GetList()
    {

      using (var work = GetUnitOfWork())
      {
        Client.User.ConnectorID.ThrowIfNull("Connector id is needed for this operation");

        var pricing = ((IProductService)work.Service<Product>()).FillPriceInformation(Client.User.ConnectorID.Value);
        var result = List(unit => (from cpv in pricing.CalculatedPriceView
                                   //group acv by new { acv.CustomItemNumber, acv.ShortDescription, cpv.PriceEx, cpv.CostPrice, cpv.CurrentRank, cpv.minPriceInc, cpv.maxPriceInc,acv.ProductID,acv.VendorID,acv.ConnectorID } into g
                                   select new
                                   {
                                     CustomItemNumber = cpv.CustomItemNumber,
                                     ProductTitel = cpv.ShortDescription,
                                     CurrentPrice = cpv.PriceEx ?? 0m,
                                     CurrentCostPrice = cpv.CostPrice ?? 0m,
                                     CurrentMarge = cpv.Margin,
                                     CurrentRank = cpv.CurrentRank ?? 0,
                                     ScanDate = cpv.lastImport,
                                     NewPrice = cpv.PriceEx ?? 0m,
                                     NewMarge = ((cpv.PriceEx ?? 0m) - (cpv.CostPrice ?? 0m)),
                                     minPriceInc = cpv.minPriceInc ?? 0m,
                                     maxPriceInc = cpv.maxPriceInc ?? 0m,
                                     PromisedDeliveryDate = cpv.CompetitorStock,
                                     PriceLabel = cpv.ContentPriceLabel,
                                     fromDate = cpv.FromDate.ToNullOrLocal(),
                                     toDate = cpv.ToDate.ToNullOrLocal(),
                                     Price = cpv.PriceEx,
                                     Label = cpv.ContentPriceLabel,
                                     ProductID = cpv.ProductID,
                                     VendorID = cpv.VendorID,
                                     ConnectorID = Client.User.ConnectorID,
                                     cpv.ProductCompareSourceID,
                                     cpv.ComparePricePosition
                                   }).AsQueryable()
                                  );



        return result;
      }
    }

    public ActionResult Update(int id, decimal? NewPrice, decimal? NewMarge, int? CurrentRank, string PriceLabel)
    {
      using (var unit = GetUnitOfWork())
      {
        var RelevantContentPrice = unit.Service<ContentPrice>().Get(c => c.ProductID == id);

        int vendorID = unit.Service<Content>().Get(x => x.ProductID == id && x.ConnectorID == Client.User.ConnectorID.Value).ContentProduct.VendorID;

        if (RelevantContentPrice == null)
        {
          RelevantContentPrice = new ContentPrice();
          //newContentPrice.CompareSourceID = ProductCompareSourceID;
          RelevantContentPrice.ContentPriceRuleIndex = 0;
          RelevantContentPrice.ProductID = id;
          RelevantContentPrice.Margin = "%";
          RelevantContentPrice.VendorID = vendorID;
          RelevantContentPrice.ConnectorID = Client.User.ConnectorID.Value;
          RelevantContentPrice.PriceRuleType = 1;
          unit.Service<ContentPrice>().Create(RelevantContentPrice);

        }

        RelevantContentPrice.BrandID = null;
        RelevantContentPrice.ProductGroupID = null;
        RelevantContentPrice.FixedPrice = (NewPrice.HasValue) ? NewPrice : RelevantContentPrice.FixedPrice;
        RelevantContentPrice.UnitPriceIncrease = (NewMarge.HasValue) ? NewMarge : RelevantContentPrice.UnitPriceIncrease;
        RelevantContentPrice.ComparePricePosition = (CurrentRank.HasValue) ? CurrentRank : RelevantContentPrice.ComparePricePosition;
        RelevantContentPrice.ContentPriceLabel = (!String.IsNullOrEmpty(PriceLabel)) ? PriceLabel : RelevantContentPrice.ContentPriceLabel;

        unit.Save();
      }
      return Json(new
      {
        success = true
      });
    }
  }
}
