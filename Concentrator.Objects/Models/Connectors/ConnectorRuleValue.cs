using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.Objects.Models.Connectors
{
  public class ConnectorRuleValue : BaseModel<ConnectorRuleValue>
  {
    public Int32 ConnectorID { get; set; }
          
    public Int32 RuleID { get; set; }
          
    public Int32 VendorID { get; set; }
          
    public Int32 Value { get; set; }
          
    public String Description { get; set; }
          
    public virtual Connector Connector { get;set;}
            
    public virtual OrderRule OrderRule { get;set;}
            
    public virtual Vendor Vendor { get;set;}


    public override System.Linq.Expressions.Expression<Func<ConnectorRuleValue, bool>> GetFilter()
    {
      return null;
    }
  }
}