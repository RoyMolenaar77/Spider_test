using Concentrator.Objects.Environments;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Web;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Concentrator.ui.Management.Controllers
{
  public class RefundController : BaseController
  {
    /// <summary>
    /// Retrieves the invalid refunds from the refund queue
    /// </summary>
    /// <returns></returns>
    [RequiresAuthentication(Functionalities.ViewOrders)]
    public ActionResult WidgetData()
    {
      try
      {
        using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
        {
          var invalidRefundCount = db.ExecuteScalar<int>("SELECT Count(*) from refundQueue where Valid = 0");
          var validRefundCount = db.ExecuteScalar<int>("SELECT Count(*) from refundQueue where Valid = 1");

          ViewBag.CountInvalidRefunds = invalidRefundCount;
          ViewBag.CountValidRefunds = validRefundCount;

          return View("RefundQueue");
        }
      }
      catch (SqlException sq)
      {
        return Failure("A database error ocurred while retrieving the refunds", sq);
      }
      catch (Exception sq)
      {
        return Failure("An error ocurred while retrieving the refunds", sq);
      }

    }

    /// <summary>
    /// Retrieves the paged and filterred list of refunds in the queue 
    /// </summary>
    /// <returns></returns>
    [RequiresAuthentication(Functionalities.ViewOrders)]
    public ActionResult List(bool? valid)
    {
      var currentOrganizationID = Client.User.OrganizationID;

      return List(unit => (from queueRecord in unit.Service<RefundQueue>().GetAll()
                           where queueRecord.Order.Connector.OrganizationID == currentOrganizationID
                           && (valid.HasValue ? queueRecord.Valid == valid : true)
                           select new PagedRefundViewModel()
                             {
                               OrderID = queueRecord.OrderID,
                               WebsiteOrderNumber = queueRecord.Order.WebSiteOrderNumber,
                               ConnectorID = queueRecord.Order.ConnectorID,
                               OrderResponseID = queueRecord.OrderResponseID,
                               ResponseType = queueRecord.OrderResponse.ResponseType,
                               CreationTime = queueRecord.CreationTime,
                               Amount = queueRecord.Amount
                             }
                           ));
    }
  }

  public class PagedRefundViewModel
  {
    public int OrderID { get; set; }
    public string WebsiteOrderNumber { get; set; }
    public int ConnectorID { get; set; }
    public int OrderResponseID { get; set; }
    public string ResponseType { get; set; }
    public DateTime CreationTime { get; set; }
    public decimal Amount { get; set; }
  }
}
