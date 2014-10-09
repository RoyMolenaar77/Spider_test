using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Concentrator.ui.Management.Controllers
{
  using Objects;
  using Objects.Models.Users;
  using Objects.Models.Products;
  using Objects.Models.Vendors;
  using Objects.Models.Statuses;
  using Web.Shared;
  using Web.Shared.Controllers;
  using Web.Shared.Models;

  public class VendorAssortmentController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetVendorAssortment)]
    public ActionResult GetList(int? productID = null, ContentFilter filter = null)
    {
      using (var unit = GetUnitOfWork())
      {
        var vendorAssortmentQuery = unit.Scope
          .Repository<Product>()
          .Include(product => product.VendorAssortments)
          .Include(product => product.VendorStocks)
          .GetAllAsQueryable(product => !productID.HasValue || productID == product.ProductID)
          .SelectMany(product => product.VendorAssortments)
          .Select(vendorAssortment => new
          {
            VendorItemNumber = vendorAssortment.Product.VendorItemNumber,
            VendorAssortmentID = vendorAssortment.VendorAssortmentID,
            VendorID = vendorAssortment.VendorID,
            ProductID = vendorAssortment.ProductID,
            CustomItemNumber = vendorAssortment.CustomItemNumber,
            ShortDescription = vendorAssortment.ShortDescription,
            LineType = vendorAssortment.LineType,
            IsActive = vendorAssortment.IsActive,
            //PromisedDeliveryDate = vendorAssortment.Product.VendorStocks.Min(x => x.PromisedDeliveryDate),
            //QuantityOnHand = vendorAssortment.Product.VendorStocks.Sum(x => x.QuantityOnHand),
            //QuantityToReceive = vendorAssortment.Product.VendorStocks.Sum(x => x.QuantityToReceive ?? 0),
            UnitCost = vendorAssortment.VendorPrices.Min(x => x.CostPrice),
            UnitPrice = vendorAssortment.VendorPrices.Min(x => x.Price)
          });

        return List(vendorAssortmentQuery);
      }
    }

    [RequiresAuthentication(Functionalities.GetVendorAssortment)]
    public ActionResult GetPricingAndStockOptions(int productID)
    {
      return List(unit => (from va in unit.Service<VendorAssortment>().GetAll().Where(x => x.ProductID == productID).ToList()
                           let vendorPrices = unit.Service<VendorPrice>().GetAll(c => c.VendorAssortmentID == va.VendorAssortmentID)
                           let stocks = unit.Service<VendorStock>().GetAll(c => c.VendorID == va.VendorID && c.ProductID == va.ProductID)
                           let stock = stocks.FirstOrDefault()
                           from vendorPrice in vendorPrices
                           select new
                           {
                             va.VendorAssortmentID,
                             va.ProductID,
                             Vendor = va.Vendor.Name,
                             CostPrice = vendorPrice != null ? vendorPrice.CostPrice : 0,
                             Price = vendorPrice != null ? vendorPrice.Price : 0,
                             Margin = (vendorPrice != null && vendorPrice.CostPrice.HasValue) ? vendorPrice.Price - vendorPrice.CostPrice : null,
                             TaxRate = vendorPrice != null ? vendorPrice.TaxRate : 0,
                             CommercialStatus = (vendorPrice != null && vendorPrice.AssortmentStatus != null) ? vendorPrice.AssortmentStatus.Status : "Unknown",
                             QuantityOnHand = stocks.Sum(x => (int?)x.QuantityOnHand),
                             VendorType = va.Vendor.VendorType,
                             VendorTypeName = String.Join(",", ((VendorType)va.Vendor.VendorType).GetList<VendorType>().ToArray()),
                             QuantityToReceive = stocks.Where(c => c.QuantityToReceive.HasValue).Sum(x => x.QuantityToReceive),
                             UnitCost = stocks.Sum(x => x.UnitCost),
                             PromisedDeliveryDate = stocks.Min(x => x.PromisedDeliveryDate).ToNullOrLocal(),
                             MinimumQuantity = vendorPrice != null ? vendorPrice.MinimumQuantity : 0,
                             StockStatus = (stock != null && stock.AssortmentStatus != null) ? stock.AssortmentStatus.Status : "Unknown",
                             IsActive = va.IsActive,
                             CustomItemNumber = va.CustomItemNumber
                           }).AsQueryable());
    }

    [RequiresAuthentication(Functionalities.GetVendorAssortment)]
    public ActionResult Search(string query)
    {
      return SimpleList((unit) => from c in unit.Service<AssortmentStatus>().GetAll()
                                  select new
                                  {
                                    c.StatusID,
                                    c.Status
                                  });
    }

    [RequiresAuthentication(Functionalities.GetVendorAssortment)]
    public ActionResult GetVendorStore(int productID)
    {
      return SimpleList((unit) => (from c in unit.Service<Vendor>().GetAll().ToList()
                                   where ((VendorType)c.VendorType).Has(VendorType.Assortment)
                                   select new
                                   {
                                     VendorID = c.VendorID,
                                     Vendor = c.Name
                                   }).AsQueryable());
    }

    [RequiresAuthentication(Functionalities.GetVendorAssortment)]
    public ActionResult GetProductStore(string query, int? vendorID)
    {
      return Search(unit => (from o in unit.Service<VendorAssortment>().Search(query).ToList()
                             where vendorID.HasValue ? o.VendorID == vendorID : true
                             select new
                             {
                               o.ProductID,
                               Product = o.Product.ProductDescriptions.Select(x => x.Description)
                             }).AsQueryable());
    }

    [RequiresAuthentication(Functionalities.GetVendorAssortment)]
    public ActionResult GetVendorAssortmentByID(int vendorID, int productID)
    {
      using (var unit = GetUnitOfWork())
      {
        var vendorAssortmentID = unit.Service<VendorAssortment>().Get(b => b.VendorID == vendorID && b.ProductID == productID).VendorAssortmentID;

        return Json(new
        {
          vendorAssortmentID = vendorAssortmentID,
          success = true
        });
      }

    }

    [RequiresAuthentication(Functionalities.UpdateVendorAssortment)]
    public ActionResult SetActive(int id, bool isActive)
    {

      try
      {
        using (var unit = GetUnitOfWork())
        {
          var assortment = unit.Service<VendorAssortment>().Get(c => c.VendorAssortmentID == id);
          assortment.IsActive = isActive;
          unit.Save();
        }
        return Success("Successfully updated item");
      }
      catch (Exception e)
      {
        return Failure("Something went wrong", e);
      }
    }

  }

  public class VendorAssortmentDto
  {
    public string VendorItemNumber { get; set; }

    public int VendorAssortmentID { get; set; }

    public int VendorID { get; set; }

    public int ProductID { get; set; }

    public string CustomItemNumber { get; set; }

    public string ShortDescription { get; set; }

    public string LineType { get; set; }

    public int QuantityOnHand { get; set; }

    public int QuantityToReceive { get; set; }

    public decimal? UnitCost { get; set; }

    public decimal? UnitPrice { get; set; }

    public DateTime? PromisedDeliveryDate { get; set; }

    public bool IsActive { get; set; }

    public VendorAssortmentDto()
    {
    }

    public VendorAssortmentDto(VendorAssortment vendorAssortment)
    {
      VendorItemNumber = vendorAssortment.Product.VendorItemNumber;
      VendorAssortmentID = vendorAssortment.VendorAssortmentID;
      VendorID = vendorAssortment.VendorID;
      ProductID = vendorAssortment.ProductID;
      CustomItemNumber = vendorAssortment.CustomItemNumber;
      ShortDescription = vendorAssortment.ShortDescription;
      LineType = vendorAssortment.LineType;
      IsActive = vendorAssortment.IsActive;

      if (vendorAssortment.Product.VendorStocks != null)
      {
        PromisedDeliveryDate = vendorAssortment.Product.VendorStocks.Min(x => x.PromisedDeliveryDate).ToNullOrLocal();
        QuantityOnHand = vendorAssortment.Product.VendorStocks.Sum(x => x.QuantityOnHand);
        QuantityToReceive = vendorAssortment.Product.VendorStocks.Sum(x => x.QuantityToReceive ?? 0);
      }

      if (vendorAssortment.VendorPrices != null)
      {
        UnitCost = vendorAssortment.VendorPrices.Min(x => x.CostPrice);
        UnitPrice = vendorAssortment.VendorPrices.Min(x => x.Price);
      }
    }
  }
}
