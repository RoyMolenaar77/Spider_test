using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.ConnectorProductSync.Models
{
  public class VendorProductInfo
  {
    public int ProductID { get; set; }
    public int ConnectorID { get; set; }
    public int ConnectorPublicationRuleID { get; set; }
    public string ShortDescription { get; set; }
    public string LongDescription { get; set; }
    public string LineType { get; set; }
    public string LedgerClass { get; set; }
    public string ProductDesk { get; set; }
    public string ExtendedCatalog { get; set; }
    public int ProductMatchID { get; set; }
    public int PublicationRuleIndex { get; set; }
  }

  public class VendorProductInfoWithWehkampInformation : VendorProductInfo
  {
    public String SentToWehkamp { get; set; }
    public String SentToWehkampAsDummy { get; set; }
  }
}
