using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Orders;
using Concentrator.Web.Shared; using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class CustomerController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetCustomer)]
    public ActionResult GetList()
    {
      return List(unit =>
                  (from c in unit.Service<Customer>().GetAll()
									 where (!Client.User.ConnectorID.HasValue || c.SoldOrder.FirstOrDefault().ConnectorID == Client.User.ConnectorID)
                  select new
                  {
                    c.CustomerID,
                    c.CustomerTelephone,
                    c.CustomerEmail,
                    c.City,
                    c.Country,
                    c.PostCode,
                    c.CustomerAddressLine1,
                    c.CustomerAddressLine2,
                    c.CustomerAddressLine3,
                    c.EANIdentifier,
                    c.CustomerName,
                    c.HouseNumber
                  }).AsQueryable());

    }

    [RequiresAuthentication(Functionalities.GetCustomer)]
    public ActionResult GetByOrder(int orderID)
    {
      var order = GetObject<Order>(c => c.OrderID == orderID);
      var soldCust = order.SoldToCustomer;
      var shippedCust = order.ShippedToCustomer;

      if (soldCust != null)
      {
        return Json(new
        {
          customer = new
          {
            City = soldCust.City,
            Country = soldCust.Country,
            CustomerAddressLine1 = soldCust.CustomerAddressLine1,
            CustomerAddressLine2 = soldCust.CustomerAddressLine2,
            CustomerAddressLine3 = soldCust.CustomerAddressLine3,
            CustomerEmail = soldCust.CustomerEmail,
            CustomerName = soldCust.CustomerName,
            CustomerTelephone = soldCust.CustomerTelephone,
            EANIdentifier = soldCust.EANIdentifier,
            PostCode = soldCust.PostCode,
            ShipCity = shippedCust.City,
            ShipCountry = shippedCust.Country,
            ShipCustomerAddressLine1 = shippedCust.CustomerAddressLine1,
            ShipCustomerAddressLine2 = shippedCust.CustomerAddressLine2,
            ShipCustomerAddressLine3 = shippedCust.CustomerAddressLine3,
            ShipCustomerEmail = shippedCust.CustomerEmail,
            ShipCustomerName = shippedCust.CustomerName,
            ShipCustomerTelephone = shippedCust.CustomerTelephone,
            ShipEANIdentifier = shippedCust.EANIdentifier,
            ShipPostCode = shippedCust.PostCode,
          }
        });
      }
      
      if (shippedCust != null)
      {
        return Json(new
        {
          customer = new
          {
            ShipCity = shippedCust.City,
            ShipCountry = shippedCust.Country,
            ShipCustomerAddressLine1 = shippedCust.CustomerAddressLine1,
            ShipCustomerAddressLine2 = shippedCust.CustomerAddressLine2,
            ShipCustomerAddressLine3 = shippedCust.CustomerAddressLine3,
            ShipCustomerEmail = shippedCust.CustomerEmail,
            ShipCustomerName = shippedCust.CustomerName,
            ShipCustomerTelephone = shippedCust.CustomerTelephone,
            ShipEANIdentifier = shippedCust.EANIdentifier,
            ShipPostCode = shippedCust.PostCode
          }
        });
      }

      return Json(new
      {
        customer = new
        {
        }
      });
    }

    [RequiresAuthentication(Functionalities.CreateCustomer)]
    public ActionResult Create()
    {
      return Create<Customer>();
    }

    [RequiresAuthentication(Functionalities.DeleteCustomer)]
    public ActionResult Delete(int id)
    {
      return Delete<Customer>(c => c.CustomerID == id);
    }

    [RequiresAuthentication(Functionalities.UpdateCustomer)]
    public ActionResult Update(int id)
    {
      return Update<Customer>(c => c.CustomerID == id);
    }
  }
}
