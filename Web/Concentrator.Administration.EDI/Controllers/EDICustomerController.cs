using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using Concentrator;
using Concentrator.Objects.Product;
using Concentrator.Objects.Security;
using Concentrator.Objects.Web;
using Concentrator.Objects.Enumaration;
using Concentrator.Objects.Enumerations;
using Concentrator.Objects;
using System.Data.Linq;


namespace Concentrator.Administration.EDI.Controllers
{
  public class EDICustomerController : EDIConcentratorController
  {
  
    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetByEdiOrder(int ediOrderID)
    {
      DataLoadOptions options = new DataLoadOptions();
      options.LoadWith<Concentrator.Objects.EDI.Order.EDIOrder>(o => o.ShipToCustomer);
      options.LoadWith<Concentrator.Objects.EDI.Order.EDIOrder>(o => o.SoldToCustomer);
      var soldCust = GetObject<Objects.EDI.Order.EDIOrder>(c => c.EdiOrderID == ediOrderID, options).SoldToCustomer;
      var shipCust = GetObject<Objects.EDI.Order.EDIOrder>(c => c.EdiOrderID == ediOrderID, options).ShipToCustomer;

      if (soldCust != null)
      {
        return Json(new
        {
          customer = new
          {
            City = soldCust != null ? soldCust.City : shipCust.City,
            Country = soldCust != null ? soldCust.Country : shipCust.Country,
            CustomerAddressLine1 = soldCust != null ? soldCust.CustomerAddressLine1 : shipCust.CustomerAddressLine1,
            CustomerAddressLine2 = soldCust != null ? soldCust.CustomerAddressLine2 : shipCust.CustomerAddressLine2,
            CustomerAddressLine3 = soldCust != null ? soldCust.CustomerAddressLine3 : shipCust.CustomerAddressLine3,
            CustomerEmail = soldCust != null ? soldCust.CustomerEmail : shipCust.CustomerEmail,
            CustomerName = soldCust != null ? soldCust.CustomerName : shipCust.CustomerName,
            CustomerTelephone = soldCust != null ? soldCust.CustomerTelephone : shipCust.CustomerTelephone,
            EANIdentifier = soldCust != null ? soldCust.EANIdentifier : shipCust.EANIdentifier,
            PostCode = soldCust != null ? soldCust.PostCode : shipCust.PostCode,

            ShipCity = shipCust.City,
            ShipCountry = shipCust.Country,
            ShipCustomerAddressLine1 = shipCust.CustomerAddressLine1,
            ShipCustomerAddressLine2 = shipCust.CustomerAddressLine2,
            ShipCustomerAddressLine3 = shipCust.CustomerAddressLine3,
            ShipCustomerEmail = shipCust.CustomerEmail,
            ShipCustomerName = shipCust.CustomerName,
            ShipCustomerTelephone = shipCust.CustomerTelephone,
            ShipEANIdentifier = shipCust.EANIdentifier,
            ShipPostCode = shipCust.PostCode,
          }
        });
      }

      else
      {
        return Json(new
        {
          customer = new
          {
            ShipCity = shipCust.City,
            ShipCountry = shipCust.Country,
            ShipCustomerAddressLine1 = shipCust.CustomerAddressLine1,
            ShipCustomerAddressLine2 = shipCust.CustomerAddressLine2,
            ShipCustomerAddressLine3 = shipCust.CustomerAddressLine3,
            ShipCustomerEmail = shipCust.CustomerEmail,
            ShipCustomerName = shipCust.CustomerName,
            ShipCustomerTelephone = shipCust.CustomerTelephone,
            ShipEANIdentifier = shipCust.EANIdentifier,
            ShipPostCode = shipCust.PostCode
          }
        });
      }
    }

  }
}
