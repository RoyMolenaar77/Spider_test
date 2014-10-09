using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Web.Shared; using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class CrossLedgerController : BaseController
  {
    [RequiresAuthentication(Functionalities.CrossLedger)]
    public ActionResult GetList()
    {
      return List(unit => from cl in unit.Service<CrossLedgerclass>().GetAll()
                          where !Client.User.ConnectorID.HasValue || (Client.User.ConnectorID.HasValue && cl.ConnectorID == Client.User.ConnectorID)
                          select new
                          {
                            cl.ConnectorID,
                            cl.CrossLedgerclassCode,
                            cl.LedgerclassCode,
                            cl.Description
                          });
    }
     
    [RequiresAuthentication(Functionalities.CrossLedger)]
    public ActionResult Create()
    {
      return Create<CrossLedgerclass>();
    }

    [RequiresAuthentication(Functionalities.CrossLedger)]
    public ActionResult Update(int _ConnectorID, string _LedgerclassCode)
    {
      return Update<CrossLedgerclass>(c => c.ConnectorID == _ConnectorID && c.LedgerclassCode == _LedgerclassCode);
    }

    [RequiresAuthentication(Functionalities.CrossLedger)]
    public ActionResult Delete(int _ConnectorID, string _LedgerclassCode)
    {
      return Delete<CrossLedgerclass>(c => c.ConnectorID == _ConnectorID && c.LedgerclassCode == _LedgerclassCode);
    }
  }
}
