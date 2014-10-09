using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;


namespace Concentrator.ui.Management.Controllers
{
  public class ConnectorPublicationController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetConnectorPublication)]
    public ActionResult GetList()
    {
      return List(unit =>
        from c in unit.Service<ConnectorPublication>().GetAll()
        let productName = c.Product != null
          ? c.Product.ProductDescriptions.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ?? c.Product.ProductDescriptions.FirstOrDefault()
          : null
        let productGroupName = c.ProductGroup != null
          ? c.ProductGroup.ProductGroupLanguages.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ?? c.ProductGroup.ProductGroupLanguages.FirstOrDefault()
          : null
        select new
        {
          c.ConnectorPublicationID,
          c.ConnectorID,
          c.VendorID,
          c.ProductGroupID,
          c.BrandID,
          BrandName = c.Brand != null ? c.Brand.Name : string.Empty,
          c.AttributeID,
          AttributeName = c.ProductAttributeMetaData != null ? c.ProductAttributeMetaData.ProductAttributeNames.FirstOrDefault().Name : string.Empty,
          c.AttributeValue,
          c.ProductID,
          ProductGroupName = productGroupName != null ? productGroupName.Name : string.Empty,
          c.Vendor.Name,
          ProductDescription = c.Product != null
            ? "(" + c.Product.VendorItemNumber + ")   " 
              + (productName != null ? (productName.ProductName ?? productName.ShortContentDescription) : (c.Product != null ? c.Product.VendorItemNumber : string.Empty))
            : String.Empty,
          c.Publish,
          c.PublishOnlyStock,
          c.CreatedBy,
          CreationTime = c.CreationTime,
          c.LastModifiedBy,
          LastModificationTime = c.LastModificationTime,
          c.ProductContentIndex,
          ConcentratorStatusID = c.StatusID,
          ConcentratorStatus = c.AssortmentStatus.Status
        });
    }

    [RequiresAuthentication(Functionalities.GetConnectorPublication)]
    public ActionResult GetListOfConnectorPublictionRulesByConnector(int ConnectorID)
    {
      return List((unit) =>
                  (from c in unit.Service<ConnectorPublicationRule>()
                   .GetAll(x => x.ConnectorID == ConnectorID)
                   let productName = c.Product != null ?
                   c.Product.ProductDescriptions.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                   c.Product.ProductDescriptions.FirstOrDefault() : null
                   let productGroupName = c.ProductGroup != null ?
                    c.ProductGroup.ProductGroupLanguages.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                    c.ProductGroup.ProductGroupLanguages.FirstOrDefault() : null
                   let masterGroupMappingName = c.MasterGroupMappingID != null ?
                    c.MasterGroupMapping.MasterGroupMappingLanguages.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                    c.MasterGroupMapping.MasterGroupMappingLanguages.FirstOrDefault() : null
                   select new
                   {
                     c.ConnectorPublicationRuleID,
                     c.ConnectorID,
                     Connector = c.Connector.Name,
                     c.VendorID,
                     VendorName = c.Vendor.Name,
                     c.ProductGroupID,
                     c.BrandID,
                     BrandName = c.Brand != null ? c.Brand.Name : string.Empty,
                     c.ProductID,
                     ProductGroupName = productGroupName != null ? productGroupName.Name : string.Empty,
                     ProductDescription = productName != null ? (productName.ProductName ?? productName.ShortContentDescription) :
  c.Product != null ? c.Product.VendorItemNumber : string.Empty,
                     PublicationType = c.PublicationType == 0 ? false : true,
                     PublishOnlyStock = c.PublishOnlyStock ?? false,
                     c.CreatedBy,
                     CreationTime = c.CreationTime.ToLocalTime(),
                     c.LastModifiedBy,
                     LastModificationTime = c.LastModificationTime.ToNullOrLocal(),
                     c.PublicationIndex,
                     ConcentratorStatusID = c.StatusID,
                     ConcentratorStatus = c.AssortmentStatus != null ? c.AssortmentStatus.Status : string.Empty,
                     FromDate = c.FromDate.ToNullOrLocal(),
                     ToDate = c.ToDate.ToNullOrLocal(),
                     c.FromPrice,
                     c.ToPrice,
                     c.IsActive,
                     c.EnabledByDefault,
                     c.MasterGroupMappingID,
                     MasterGroupMappingName = masterGroupMappingName != null ? masterGroupMappingName.Name : null,
                     c.OnlyApprovedProducts,
                     c.ConnectorRelationID,
                     ConnectorRelationCustomerID = c.ConnectorRelation.CustomerID
                   })
                   .AsQueryable()
                   .OrderBy(x => x.PublicationIndex));

    }

    [RequiresAuthentication(Functionalities.CreateConnectorPublication)]
    public ActionResult Create()
    {
      return Create<ConnectorPublication>();
    }

    [RequiresAuthentication(Functionalities.DeleteConnectorPublication)]
    public ActionResult Delete(int id)
    {
      return Delete<ConnectorPublication>(c => c.ConnectorPublicationID == id);
    }

    [RequiresAuthentication(Functionalities.UpdateConnectorPublication)]
    public ActionResult Update(int id)
    {
      return Update<ConnectorPublication>(c => c.ConnectorPublicationID == id);
    }
  }
}
