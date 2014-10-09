using System;
using System.Linq;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Web.Shared;

namespace Concentrator.ui.Management.Controllers
{
  public class AttributeMatchController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetAttributeMatch)]
    public ActionResult GetTreeView(int? attributeStoreID, int? connectorID)
    {
      using (var unit = GetUnitOfWork())
      {

        int? conID = null;
        if (Client.User.ConnectorID.HasValue)
        {
          conID = Client.User.ConnectorID.Value;
        }
        else
        {
          conID = connectorID;
        }

        conID.ThrowIf(c => !c.HasValue, "A connector must be specified");

        var dist = (from d in unit.Service<AttributeMatchStore>().GetAll(c => c.ConnectorID == conID)
                    group d by d.AttributeStoreID into groupD
                    select groupD
                      ).ToList();

        //return parent nodes
        if (attributeStoreID.HasValue && attributeStoreID.Value == -1)
        {
          var result = (from p in dist
                        let productAttributeName = p.FirstOrDefault().ProductAttributeMetaData.ProductAttributeNames.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ?? p.FirstOrDefault().ProductAttributeMetaData.ProductAttributeNames.FirstOrDefault()
                        orderby p.Key descending
                        select new
                        {
                          AttributeStoreID = p.Key,
                          text = p.FirstOrDefault().StoreName ?? productAttributeName.Name,
                          leaf = false,
                          p.FirstOrDefault().ConnectorID
                        }).OrderBy(c => c.AttributeStoreID).ToList();



          return Json(result);
        }
        //return childs from a certain parentnode (no more levels than this)
        else
        {
          var result = (from p in unit.Service<AttributeMatchStore>().GetAll()
                        orderby p.AttributeStoreID descending
                        where p.ConnectorID == conID.Value && p.AttributeStoreID == attributeStoreID.Value
                        select new
                        {
                          p.AttributeStoreID,
                          p.AttributeID,
                          text = p.ProductAttributeMetaData.ProductAttributeNames.FirstOrDefault(c => c.AttributeID == p.AttributeID).Name,
                          leaf = true,
                          p.ConnectorID
                        }).OrderBy(c => c.AttributeStoreID).ToList();



          return Json(result);
        }


      }
    }

    [RequiresAuthentication(Functionalities.GetAttributeMatch)]
    public ActionResult GetList(bool? isMatched)
    {
      return new JsonResult();
    }
    
    [RequiresAuthentication(Functionalities.CreateAttributeMatch)]
    public ActionResult CreateAttributeMatch(int AttributeID)
    {
      using (var unit = GetUnitOfWork())
      {
        var existingMatch = unit.Service<ProductAttributeMatch>().Get(c => c.AttributeID == AttributeID);



        existingMatch.IsMatched = true;

        unit.Save();

        return Json(new
        {
          succes = true,
          message = "Successfully matched the Attributes"
        });
      }

    }

    [RequiresAuthentication(Functionalities.UpdateAttributeMatch)]
    public ActionResult Add(int[] attributeIDs, int? connectorID, int? attributeStoreID, int? oldAttributeStoreID)
    {
      try
      {
        if (!connectorID.HasValue) connectorID = Client.User.ConnectorID;

        connectorID.ThrowIf(c => !c.HasValue, "Connector id has to be specified");

        using (var unit = GetUnitOfWork())
        {
          var matchStoreService = unit.Service<AttributeMatchStore>();
          if (!attributeStoreID.HasValue)
          {
            var attrMatches = matchStoreService.GetAll();

            if (attrMatches.Count() == 0)
              attributeStoreID = 1;
            else
              attributeStoreID = attrMatches.Max(c => c.AttributeStoreID + 1);
          }

          AttributeMatchStore store = null;

          if (oldAttributeStoreID.HasValue)
          {
            matchStoreService.Delete(unit.Service<AttributeMatchStore>().GetAll(c => c.AttributeStoreID == oldAttributeStoreID.Value && attributeIDs.Contains(c.AttributeID)));
            unit.Save();
          }

          foreach (var id in attributeIDs)
          {
            store = new AttributeMatchStore
            {
              AttributeStoreID = attributeStoreID.Value,
              AttributeID = id,
              ConnectorID = connectorID.Value
            };
            matchStoreService.Create(store);
          }

          unit.Save();

          return Success("Successfully added to stores",
          data: new
          {
            AttributeStoreID = attributeStoreID.Value
          });
        }

      }
      catch (Exception e) { return Failure("Operation was not successful", e); }
    }

    [RequiresAuthentication(Functionalities.UpdateAttributeMatch)]
    public ActionResult UpdateAttributeMatch(int AttributeID)
    {
      //todo..
      using (var unit = GetUnitOfWork())
      {
        var existingMatch = unit.Service<ProductAttributeMatch>().Get(c => c.AttributeID == AttributeID);
                             


        existingMatch.IsMatched = true;

        unit.Save();

        return Json(new
        {
          succes = true,
          message = "Successfully updated the Attributes match"
        });
      }

    }

    [RequiresAuthentication(Functionalities.DeleteAttributeMatch)]
    public ActionResult RemoveAttributeMatch(int AttributeID)
    {
      using (var unit = GetUnitOfWork())
      {
        var existingMatch = unit.Service<ProductAttributeMatch>().Get(c => c.AttributeID == AttributeID);


        existingMatch.IsMatched = false;

        unit.Save();

        return Json(new
        {
          succes = true,
          message = "These Attributes have been stored as a non-match"
        });
      }

    }

    [RequiresAuthentication(Functionalities.DeleteAttributeMatch)]
    public ActionResult Delete(int? attributeID, int attributeStoreID, int? connectorID)
    {
      if (!connectorID.HasValue) connectorID = Client.User.ConnectorID;

      connectorID.ThrowIf(c => !c.HasValue, "Connector id has to be specified");

      return Delete<AttributeMatchStore>(c => (attributeID.HasValue ? c.AttributeID == attributeID : true) && c.ConnectorID == connectorID.Value && c.AttributeStoreID == attributeStoreID);
    }
  }
}
