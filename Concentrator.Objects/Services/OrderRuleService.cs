using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Services.Base;
using Concentrator.Objects.Models.Orders;

namespace Concentrator.Objects.Services
{
  public class OrderRuleService : Service<OrderRule>
  {
    public override IQueryable<OrderRule> Search(string queryTerm)
    {
      var query = queryTerm.IfNullOrEmpty("").ToLower();
      return Repository().GetAllAsQueryable(c => c.Name.ToLower().Contains(query));
    }
  }
}