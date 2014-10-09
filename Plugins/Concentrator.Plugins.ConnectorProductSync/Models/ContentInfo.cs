using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.ConnectorProductSync.Models
{
  public class ContentInfo
  {
    public Int32 ProductID { get; set; }
    public Int32 ConnectorID { get; set; }
    public String ShortDescription { get; set; }
    public String LongDescription { get; set; }
    public String LineType { get; set; }
    public String LedgerClass { get; set; }
    public String ProductDesk { get; set; }
    public Boolean? ExtendedCatalog { get; set; }
    public Int32 ConnectorPublicationRuleID { get; set; }
  }
}
