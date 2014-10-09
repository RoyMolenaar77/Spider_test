using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Objects.Models.Management;
using Concentrator.Objects.Models.Users;
using Concentrator.Web.Shared;

namespace Concentrator.ui.Management.Controllers
{
  public class ConnectorVendorManagementContentController : BaseController
  {
    [RequiresAuthentication(Functionalities.ManageConnectorVendorContentFilter)]
    public ActionResult GetList(int? connectorID)
    {
      if (!connectorID.HasValue)
      {
        connectorID = Client.User.ConnectorID;
      }

      connectorID.ThrowIfNull("Connector id is needed for this operation");

      return List((unit) => from p in unit.Service<ConnectorVendorManagementContent>().GetAll(c => c.ConnectorID == connectorID.Value)
                            select new
                            {
                              p.ConnectorID,
                              Connector = p.Connector.Name,
                              p.VendorID,
                              Vendor = p.Vendor.Name,
                              p.IsDisplayed
                            });
    }

    [RequiresAuthentication(Functionalities.ManageConnectorVendorContentFilter)]
    public ActionResult Create()
    {
      return Create<ConnectorVendorManagementContent>(needsRefresh: true);
    }

    [RequiresAuthentication(Functionalities.ManageConnectorVendorContentFilter)]
    public ActionResult Delete(int _ConnectorID, int _VendorID)
    {
      return Delete<ConnectorVendorManagementContent>(c => c.ConnectorID == _ConnectorID && c.VendorID == _VendorID, needsRefresh: true);
    }

    [RequiresAuthentication(Functionalities.ManageConnectorVendorContentFilter)]
    public ActionResult Update(int _ConnectorID, int _VendorID)
    {
      return Update<ConnectorVendorManagementContent>(c => c.ConnectorID == _ConnectorID && c.VendorID == _VendorID, needsRefresh: true);
    }
  }
}
