using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Web.Shared;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Web;

namespace Concentrator.ui.Management.Controllers
{
  public class ConnectorAttributeValueGroupController : BaseController
  {
    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult Create()
    {
      return Create<ProductAttributeValueConnectorValueGroup>();
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult Delete(int _ConnectorID, int _AttributeID, int _AttributeValueGroupID)
    {
      return Delete<ProductAttributeValueConnectorValueGroup>(c => c.AttributeID == _AttributeID && c.AttributeValueGroupID == _AttributeValueGroupID && c.ConnectorID == _ConnectorID);
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetList()
    {
      return List(unit =>
                    from c in unit.Service<ProductAttributeValueConnectorValueGroup>().GetAll()
                    select new
                    {
                      c.ConnectorID,
                      Connector = c.Connector.Name,
                      c.AttributeID,
                      Attribute = c.ProductAttributeMetaData.ProductAttributeNames.FirstOrDefault(l => l.LanguageID == Client.User.LanguageID).Name,
                      c.AttributeValueGroupID,
                      AttributeValueGroup = c.ProductAttributeValueGroup.ProductAttributeValueGroupNames.FirstOrDefault(s => s.LanguageID == Client.User.LanguageID).Name
                    });
    }
  }
}
