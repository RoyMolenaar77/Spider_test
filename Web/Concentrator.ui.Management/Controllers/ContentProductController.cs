using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Contents;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class ContentProductController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetContentProduct)]
    public ActionResult GetList()
    {
      return List(unit => (from cp in unit.Service<ContentProduct>().GetAll()
                           let productName = cp.Product != null ?
                   cp.Product.ProductDescriptions.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                   cp.Product.ProductDescriptions.FirstOrDefault() : null
                           let productGroupName = cp.ProductGroup != null ?
                    cp.ProductGroup.ProductGroupLanguages.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                    cp.ProductGroup.ProductGroupLanguages.FirstOrDefault() : null
                           select new
                            {
                              cp.ProductContentID,
                              BrandName = cp.Brand != null ? cp.Brand.Name : string.Empty,
                              cp.BrandID,
                              cp.ProductID,
                              cp.ConnectorID,
                              cp.Connector.Name,
                              cp.ProductGroupID,
                              cp.ProductContentIndex,
                              cp.IsAssortment,
                              ProductGroupName = productGroupName != null ? productGroupName.Name : string.Empty,
                              ProductDescription = productName != null ? (productName.ProductName ?? productName.ShortContentDescription) :
   cp.Product != null ? cp.Product.VendorItemNumber : string.Empty,
                              cp.VendorID
                            }).AsQueryable());

    }

    [RequiresAuthentication(Functionalities.GetContentProduct)]
    public ActionResult ViewProducts(int productContentID)
    {
      return List((unit) =>
                  (from c in unit.Service<Content>().GetAll(c => c.ProductContentID == productContentID).ToList()
                   select new
                   {
                     c.ProductID,
                     c.ShortDescription,
                     c.LongDescription,
                     c.LineType,
                     CreationTime = c.CreationTime.ToLocalTime(),
                     LastModificationTime = c.LastModificationTime.ToNullOrLocal(),
                     CustomItemNumber = c.Product.VendorAssortments.FirstOrDefault().CustomItemNumber
                   }).AsQueryable());
    }

    [RequiresAuthentication(Functionalities.CreateContentProduct)]
    public ActionResult Create()
    {
      return Create<ContentProduct>();
    }

    [RequiresAuthentication(Functionalities.UpdateContentProduct)]
    public ActionResult Update(int _ProductContentID)
    {
      return Update<ContentProduct>(c => c.ProductContentID == _ProductContentID);
    }

    [RequiresAuthentication(Functionalities.DeleteContentProduct)]
    public ActionResult Delete(int _ProductContentID)
    {
      return Delete<ContentProduct>(c => c.ProductContentID == _ProductContentID);
    }
  }
}
