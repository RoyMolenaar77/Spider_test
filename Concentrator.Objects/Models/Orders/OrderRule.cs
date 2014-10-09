using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Objects.Models.Orders
{
  public class OrderRule : BaseModel<OrderRule>
  {
    public Int32 RuleID { get; set; }
          
    public String Name { get; set; }
          
    public Int32? Score { get; set; }
          
    public virtual ICollection<ConnectorRuleValue> ConnectorRuleValues { get;set;}

    public override System.Linq.Expressions.Expression<Func<OrderRule, bool>> GetFilter()
    {
      return null;
    }
  }
}