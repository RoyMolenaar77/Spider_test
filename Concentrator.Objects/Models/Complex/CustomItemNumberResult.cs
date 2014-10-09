using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;

namespace Concentrator.Objects.Models.Complex
{
  public class CustomItemNumberResult : ComplexObject
  {
    public int productid { get; set; }
    public string customitemnumber { get; set; }
    public int VendorID { get; set; }
    public int ConnectorID { get; set; }
    public bool isPreferred { get; set; }
    public bool isContentVisible { get; set; }
    public string VendorIdentifier { get; set; }
    public bool CentralDelivery { get; set; }
  }
}
