using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Web.Shared;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Services.ServiceInterfaces;

using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.ui.Management.Controllers
{
  public class ProductAttributeValueTranslationController : BaseController
  {

    public ActionResult GetTranslations(int AttributeID, string Value)
    {
      return List(unit => (from l in unit.Service<Language>().GetAll()
                           join vl in unit.Service<ProductAttributeValueLabel>().GetAll().Where(c => c.AttributeID == AttributeID && c.Value == Value && (!c.ConnectorID.HasValue || c.ConnectorID == Client.User.ConnectorID)) on l.LanguageID equals vl.LanguageID into temp // new { vg.Value, vg.AttributeID, vg.ConnectorID } equals new { vl.Value, vl.AttributeID, vl.ConnectorID }
                           from tr in temp.DefaultIfEmpty()
                           // join p in unit.Service<ProductAttributeValueConnectorValueGroup>().GetAll().Where(c => c.AttributeValueGroupID == attributeValueValueGroupID) on l.LanguageID equals p. into temp
                           // from tr in temp.DefaultIfEmpty()
                           select new
                           {
                             l.LanguageID,
                             Language = l.Name,
                             AttributeID = AttributeID,
                             Translation = (l == null ? string.Empty : tr.Label) //  string.Empty // tr.Translation
                           }));
    }

    public ActionResult SetTranslation(int _LanguageID, string Translation, int _AttributeID, string Value)
    {

      using (var unit = GetUnitOfWork())
      {
        var service = ((IProductService)unit.Service<Product>());
        try
        {
          service.SetAttributeValueTranslations(_LanguageID, _AttributeID, Client.User.ConnectorID.Value, Translation, Value);

          //update for other languages aswell
          var childConnectors = unit.Service<Connector>().GetAll(x => x.ParentConnectorID == Client.User.ConnectorID.Value);
          foreach (var childCon in childConnectors)
          {
            service.SetAttributeValueTranslations(_LanguageID, _AttributeID, childCon.ConnectorID, Translation, Value);
          }

          unit.Save();
          return Success("Translations set");
        }
        catch (Exception e)
        {
          return Failure("Something went wrong", e);
        }
      }

    }
  }
}
