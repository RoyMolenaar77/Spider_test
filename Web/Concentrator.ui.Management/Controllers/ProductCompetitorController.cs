using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Products;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Brands;


namespace Concentrator.ui.Management.Controllers
{
  public class ProductCompetitorController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetProductCompetitor)]
    public ActionResult GetProductLedgerList()
    {
      return List(unit => (from c in unit.Service<ProductCompetitorLedger>().GetAll()
                           let competitorName = c.ProductCompetitorPrice.ProductCompetitorMapping.Competitor
                           let productName = c.ProductCompetitorPrice.Product != null ?
                                           c.ProductCompetitorPrice.Product.ProductDescriptions.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                                           c.ProductCompetitorPrice.Product.ProductDescriptions.FirstOrDefault() : null
                           select new
                           {
                             c.ProductCompetitorLedgerID,
                             c.ProductCompetitorPriceID,
                             CompetitorName = competitorName,
                             ProductName = productName != null ? (productName.ProductName ?? productName.ShortContentDescription) : c.ProductCompetitorPrice.Product != null ?
                                           c.ProductCompetitorPrice.Product.VendorItemNumber : null,
                             c.Stock,
                             c.Price,
                             c.CreatedBy,
                             CreationTime = c.CreationTime.ToLocalTime(),
                             LastModificationTime = c.LastModificationTime.ToNullOrLocal(),
                             c.LastModifiedBy
                           }));
    }

    [RequiresAuthentication(Functionalities.GetProductCompetitor)]
    public ActionResult Search(string query)
    {
      return Search(unit => from p in unit.Service<ProductCompetitor>().GetAll()
                            select new
                            {
                              p.ProductCompetitorID,
                              p.Name,
                            });
    }

    //public ActionResult GetDeliveryStatuses()
    //{
    //  return Json(new
    //  {
    //    results = (from c in Enums.Get<DeliveryStatus>().AsQueryable()
    //               select new
    //               {
    //                 DeliveryStatusName = c.ToString()
    //               }
    //             )
    //  });
    //}

    [RequiresAuthentication(Functionalities.GetProductCompetitor)]
    public ActionResult GetProductCompetitorList()
    {
      return List(unit => (from p in unit.Service<ProductCompetitor>().GetAll()
                           select new
                           {
                             p.ProductCompetitorID,
                             p.Name,
                             p.Reliability,
                             p.DeliveryDate,
                             p.ShippingCostPerOrder,
                             p.ShippingCost
                           }));
    }

    //public ActionResult GetProductCompareSourceList()
    //{
    //  using (var unit = GetUnitOfWork())
    //  {
    //    var list = unit.Service<ProductCompareSource>().GetAll().ToList();

    //    return Json(new
    //    {
    //      results = list
    //    });
    //  }
    //}

    [RequiresAuthentication(Functionalities.CreateProductCompetitorLedger)]
    public ActionResult CreateProductCompetitorLedger(int? ProductCompetitorID, int ProductCompareSourceID, string InTaxPrice, string IncludeShippingCost, string Competitor, int ProductID, long Price, string Stock)
    {
      //create ProductCompetitorMapping first and use specified sourceID for creating ProductCompetitorPrice
      //use that ID for creating the Ledger



      using (var unit = GetUnitOfWork())
      {
        try
        {
          var newMapping = new ProductCompetitorMapping()
          {
            ProductCompareSourceID = ProductCompareSourceID,
            InTaxPrice = InTaxPrice == "on" ? true : false,
            IncludeShippingCost = IncludeShippingCost == "on" ? true : false,
            Competitor = Competitor,
            ProductCompetitorID = ProductCompetitorID.HasValue ? ProductCompetitorID : null
          };
          unit.Service<ProductCompetitorMapping>().Create(newMapping);

          var newCompetitorPrice = new ProductCompetitorPrice()
          {
            CreationTime = DateTime.Now.ToUniversalTime(),
            ProductCompetitorMapping = newMapping,
            ProductID = ProductID,
            Price = Price,
            Stock = Stock.ToString(),
            LastImport = DateTime.Now.ToUniversalTime()
          };
          unit.Service<ProductCompetitorPrice>().Create(newCompetitorPrice);

          var newLedger = new ProductCompetitorLedger()
          {
            CreationTime = DateTime.Now.ToUniversalTime(),
            ProductCompetitorPrice = newCompetitorPrice,
            Stock = Stock.ToString(),
            Price = Price,
            CreatedBy = Concentrator.Objects.Web.Client.User.UserID
          };
          unit.Service<ProductCompetitorLedger>().Create(newLedger);

          unit.Save();

          return Success("Sucessfully created ProductCompetitorLedger");
        }
        catch (Exception ex)
        {
          return Failure("Failed to create ProductCompetitorLedger", ex);
        }
      }


    }

    [RequiresAuthentication(Functionalities.UpdateProductCompetitorLedger)]
    public ActionResult UpdateProductCompetitorLedger(int id, string CompetitorName, string Stock, decimal? Price)
    {
      using (var unit = GetUnitOfWork())
      {
        try
        {
          var toUpdate = unit.Service<ProductCompetitorLedger>().Get(x => x.ProductCompetitorLedgerID == id);
          if (!string.IsNullOrEmpty(CompetitorName))
          {
            toUpdate.ProductCompetitorPrice.ProductCompetitorMapping.Competitor = CompetitorName;
          }
          toUpdate.Stock = (!string.IsNullOrEmpty(Stock) ? Stock : toUpdate.Stock);
          toUpdate.Price = (Price.HasValue ? Price.Value : toUpdate.Price);
          unit.Save();
          return Success("Successfully updated ProductCompetitorLedger");
        }
        catch (Exception ex)
        {
          return Failure("Update failed", ex);
        }
      }
    }

    [RequiresAuthentication(Functionalities.DeleteProductCompetitorLedger)]
    public ActionResult DeleteProductCompetitorLedger(int id)
    {
      return Delete<ProductCompetitorLedger>(x => x.ProductCompetitorLedgerID == id);
    }

    [RequiresAuthentication(Functionalities.CreateProductCompetitor)]
    public ActionResult CreateProductCompetitor()
    {
      return Create<ProductCompetitor>();
    }

    [RequiresAuthentication(Functionalities.UpdateProductCompetitor)]
    public ActionResult UpdateProductCompetitor(int id)
    {
      return Update<ProductCompetitor>(x => x.ProductCompetitorID == id);
    }

    [RequiresAuthentication(Functionalities.DeleteProductCompetitor)]
    public ActionResult DeleteProductCompetitor(int id)
    {
      return Delete<ProductCompetitor>(x => x.ProductCompetitorID == id);
    }

    [RequiresAuthentication(Functionalities.GetProductCompetitor)]
    public ActionResult GetProductCompareList()
    {
      return List(unit => (from p in unit.Service<ProductCompare>().GetAll()
                           select new
                           {
                             p.CompareProductID,
                             p.ConnectorID,
                             ConnectorName = p.Connector.Name,
                             p.ConnectorCustomItemNumber,
                             p.VendorItemNumber,
                             p.MinPrice,
                             p.MaxPrice,
                             CreationTime = p.CreationTime.ToLocalTime(),
                             LastModificationTime = p.LastModificationTime.ToNullOrLocal(),
                             p.ProductCompareSourceID,
                             p.CreatedBy,
                             p.LastModifiedBy,
                             p.HotSeller,
                             p.PriceIndex,
                             p.UPID,
                             p.EAN,
                             p.SourceProductID,
                             p.AveragePrice,
                             p.TotalStock,
                             MinStock = p.MinStock.HasValue ? p.MinStock : 0,
                             MaxStock = p.MaxStock.HasValue ? p.MaxStock : 0,
                             p.PriceGroup1Percentage,
                             p.PriceGroup2Percentage,
                             p.PriceGroup3Percentage,
                             p.PriceGroup4Percentage,
                             p.PriceGroup5Percentage,
                             p.TotalSales,
                             p.Popularity,
                             p.Price,
                             p.LastImport
                           }));
    }

    [RequiresAuthentication(Functionalities.GetProductCompetitor)]
    public ActionResult GetPcSourceList()
    {
      return List(unit => (from i in unit.Service<ProductCompareSource>().GetAll()
                           select new
                           {
                             i.ProductCompareSourceID,
                             i.Source,
                             i.ProductCompareSourceParentID,
                             i.ProductCompareSourceType,
                             i.IsActive
                           }).AsQueryable());
    }


    [RequiresAuthentication(Functionalities.GetProductCompetitor)]
    public ActionResult GetProductCompetitorStore()
    {
      return Json(new
      {
        results = SimpleList<ProductCompetitor>(c => new
            {
              ProductCompetitorID = c.ProductCompetitorID,
              Name = c.Name
            }).ToArray()
      });
    }

    [RequiresAuthentication(Functionalities.GetProductCompetitor)]
    public ActionResult GetActiveProductCompareSourceStore()
    {
      using (var unit = GetUnitOfWork())
      {


        var productCompareSources = unit.Service<ProductCompareSource>().GetAll(c => c.IsActive == true)
          .Select(c => new
        {
          ProductCompareSourceID = c.ProductCompareSourceID,
          Value = c.Source
        }).ToList();

        return Json(new
        {
          results = productCompareSources
        });
      }
      //return Json(new
      //{
      //  results = SimpleList<ProductCompareSource>(c => new
      //  {
      //    ProductCompareSourceID = c.ProductCompareSourceID,
      //    Value = c.Source
      //  })
      //});
    }

    [RequiresAuthentication(Functionalities.GetProductCompetitor)]
    public ActionResult GetPcSourceStore()
    {
      return Json(new
      {
        results = SimpleList<ProductCompareSource>(c => new
        {
          ProductCompareSourceID = c.ProductCompareSourceID,
          Name = c.Source
        })
      });
    }


    [RequiresAuthentication(Functionalities.GetProductCompetitor)]
    public ActionResult GetProductCompetitorMapping()
    {
      return List(unit => from i in unit.Service<ProductCompetitorMapping>().GetAll()
                          select new
                          {
                            i.ProductCompetitorMappingID,
                            i.Competitor,
                            productCompareSourceID = i.ProductCompareSourceID,
                            Source = i.ProductCompareSource.Source,
                            ProductCompetitorID = i.ProductCompetitorID,
                            ProductCompetitorName = i.ProductCompetitor.Name,
                            i.IncludeShippingCost,
                            i.InTaxPrice
                          });
    }

    [RequiresAuthentication(Functionalities.UpdateProductCompetitor)]
    public ActionResult UpdateProductCompetitorMapping(int ID)
    {
      return Update<ProductCompetitorMapping>(c => c.ProductCompetitorMappingID == ID);
    }

    [RequiresAuthentication(Functionalities.GetProductCompetitor)]
    public ActionResult GetProductCompetitorPriceList()
    {
      return List(unit => (from i in unit.Service<ProductCompetitorPrice>().GetAll()
                           select new
                           {
                             i.ProductCompetitorPriceID,
                             i.ProductCompetitorMappingID,
                             i.CompareProductID,
                             i.ProductID,
                             i.Price,
                             i.Stock,
                             i.CreatedBy,
                             CreationTime = i.CreationTime.ToLocalTime(),
                             LastModificationTime = i.LastModificationTime.ToNullOrLocal(),
                             i.LastModifiedBy,
                             i.LastImport
                           }).AsQueryable());
    }
  }
}
