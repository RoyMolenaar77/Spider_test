using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Web.Shared.Models;

namespace Concentrator.ui.Management.Controllers
{
  public class PreferredConnectorVendorController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetPreferredConnectorVendor)]
    public ActionResult GetList(ContentFilter filter)
    {
      return List((unit) =>
                  (from c in unit.Service<PreferredConnectorVendor>().GetAll()
                   select new
                   {
                     c.VendorID,
                     c.ConnectorID,
                     c.isPreferred,
                     c.isContentVisible,
                     c.VendorIdentifier,
                     c.CentralDelivery
                   }));
    }

    [RequiresAuthentication(Functionalities.CreatePreferredConnectorVendor)]
    public ActionResult Create()
    {
      return Create<PreferredConnectorVendor>();
    }

    [RequiresAuthentication(Functionalities.UpdatePreferredConnectorVendor)]
    public ActionResult Update(int _ConnectorID, int _VendorID)
    {
      return Update<PreferredConnectorVendor>(c => c.ConnectorID == _ConnectorID && c.VendorID == _VendorID);
    }

    [RequiresAuthentication(Functionalities.DeletePreferredConnectorVendor)]
    public ActionResult Delete(int _ConnectorID, int _VendorID)
    {
      return Delete<PreferredConnectorVendor>(c => c.ConnectorID == _ConnectorID && c.VendorID == _VendorID);
    }
  }
}
