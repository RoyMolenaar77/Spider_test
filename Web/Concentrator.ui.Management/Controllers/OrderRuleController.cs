using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Web.Shared; using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class OrderRuleController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetOrderRule)]
    public ActionResult GetList()
    {
      return List(unit =>
                  from c in unit.Service<ConnectorRuleValue>().GetAll()
                  where !Client.User.ConnectorID.HasValue || (Client.User.ConnectorID.HasValue && c.ConnectorID == Client.User.ConnectorID)
                  select new
                  {
                    c.ConnectorID,
                    c.RuleID,
                    c.OrderRule.Name,
                    c.Value,
                    c.Description,
                    c.VendorID
                  });
    }

    [RequiresAuthentication(Functionalities.GetOrderRule)]
    public ActionResult GetStore()
    {
      return Json(new
      {
        OrderRules = SimpleList<OrderRule>(c => new
        {
          c.RuleID,
          c.Name
        })
      });
    }

    [RequiresAuthentication(Functionalities.GetOrderRule)]
    public ActionResult Search(string query)
    {
      return SimpleList(unit => (from i in unit.Service<OrderRule>().Search(query).ToList()
                                select new
                                {
                                  i.RuleID,
                                  i.Name
                                }).AsQueryable());
    }

    [RequiresAuthentication(Functionalities.DeleteOrderRule)]
    public ActionResult Delete(int _ConnectorID, int _RuleID, int _VendorID)
    {
      return Delete<ConnectorRuleValue>(c => c.RuleID == _RuleID && c.ConnectorID == _ConnectorID && c.VendorID == _VendorID);
    }

    [RequiresAuthentication(Functionalities.CreateOrderRule)]
    public ActionResult Create()
    {
      using (var unit = GetUnitOfWork())
      {
        var v = new ConnectorRuleValue();
        TryUpdateModel(v);

        if (((IOrderService)unit.Service<Order>()).IsOrderRuleValueValid(v))
        {
          unit.Service<ConnectorRuleValue>().Create(v);

          unit.Save();
        }
        return Failure("The sum of all values for a specific connector and a vendor cannot be more than 100. Current sum: " + v.Value);
      }
    }

    [RequiresAuthentication(Functionalities.UpdateOrderRule)]
    public ActionResult Update(int _ConnectorID, int _RuleID, int _VendorID)
    {
      var crv = new ConnectorRuleValue();
      TryUpdateModel<ConnectorRuleValue>(crv);
      using (var unit = GetUnitOfWork())
      {
        if (!((IOrderService)unit.Service<Order>()).IsOrderRuleValueValid(crv))
        {
          return Failure("The sum of all values for a specific connector and a vendor cannot be more than 100. Current sum: " + crv.Value);
        }

        return Update<ConnectorRuleValue>(c => c.RuleID == _RuleID && c.ConnectorID == _ConnectorID && c.VendorID == _VendorID);
      }
    }
  }
}
