using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Orders;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class OrderOutboundController : BaseController
  {
    private static readonly string[] _editPropertyWhiteList = new string[] { "OutboundID", "ConnectorID", "Processed", 
                                                                             "CreationTime", "Type", "OutboundUrl", 
                                                                             "ResponseRemark", "ResponseTime", "ProcessedCount", 
                                                                             "ErrorMessage", "ProcessDate", "OrderID" 
                                                                            };

    [RequiresAuthentication(Functionalities.GetOrderOutbound)]
    public ActionResult GetList(int? OrderID)
    {
      return List(unit =>
                  from o in unit.Service<Outbound>().GetAll()
                  where (OrderID.HasValue ? o.OrderID == OrderID : true)
                  && (o.ConnectorID == Client.User.ConnectorID || o.Order.Connector.ParentConnectorID == Client.User.ConnectorID)
                  select new OutboundDto
                  {
                    OutboundID = o.OutboundID,
                    OutboundMessage = o.OutboundMessage,
                    ConnectorID = o.ConnectorID,
                    Processed = o.Processed,
                    CreationTime = o.CreationTime,
                    Type = o.Type,
                    OutboundUrl = o.OutboundUrl,
                    ResponseRemark = o.ResponseRemark,
                    ResponseTime = o.ResponseTime,
                    ProcessedCount = o.ProcessedCount,
                    ErrorMessage = o.ErrorMessage,
                    ProcessDate = o.ProcessDate,
                    OrderID = o.OrderID
                  });
    }

    [RequiresAuthentication(Functionalities.UpdateOrderOutbound)]
    public ActionResult Update(int id)
    {
      return Update<Outbound>(c => c.OutboundID == id, properties: _editPropertyWhiteList);
    }
  }

  public class OutboundDto
  {
    public int OutboundID { get; set; }
    public string OutboundMessage { get; set; }
    public int ConnectorID { get; set; }
    public bool Processed { get; set; }
    public DateTime CreationTime { get; set; }
    public string Type { get; set; }
    public string OutboundUrl { get; set; }
    public string ResponseRemark { get; set; }
    public int? ResponseTime { get; set; }
    public int? ProcessedCount { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime? ProcessDate { get; set; }
    public int OrderID { get; set; }
  }
}
