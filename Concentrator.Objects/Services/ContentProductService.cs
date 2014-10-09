using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Services.Base;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Services
{
  public class ContentProductService : Service<ContentProduct>
  {
    public override IQueryable<ContentProduct> GetAll(System.Linq.Expressions.Expression<Func<ContentProduct, bool>> predicate = null)
    {
      return base.GetAll(predicate).Where(c => ((!Client.User.ConnectorID.HasValue) || (Client.User.ConnectorID.HasValue && c.ConnectorID == Client.User.ConnectorID)));
    }
  }
}
