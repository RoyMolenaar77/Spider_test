using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.EDI.Order;
using Concentrator.Objects.Models.EDI.Response;
using Concentrator.Objects.Models.EDI.Post;

namespace Concentrator.Objects.Models.Orders
{
  public class Customer : BaseModel<Customer>
  {
    public Int32 CustomerID { get; set; }

    public String CustomerTelephone { get; set; }

    public String CustomerEmail { get; set; }

    public String City { get; set; }

    public String Country { get; set; }

    public String PostCode { get; set; }

    public String CustomerAddressLine1 { get; set; }

    public String CustomerAddressLine2 { get; set; }

    public String CustomerAddressLine3 { get; set; }

    public String EANIdentifier { get; set; }

    public String CustomerName { get; set; }

    public string Street { get; set; }

    public String HouseNumber { get; set; }

    public string HouseNumberExt { get; set; }

    public String CoCNumber { get; set; }

    public String TaxID { get; set; }

    public String CompanyName { get; set; }

    public string ServicePointCode { get; set; }

    public string ServicePointID { get; set; }

    public string ServicePointName { get; set; }

    public virtual ICollection<Order> ShippedOrder { get; set; }

    public virtual ICollection<Order> SoldOrder { get; set; }

    public virtual ICollection<OrderResponse> ShippedOrderResponse { get; set; }

    public virtual ICollection<OrderResponse> SoldOrderResponse { get; set; }

    public virtual ICollection<EdiOrderResponse> ShippedEdiOrderResponse { get; set; }

    public virtual ICollection<EdiOrderResponse> SoldEdiOrderResponse { get; set; }

    public virtual ICollection<EdiOrder> ShippedEdiOrder { get; set; }

    public virtual ICollection<EdiOrder> SoldEdiOrder { get; set; }

    public override System.Linq.Expressions.Expression<Func<Customer, bool>> GetFilter()
    {
      return null;
    }
  }
}