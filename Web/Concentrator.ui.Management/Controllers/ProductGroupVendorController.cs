using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Web.Shared.Models;

namespace Concentrator.ui.Management.Controllers
{
  public class ProductGroupVendorController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetProductGroupVendor)]
    public ActionResult GetListFiltered(int vendorAssortmentID)
    {
      return List(unit => (from p in unit.Service<ProductGroupVendor>().GetAll()
                           let productGroupName = p.ProductGroup.ProductGroupLanguages.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID)
                           where p.VendorAssortments.Any(x => x.VendorAssortmentID == vendorAssortmentID)
                           select new
                           {
                             p.ProductGroupVendorID,
                             p.ProductGroupID,
                             p.VendorID,
                             VendorName = p.Vendor.Name,
                             VendorProductGroupName = p.VendorName,
                             ProductGroupName = productGroupName != null ? productGroupName.Name : string.Empty,
                             p.VendorProductGroupCode1,
                             p.VendorProductGroupCode2,
                             p.VendorProductGroupCode3,
                             p.VendorProductGroupCode4,
                             p.VendorProductGroupCode5,
                             p.VendorProductGroupCode6,
                             p.VendorProductGroupCode7,
                             p.VendorProductGroupCode8,
                             p.VendorProductGroupCode9,
                             p.VendorProductGroupCode10,
                             p.BrandCode,
                           }).AsQueryable());
    }

    [RequiresAuthentication(Functionalities.GetProductGroupVendor)]
    public ActionResult GetListByProduct(int productID)
    {
      return List(unit =>
                  (from va in unit.Service<VendorAssortment>().GetAll(c => c.ProductID == productID)
                   let vendors = va.ProductGroupVendors
                   from productGroupVendor in vendors
                   select new
                   {
                     va.VendorAssortmentID,
                     productGroupVendor.ProductGroupVendorID,
                     productGroupVendor.ProductGroupID,
                     ProductGroupName = productGroupVendor.ProductGroup.ProductGroupLanguages.FirstOrDefault(e => e.LanguageID == Client.User.LanguageID).Name,
                     va.VendorID,
                     productGroupVendor.VendorProductGroupCode1,
                     productGroupVendor.VendorProductGroupCode2,
                     productGroupVendor.VendorProductGroupCode3,
                     productGroupVendor.VendorProductGroupCode4,
                     productGroupVendor.VendorProductGroupCode5,
                     productGroupVendor.VendorProductGroupCode6,
                     productGroupVendor.VendorProductGroupCode7,
                     productGroupVendor.VendorProductGroupCode8,
                     productGroupVendor.VendorProductGroupCode9,
                     productGroupVendor.VendorProductGroupCode10,
                     va.Vendor.Name,
                     productGroupVendor.BrandCode,
                     VendorProductGroupName = productGroupVendor.VendorName
                   }));



      //(from productGroupVendor in unit.Service<ProductGroupVendor>().GetAll(x => x.VendorAssortments.Any(y => y.ProductID == productID)).ToList()
      //       from va in unit.Service<VendorAssortment>().GetAll(x => x.ProductID == productID).ToList()
      //       where productGroupVendor.VendorAssortments.Any(x => x.ProductID == productID)               
      //       //let va = productGroupVendor.VendorAssortments.FirstOrDefault()               
      //       && va.ProductGroupVendors.Contains(productGroupVendor)
      //       select new
      //       {
      //         va.VendorAssortmentID,
      //         productGroupVendor.ProductGroupVendorID,
      //         productGroupVendor.ProductGroupID,
      //         ProductGroupName = productGroupVendor.ProductGroup.ProductGroupLanguages.FirstOrDefault(e => e.LanguageID == Client.User.LanguageID).Name,
      //         va.VendorID,
      //         productGroupVendor.VendorProductGroupCode1,
      //         productGroupVendor.VendorProductGroupCode2,
      //         productGroupVendor.VendorProductGroupCode3,
      //         productGroupVendor.VendorProductGroupCode4,
      //         productGroupVendor.VendorProductGroupCode5,
      //         productGroupVendor.VendorProductGroupCode6,
      //         productGroupVendor.VendorProductGroupCode7,
      //         productGroupVendor.VendorProductGroupCode8,
      //         productGroupVendor.VendorProductGroupCode9,
      //         productGroupVendor.VendorProductGroupCode10,
      //         va.Vendor.Name,
      //         productGroupVendor.BrandCode,
      //         VendorProductGroupName = productGroupVendor.VendorName
      //       }).Distinct().AsQueryable()
      //  );
    }

    [RequiresAuthentication(Functionalities.GetProductGroupVendor)]
    public ActionResult GetList(int? ProductGroupID, bool? initialUnmapped, ContentFilter filter)
    {
      var missingPgm = (from i in GetUnitOfWork().Service<ProductGroup>().GetAll(x => x.ProductGroupID < 0) select i.ProductGroupID).Distinct().ToList();
      var activePgm = (from i in GetUnitOfWork().Service<ProductGroup>().GetAll(x => x.ProductGroupID > 0) select i.ProductGroupID).Distinct().ToList();


      return List(unit =>
        (from p in unit.Service<ProductGroupVendor>().GetAll(p => ProductGroupID.HasValue ? p.ProductGroupID == ProductGroupID : true && (!initialUnmapped.HasValue || (initialUnmapped.HasValue && p.ProductGroupID == -1)))
         let a = p.ProductGroup.ProductGroupID
         let productGroupName = p.ProductGroup != null ?
                  p.ProductGroup.ProductGroupLanguages.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                  p.ProductGroup.ProductGroupLanguages.FirstOrDefault() : null
         where (filter.UnactivePgvIdentification.HasValue ? missingPgm.Contains(p.ProductGroupID) : true)
         ||
               (filter.ActivePgvIdentification.HasValue ? activePgm.Contains(p.ProductGroupID) : true)
         select new
         {
           p.ProductGroupVendorID,
           p.ProductGroupID,
           p.VendorID,
           VendorName = p.Vendor.Name,
           ProductGroupName = productGroupName != null ? productGroupName.Name : string.Empty,
           p.VendorProductGroupCode1,
           p.VendorProductGroupCode2,
           p.VendorProductGroupCode3,
           p.VendorProductGroupCode4,
           p.VendorProductGroupCode5,
           p.VendorProductGroupCode6,
           p.VendorProductGroupCode7,
           p.VendorProductGroupCode8,
           p.VendorProductGroupCode9,
           p.VendorProductGroupCode10,
           p.BrandCode,
           VendorProductGroupName = p.VendorName
         }).AsQueryable());
    }

    [RequiresAuthentication(Functionalities.UpdateProductGroupVendor)]
    public ActionResult Update(int _ProductGroupVendorID, int? Identification, int? ProductGroupID)
    {
      return Update<ProductGroupVendor>(c => c.ProductGroupVendorID == _ProductGroupVendorID);
    }

    [RequiresAuthentication(Functionalities.CreateProductGroupVendor)]
    public ActionResult Create()
    {
      return Create<ProductGroupVendor>();
    }

    [RequiresAuthentication(Functionalities.DeleteProductGroupVendor)]
    public ActionResult Delete(int _ProductGroupVendorID)
    {
      return Delete<ProductGroupVendor>(c => c.ProductGroupVendorID == _ProductGroupVendorID);
    }

    [RequiresAuthentication(Functionalities.GetProductGroupVendor)]
    public ActionResult ViewAssociatedConnectorProducts(int productGroupID)
    {
      return List(unit => from m in unit.Service<ProductGroupMapping>().GetAll(m => m.ProductGroupID == productGroupID)
                          join i in unit.Service<ContentProductGroup>().GetAll() on m.ProductGroupMappingID equals i.ProductGroupMappingID
                          select new
                          {
                            i.ProductID,
                            i.Product.BrandID,
                            i.Product.CreationTime,
                            ProductName = i.Product.ProductDescriptions.Select(x => x.ShortContentDescription).FirstOrDefault(),
                            BrandName = i.Product.Brand.Name,
                            i.Product.VendorItemNumber
                          });
    }

    [RequiresAuthentication(Functionalities.GetProductGroupVendor)]
    public ActionResult ViewAssociatedVendorProducts(int ProductGroupVendorID)
    {
      return List(unit => from m in unit.Service<VendorAssortment>().GetAll(m => m.ProductGroupVendors.Any(x => x.ProductGroupVendorID == ProductGroupVendorID))
                          let productName = m.Product.ProductDescriptions.Select(x => x.ProductName).FirstOrDefault()
                          let shortdesc = m.Product.ProductDescriptions.Select(x => x.ShortContentDescription).FirstOrDefault()
                          select new
                          {
                            m.ProductID,
                            m.Product.BrandID,
                            m.Product.CreationTime,
                            ProductName = !String.IsNullOrEmpty(shortdesc) ? shortdesc : productName != null ? productName : String.Empty,
                            BrandName = m.Product.Brand.Name,
                            m.Product.VendorItemNumber,
                            m.CustomItemNumber,
                            VendorName = m.Vendor.Name
                          });
    }
  }
  public class ProductGroupVendorEqualityComparer : IEqualityComparer<ProductGroupVendor>
  {
    #region IEqualityComparer<ProductGroupVendor> Members

    public bool Equals(ProductGroupVendor x, ProductGroupVendor y)
    {
      var productGroupX = x.ProductGroup.ProductGroupLanguages.FirstOrDefault(c => c.LanguageID == Client.User.LanguageID) ?? x.ProductGroup.ProductGroupLanguages.FirstOrDefault();
      var productGroupY = y.ProductGroup.ProductGroupLanguages.FirstOrDefault(c => c.LanguageID == Client.User.LanguageID) ?? y.ProductGroup.ProductGroupLanguages.FirstOrDefault();

      return productGroupX.Name == productGroupY.Name;
    }

    public int GetHashCode(ProductGroupVendor obj)
    {
      return obj.ProductGroupVendorID;
    }

    #endregion
  }
}
