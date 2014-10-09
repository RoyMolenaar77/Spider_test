using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Web;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Concentrator.Web.CustomerSpecific.Coolcat.Controllers
{
  public class DatcolController : BaseController
  {
    [RequiresAuthentication(Functionalities.Datcol)]
    public ActionResult GetAll()
    {
      var connectorID = Client.User.ConnectorID;

      return List((unit => from p in unit.Service<DatcolLink>().GetAll()
                           where p.Order.ConnectorID == connectorID || p.Order.Connector.ParentConnectorID == connectorID
                           select new DatcolDto
                           {
                             Id = p.Id,
                             WebsiteOrderNumber = p.Order.WebSiteOrderNumber,
                             DatcolNumber = p.DatcolNumber,
                             ShopNumber = p.ShopNumber,
                             DateCreated = p.DateCreated,
                             Amount = p.Amount,
                             PaymentMethod = p.Order.PaymentTermsCode,
                             MessageType = p.SourceMessage
                           }));
    }
  }
  public class DatcolDto
  {
    public int Id { get; set; }

    public string WebsiteOrderNumber { get; set; }

    public string DatcolNumber { get; set; }

    public string ShopNumber { get; set; }

    public DateTime DateCreated { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; }

    public string MessageType { get; set; }
  }
}
