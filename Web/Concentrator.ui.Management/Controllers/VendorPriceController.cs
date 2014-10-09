using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Vendors;
using System.Configuration;
using Concentrator.Objects.Models.Products;

namespace Concentrator.ui.Management.Controllers
{
  public class VendorPriceController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetVendorPrice)]
    public ActionResult GetList(int vendorAssortmentID)
    {
      return List(unit =>
                  from vp in unit.Service<VendorPrice>().GetAll()
                  where vp.VendorAssortmentID == vendorAssortmentID
                  select new
                  {
                    vp.VendorAssortmentID,
                    vp.Price,
                    vp.CostPrice,
                    vp.TaxRate,
                    vp.MinimumQuantity,
                    vp.CommercialStatus
                  });
    }

    [RequiresAuthentication(Functionalities.CreateVendorPrice)]
    public ActionResult CreateByVendorAssortment(int productID, int vendorID, bool isSearched = false)
    {
      if (isSearched)
      {
        List<int> underlyingProductIDs = GetAllUnderlyingProds(productID);

        if (underlyingProductIDs.Contains(productID))
        {
          underlyingProductIDs.Remove(productID);
        }

        foreach (int underlyingProductID in underlyingProductIDs)
        {
          Create<VendorPrice>((unit, vendorPrice) =>
          {
            var vendorAssortment = unit.Service<VendorAssortment>().Get(c => c.ProductID == underlyingProductID && c.VendorID == vendorID);

            var firstVa = unit.Service<VendorAssortment>().Get(c => c.ProductID == underlyingProductID);

            if (vendorAssortment == null)
            {

              vendorAssortment = new VendorAssortment()
              {
                ProductID = underlyingProductID,
                CustomItemNumber = firstVa.CustomItemNumber,
                VendorID = vendorID,
                ShortDescription = firstVa.ShortDescription,
                LongDescription = firstVa.LongDescription,
                LineType = firstVa.LineType,
                LedgerClass = firstVa.LedgerClass,
                ExtendedCatalog = firstVa.ExtendedCatalog,
                ProductDesk = firstVa.ProductDesk,
                IsActive = firstVa.IsActive,
                ZoneReferenceID = firstVa.ZoneReferenceID,
                ShipmentRateTableReferenceID = firstVa.ShipmentRateTableReferenceID
              };

              unit.Service<VendorAssortment>().Create(vendorAssortment);

              VendorStock venStock = new VendorStock
              {
                ProductID = underlyingProductID,
                VendorID = vendorID,
                QuantityOnHand = 0,
                VendorStockTypeID = 1
              };

              unit.Service<VendorStock>().Create(venStock);
              unit.Save();
            }

            vendorAssortment.ThrowIfNull("The vendor selected does not have any assortment from this item");

            var vendorAssortmentID = vendorAssortment.VendorAssortmentID;

            vendorPrice.VendorAssortmentID = vendorAssortmentID;

            unit.Service<VendorPrice>().Create(vendorPrice);
          });
        }
      }

      return Create<VendorPrice>((unit, vendorPrice) =>
      {
        var vendorAssortment = unit.Service<VendorAssortment>().Get(c => c.ProductID == productID && c.VendorID == vendorID);

        var firstVa = unit.Service<VendorAssortment>().Get(c => c.ProductID == productID);

        if (vendorAssortment == null)
        {

          vendorAssortment = new VendorAssortment()
          {
            ProductID = productID,
            CustomItemNumber = firstVa.CustomItemNumber,
            VendorID = vendorID,
            ShortDescription = firstVa.ShortDescription,
            LongDescription = firstVa.LongDescription,
            LineType = firstVa.LineType,
            LedgerClass = firstVa.LedgerClass,
            ExtendedCatalog = firstVa.ExtendedCatalog,
            ProductDesk = firstVa.ProductDesk,
            IsActive = firstVa.IsActive,
            ZoneReferenceID = firstVa.ZoneReferenceID,
            ShipmentRateTableReferenceID = firstVa.ShipmentRateTableReferenceID
          };
          vendorAssortment.ProductGroupVendors = new List<ProductGroupVendor>();
          unit.Service<VendorAssortment>().Create(vendorAssortment);

          firstVa.ProductGroupVendors.ForEach((pg, idx) =>
          {
            vendorAssortment.ProductGroupVendors.Add(pg);
          });

          VendorStock venStock = new VendorStock
          {
            ProductID = productID,
            VendorID = vendorID,
            QuantityOnHand = 0,
            VendorStockTypeID = 1
          };

          unit.Service<VendorStock>().Create(venStock);
          unit.Save();
        }

        vendorAssortment.ThrowIfNull("The vendor selected does not have any assortment from this item");

        var vendorAssortmentID = vendorAssortment.VendorAssortmentID;

        vendorPrice.VendorAssortmentID = vendorAssortmentID;

        unit.Service<VendorPrice>().Create(vendorPrice);
      });
    }

    [Obsolete("This method is obsolete. Replaced by CreateByVendorAssortment in product browser")]
    [RequiresAuthentication(Functionalities.CreateVendorPrice)]
    public ActionResult Create(int productID, int vendorID, int quantityOnHand, int vendorStockTypeID, int minimumQuantity)
    {

      return Create<VendorPrice>((unit, vendorPrice) =>
      {
        var assortmentService = unit.Service<VendorAssortment>();

        var va = assortmentService.Get(c => c.ProductID == productID && c.VendorID == vendorID);
        var firstVa = assortmentService.Get(c => c.ProductID == productID);

        if (va == null)
        {

          va = new VendorAssortment()
          {
            ProductID = productID,
            CustomItemNumber = firstVa.CustomItemNumber,
            VendorID = vendorID,
            ShortDescription = firstVa.ShortDescription,
            LongDescription = firstVa.LongDescription,
            LineType = firstVa.LineType,
            LedgerClass = firstVa.LedgerClass,
            ExtendedCatalog = firstVa.ExtendedCatalog,
            ProductDesk = firstVa.ProductDesk,
            IsActive = firstVa.IsActive,
            ZoneReferenceID = firstVa.ZoneReferenceID,
            ShipmentRateTableReferenceID = firstVa.ShipmentRateTableReferenceID
          };

          assortmentService.Create(va);
        }


        vendorPrice.VendorAssortment = va;
        vendorPrice.MinimumQuantity = minimumQuantity;
        unit.Service<VendorPrice>().Create(vendorPrice);

        VendorStock venStock = new VendorStock
        {
          ProductID = productID,
          VendorID = vendorID,
          QuantityOnHand = quantityOnHand,
          VendorStockTypeID = vendorStockTypeID
        };

        unit.Service<VendorStock>().Create(venStock);
      });
    }

    [RequiresAuthentication(Functionalities.UpdateVendorPrice)]
    public ActionResult Update(int _VendorAssortmentID, int _MinimumQuantity, bool? IsActive, string Price, string CostPrice, string SpecialPrice, int? StockStatus, int? CommercialStatus, bool isSearched = false)//, bool isSearched)
    {
      decimal unitPrice = !string.IsNullOrEmpty(Price) ? decimal.Parse(Price, System.Globalization.CultureInfo.InvariantCulture) : -1;
      decimal costPrice = !string.IsNullOrEmpty(CostPrice) ? decimal.Parse(CostPrice, System.Globalization.CultureInfo.InvariantCulture) : -1;
      decimal specialPrice = !string.IsNullOrEmpty(SpecialPrice) ? decimal.Parse(SpecialPrice, System.Globalization.CultureInfo.InvariantCulture) : -1;

      if (isSearched)
      {
        return Update<VendorPrice>(c => c.VendorAssortmentID == _VendorAssortmentID && c.MinimumQuantity == _MinimumQuantity, (unit, vendorPrice) =>
        {
          if (unitPrice > 0)
          {
            vendorPrice.Price = unitPrice;
          }

          if (costPrice > 0)
          {
            vendorPrice.CostPrice = costPrice;
          }

          if (specialPrice > 0)
          {
            vendorPrice.SpecialPrice = specialPrice;
          }

          if (CommercialStatus.HasValue)
          {
            vendorPrice.ConcentratorStatusID = CommercialStatus;
          }
        });
      }
      else
      {
        return Update<VendorPrice>(c => c.VendorAssortmentID == _VendorAssortmentID && c.MinimumQuantity == _MinimumQuantity, (unit, vendorPrice) =>
        {
          bool calculatePrice = false;

          if (unitPrice > 0)
          {
            vendorPrice.Price = unitPrice;
          }

          if (costPrice > 0)
          {
            vendorPrice.CostPrice = costPrice;
          }

          if (specialPrice > 0)
          {
            vendorPrice.SpecialPrice = specialPrice;
          }

          if (ConfigurationManager.AppSettings["CalculatePrice"] != null)
          {
            bool.TryParse(ConfigurationManager.AppSettings["CalculatePrice"], out calculatePrice);

            if (calculatePrice)
            {
              if (costPrice > 0)
              {
                if (vendorPrice.TaxRate == null)
                {
                  vendorPrice.Price = costPrice;
                }
                else
                {
                  vendorPrice.Price = costPrice * (1 + (vendorPrice.TaxRate / 100));
                }
              }

              if (unitPrice > 0)
              {
                if (vendorPrice.TaxRate == null)
                {
                  vendorPrice.CostPrice = unitPrice;
                }
                else
                {
                  vendorPrice.CostPrice = unitPrice / (1 + (vendorPrice.TaxRate / 100));
                }
              }
            }
          }

          if (CommercialStatus.HasValue)
          {
            vendorPrice.ConcentratorStatusID = CommercialStatus;
          }
        });
      }
    }

    [RequiresAuthentication(Functionalities.DeleteVendorPrice)]
    public ActionResult Delete(int id)
    {
      return Delete<VendorAssortment>(c => c.VendorAssortmentID == id, (unit, assortment) =>
      {
        unit.Service<VendorStock>().Delete(c => c.VendorID == assortment.VendorID && c.ProductID == assortment.ProductID);
      });
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult DeleteByAssortmentID(int _VendorAssortmentID, int _MinimumQuantity)
    {
      return Delete<VendorPrice>(c => c.VendorAssortmentID == _VendorAssortmentID && c.MinimumQuantity == _MinimumQuantity);
    }

    [RequiresAuthentication(Functionalities.GetVendorAssortment)]
    public ActionResult GetPricesByProduct(int productID)
    {
      return List(unit => unit.Scope
        .Repository<VendorAssortment>()
        .Include(vendorAssortment => vendorAssortment.VendorPrices)
        .GetAll(vendorAssortment => vendorAssortment.ProductID == productID)
        .SelectMany(vendorAssortment => vendorAssortment.VendorPrices)
        .Select(vendorPrice => new
        {
          CommercialStatus = vendorPrice.AssortmentStatus != null
            ? vendorPrice.AssortmentStatus.Status
            : "N/A",
          Vendor = vendorPrice.VendorAssortment.Vendor.Name,
          //VendorTypeName = String.Join(",", ((VendorType)vendorPrice.VendorAssortment.Vendor.VendorType).GetList<VendorType>().ToArray()),
          vendorPrice.VendorAssortmentID,
          vendorPrice.VendorAssortment.ProductID,
          vendorPrice.VendorAssortment.VendorID,
          vendorPrice.CostPrice,
          vendorPrice.Price,
          vendorPrice.SpecialPrice,
          vendorPrice.TaxRate,
          vendorPrice.MinimumQuantity,
          vendorPrice.VendorAssortment.CustomItemNumber
        })
        .AsQueryable());
    }
  }
}
