using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Web.ServiceModels
{
  public class JdeCustomerTelephone
  {
    public double BackendRelationID { get; set; }
    public string TelephoneType { get; set; }
    public string AreaCode { get; set; }
    public string PhoneNumber { get; set; }
  }
}
