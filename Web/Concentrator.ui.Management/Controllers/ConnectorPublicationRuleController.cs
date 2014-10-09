using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Web;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Concentrator.ui.Management.Controllers
{
  public class ConnectorPublicationRuleController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetConnectorPublication)]
    public ActionResult GetList()
    {
      return List((unit) =>
                  (from c in unit.Service<ConnectorPublicationRule>().GetAll().ToList()
                   let productName = c.Product != null ?
                   c.Product.ProductDescriptions.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                   c.Product.ProductDescriptions.FirstOrDefault() : null
                   let productGroupName = c.ProductGroup != null ?
                    c.ProductGroup.ProductGroupLanguages.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                    c.ProductGroup.ProductGroupLanguages.FirstOrDefault() : null
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
                     c.EnabledByDefault,
                     c.FromPrice,
                     c.ToPrice,
                     c.AttributeID,
                     c.AttributeValue,
                     AttributeCode = c.AttributeID.HasValue ? c.ProductAttributeMetaData.AttributeCode : "",
                     c.IsActive,
                     c.ConnectorRelationID,
                     ConnectorRelationCustomerID = c.ConnectorRelation != null ? c.ConnectorRelation.CustomerID : null
                   }).AsQueryable());
    }

    [RequiresAuthentication(Functionalities.GetConnectorPublication)]
    public ActionResult Get(int ConnectorPublicationRuleID)
    {
      var connectorPublicationRule = GetObject<ConnectorPublicationRule>(c => c.ConnectorPublicationRuleID == ConnectorPublicationRuleID);
      var productName = connectorPublicationRule.Product != null ?
                        connectorPublicationRule.Product.ProductDescriptions.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                        connectorPublicationRule.Product.ProductDescriptions.FirstOrDefault() : null;
      var masterGroupMappingName = connectorPublicationRule.MasterGroupMappingID != null ?
                        connectorPublicationRule.MasterGroupMapping.MasterGroupMappingLanguages.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                        connectorPublicationRule.MasterGroupMapping.MasterGroupMappingLanguages.FirstOrDefault() : null;

      var attributeName = connectorPublicationRule.ProductAttributeMetaData != null ?
                        connectorPublicationRule.ProductAttributeMetaData.ProductAttributeNames.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                        connectorPublicationRule.ProductAttributeMetaData.ProductAttributeNames.FirstOrDefault() : null;

      return Json(new
      {
        success = true,
        data = new
        {
          connectorPublicationRule.ConnectorPublicationRuleID,
          connectorPublicationRule.ConnectorID,
          Connector = connectorPublicationRule.Connector.Name,
          connectorPublicationRule.VendorID,
          VendorName = connectorPublicationRule.Vendor.Name,
          connectorPublicationRule.ProductGroupID,
          connectorPublicationRule.BrandID,
          BrandName = connectorPublicationRule.Brand != null ? connectorPublicationRule.Brand.Name : string.Empty,
          connectorPublicationRule.ProductID,
          ProductDescription = productName != null ? (productName.ProductName ?? productName.ShortContentDescription) :
          connectorPublicationRule.Product != null ? connectorPublicationRule.Product.VendorItemNumber : string.Empty,
          PublicationType = connectorPublicationRule.PublicationType == 0 ? false : true,
          PublishOnlyStock = connectorPublicationRule.PublishOnlyStock ?? false,
          connectorPublicationRule.CreatedBy,
          CreationTime = connectorPublicationRule.CreationTime.ToLocalTime(),
          connectorPublicationRule.LastModifiedBy,
          LastModificationTime = connectorPublicationRule.LastModificationTime.ToNullOrLocal(),
          connectorPublicationRule.PublicationIndex,
          ConcentratorStatusID = connectorPublicationRule.StatusID,
          ConcentratorStatus = connectorPublicationRule.AssortmentStatus != null ? connectorPublicationRule.AssortmentStatus.Status : string.Empty,
          FromDate = connectorPublicationRule.FromDate.ToNullOrLocal(),
          ToDate = connectorPublicationRule.ToDate.ToNullOrLocal(),
          connectorPublicationRule.EnabledByDefault,
          connectorPublicationRule.FromPrice,
          connectorPublicationRule.ToPrice,
          connectorPublicationRule.IsActive,
          connectorPublicationRule.MasterGroupMappingID,
          MasterGroupMappingName = masterGroupMappingName != null ? masterGroupMappingName.Name : null,
          connectorPublicationRule.OnlyApprovedProducts,
          connectorPublicationRule.ConnectorRelationID,
          ConnectorRelationCustomerID = connectorPublicationRule.ConnectorRelation != null ? connectorPublicationRule.ConnectorRelation.CustomerID : null,
          AttributeName = attributeName != null ? attributeName.Name : null,
          connectorPublicationRule.AttributeID,
          connectorPublicationRule.AttributeValue
        }
      });
    }

    [RequiresAuthentication(Functionalities.GetConnectorPublication)]
    public ActionResult GetListOfConnectorPublictionRulesByConnector(int ConnectorID)
    {
      return List((unit) =>
                  (from c in unit.Service<ConnectorPublicationRule>()
                   .GetAll(x => x.ConnectorID == ConnectorID || x.Connector.ParentConnectorID == ConnectorID).ToList()
                   let productName = c.Product != null ?
                   c.Product.ProductDescriptions.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                   c.Product.ProductDescriptions.FirstOrDefault() : null
                   let productGroupName = c.ProductGroup != null ?
                    c.ProductGroup.ProductGroupLanguages.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                    c.ProductGroup.ProductGroupLanguages.FirstOrDefault() : null
                   let masterGroupMappingName = c.MasterGroupMappingID != null ?
                    c.MasterGroupMapping.MasterGroupMappingLanguages.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                    c.MasterGroupMapping.MasterGroupMappingLanguages.FirstOrDefault() : null
                   let attributeName = c.ProductAttributeMetaData != null ?
                     c.ProductAttributeMetaData.ProductAttributeNames.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                     c.ProductAttributeMetaData.ProductAttributeNames.FirstOrDefault() : null
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
                     ConnectorRelationCustomerID = c.ConnectorRelation != null ? c.ConnectorRelation.CustomerID : string.Empty,
                     c.AttributeID,
                     AttributeName = attributeName != null ? attributeName.Name : null,
                     c.AttributeValue
                   })
                   .AsQueryable()
                   .OrderBy(x => x.PublicationIndex));

    }

    [RequiresAuthentication(Functionalities.CreateConnectorPublication)]
    public ActionResult Create(string PublicationType)
    {
      return Create<ConnectorPublicationRule>(onCreatedAction: (unit, cpr) =>
      {
        if (!string.IsNullOrEmpty(PublicationType))
        {
          if (PublicationType.Equals("on"))
            cpr.PublicationType = 1;
        }

      });
    }

    [RequiresAuthentication(Functionalities.CreateConnectorPublication)]
    public ActionResult CreateConnectorPublicationRule(bool Publication)
    {
      var connectorPublicationRule = new ConnectorPublicationRule();
      TryUpdateModel<ConnectorPublicationRule>(connectorPublicationRule);

      if (connectorPublicationRule.ConnectorID == 0)
      {
        return Failure("Faild to save Connector Publication Rule. Connector does not exist");
      }

      if (connectorPublicationRule.VendorID == 0)
      {
        return Failure("Faild to save Connector Publication Rule. Vendor does not exist");
      }

      connectorPublicationRule.PublicationType = Publication ? (int)ConnectorPublicationRuleType.Include : (int)ConnectorPublicationRuleType.Exclude;

      using (var unit = GetUnitOfWork())
      {
        try
        {
          unit.Service<ConnectorPublicationRule>().Create(connectorPublicationRule);
          unit.Save();

          return Success("Successfully created Connector Publication Rule", isMultipartRequest: true, needsRefresh: false);
        }
        catch (Exception e)
        {
          return Failure("Failed to create Connector Publication Rule. ", e, isMultipartRequest: true);
        }
      }
    }

    [RequiresAuthentication(Functionalities.DeleteConnectorPublication)]
    public ActionResult Delete(int id)
    {
      return Delete<ConnectorPublicationRule>(c => c.ConnectorPublicationRuleID == id);
    }

    [RequiresAuthentication(Functionalities.UpdateConnectorPublication)]
    public ActionResult Update(int id, bool? PublicationType)
    {
      return Update<ConnectorPublicationRule>(c => c.ConnectorPublicationRuleID == id, action: (unit, cpr) =>
      {
        if (PublicationType.HasValue)
        {
          cpr.PublicationType = Convert.ToInt32(PublicationType);
        }
      });
    }

    [RequiresAuthentication(Functionalities.UpdateConnectorPublication)]
    public ActionResult UpdatePublicationRuleIndex(string ListOfConnectorPublicationRuleIDsJson)
    {
      var ListOfConnectorPublicationRuleIDsModel = new
      {
        listOfPublicationIndex = new[] { new { ConnectorPublicationRuleID = 0, ConnectorPublicationRuleIndex = 0 } }
      };
      var ListOfIDs = JsonConvert.DeserializeAnonymousType(ListOfConnectorPublicationRuleIDsJson, ListOfConnectorPublicationRuleIDsModel);
      using (var unit = GetUnitOfWork())
      {
        bool savePublicationIndex = true;
        ListOfIDs.listOfPublicationIndex.ForEach(x =>
        {
          ConnectorPublicationRule connectorPublicationRule = unit
            .Service<ConnectorPublicationRule>()
            .Get(c => c.ConnectorPublicationRuleID == x.ConnectorPublicationRuleID);

          if (connectorPublicationRule != null)
          {
            connectorPublicationRule.PublicationIndex = x.ConnectorPublicationRuleIndex;
          }
          else
          {
            savePublicationIndex = false;
          }
        });

        if (savePublicationIndex)
        {
          unit.Save();
          return Success("Successfully updated Connector Publication Rule Index");
        }
        else
        {
          return Failure("Faild to updated Connector Publication Rule Index");
        }
      }
    }
  }
}
