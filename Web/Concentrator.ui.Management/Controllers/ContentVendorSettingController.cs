using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class ContentVendorSettingController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetContentVendorSetting)]
    public ActionResult GetList()
    {
      return List(unit => (from c in unit.Service<ContentVendorSetting>().GetAll().ToList()
                           let v = c.ProductID
                           let productName = c.Product != null ?
                  c.Product.ProductDescriptions.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                  c.Product.ProductDescriptions.FirstOrDefault() : null
                           let productgroupName = c.ProductGroup != null ?
                              c.ProductGroup.ProductGroupLanguages.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                              c.ProductGroup.ProductGroupLanguages.FirstOrDefault() : null
                           select new
                           {
                             c.ContentVendorSettingID,
                             c.ConnectorID,
                             c.VendorID,
                             ProductGroupID = c.ProductGroupID,
                             ProductGroupName = productgroupName != null ? productgroupName.Name : string.Empty,
                             c.ProductID,
                             ProductDescription = productName != null ? (productName.ProductName ?? productName.ShortContentDescription) :
   c.Product != null ? c.Product.VendorItemNumber : string.Empty,
                             c.BrandID,
                             Brandname = c.Brand != null ? c.Brand.Name : string.Empty,
                             c.CreatedBy,
                             CreationTime = c.CreationTime.ToLocalTime(),
                             c.LastModifiedBy,
                             LastModificationTime = c.LastModificationTime.ToNullOrLocal(),
                             c.ContentVendorIndex
                           }).AsQueryable());
    }

    [RequiresAuthentication(Functionalities.CreateContentVendorSetting)]
    public ActionResult Create(int ConnectorID, int ContentVendorIndex, int? ProductGroupID, int VendorID)
    {
      return Create<ContentVendorSetting>(onCreatingAction: (unit, model) =>
      {
        model.ConnectorID = ConnectorID;
        model.ProductGroupID = ProductGroupID;
        model.VendorID = VendorID;
        model.ContentVendorIndex = ContentVendorIndex;
      });
    }

    [RequiresAuthentication(Functionalities.UpdateContentVendorSetting)]
    public ActionResult Update(int ID)
    {
      return Update<ContentVendorSetting>(c => c.ContentVendorSettingID == ID,
      updatePredicate: (unit, setting) => unit.Service<ContentVendorSetting>().Get(c => c.ConnectorID == setting.ConnectorID && c.ProductGroupID == setting.ProductGroupID && c.ContentVendorIndex == setting.ContentVendorIndex) != null,
      updatePredicateErrorMessage: "Duplicate content vendor setting");
    }

    [RequiresAuthentication(Functionalities.DeleteContentVendorSetting)]
    public ActionResult Delete(int ID)
    {
      return Delete<ContentVendorSetting>(c => c.ContentVendorSettingID == ID);
    }

    [RequiresAuthentication(Functionalities.GetContentVendorSetting)]
    public ActionResult GetByProductGroupMapping(int productgroupMappingID, int? connectorID)
    {

      if (!connectorID.HasValue)
      {
        connectorID = Client.User.ConnectorID;
      }

      connectorID.ThrowIfNull("Needs a Connector ID");

      return List(unit =>
      {
        var pgID = unit.Service<ProductGroupMapping>().Get(c => c.ProductGroupMappingID == productgroupMappingID).ProductGroupID;

        return (from p in unit.Service<ContentVendorSetting>().GetAll().ToList()
                where !p.ProductGroupID.HasValue || p.ProductGroupID == pgID
                && p.ConnectorID == connectorID
                let productGroupName = p.ProductGroup != null ?
                p.ProductGroup.ProductGroupLanguages.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                p.ProductGroup.ProductGroupLanguages.FirstOrDefault() : null
                select new
                {
                  p.ContentVendorSettingID,
                  p.ProductGroupID,
                  ProductGroup = productGroupName != null ? productGroupName.Name : string.Empty,
                  p.VendorID,
                  Vendor = p.Vendor != null ? p.Vendor.Name : String.Empty,
                  p.BrandID,
                  Brand = (p.Brand != null ? p.Brand.Name : string.Empty),
                  p.ContentVendorIndex
                }).AsQueryable();
      }
      );
    }
  }
}
