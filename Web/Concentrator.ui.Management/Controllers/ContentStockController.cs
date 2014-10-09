using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Web;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Concentrator.ui.Management.Controllers
{
  public class ContentStockController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetContentStock)]
    public ActionResult GetList()
    {
      return List((unit) => from cs in unit.Service<ContentStock>().GetAll()
                            select new
                            {
                              cs.VendorID,
                              cs.ConnectorID,
                              cs.VendorStockTypeID,
                              StockType = cs.VendorStockType.StockType,
                              VendorName = cs.Vendor.Name
                            });
    }

    [RequiresAuthentication(Functionalities.CreateContentStock)]
    public ActionResult Create()
    {
      return Create<ContentStock>();
    }

    [RequiresAuthentication(Functionalities.DeleteContentStock)]
    public ActionResult Delete(int _ConnectorID, int _VendorID, int _VendorStockTypeID)
    {
      return Delete<ContentStock>(c => c.ConnectorID == _ConnectorID && c.VendorID == _VendorID && c.VendorStockTypeID == _VendorStockTypeID);
    }

    [RequiresAuthentication(Functionalities.UpdateContentStock)]
    public ActionResult Update(int _ConnectorID, int _VendorID, int _VendorStockTypeID)
    {
      return Update<ContentStock>(c => c.ConnectorID == _ConnectorID && c.VendorID == _VendorID && c.VendorStockTypeID == _VendorStockTypeID);
    }

  }
}
