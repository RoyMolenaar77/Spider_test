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
  public class ConnectorLanguageController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetConnectorLanguage)]
    public ActionResult GetList()
    {
      return List(unit => from cl in unit.Service<ConnectorLanguage>().GetAll()
                          select new
                          {
                            cl.ConnectorLanguageID,
                            cl.ConnectorID,
                            cl.LanguageID,
                            cl.Country
                          });
    }

    [RequiresAuthentication(Functionalities.CreateConnectorLanguage)]
    public ActionResult Create()
    {
      return Create<ConnectorLanguage>();
    }

    [RequiresAuthentication(Functionalities.DeleteConnectorLanguage)]
    public ActionResult Delete(int id)
    {
      return Delete<ConnectorLanguage>(c => c.ConnectorLanguageID == id);
    }

    [RequiresAuthentication(Functionalities.UpdateConnectorLanguage)]
    public ActionResult Update(int id)
    {
      return Update<ConnectorLanguage>(c => c.ConnectorLanguageID == id);
    }
  }
}
