using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Services.Base;
using Concentrator.Objects.Models.Management;
using System.Linq.Expressions;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Services
{
  public class ManagementService : Service<ManagementPage>
  {
    public override IQueryable<ManagementPage> GetAll(Expression<Func<ManagementPage, bool>> predicate = null)
    {
      return base.GetAll(predicate).Where(c => (c.isVisible.HasValue ? c.isVisible.Value : true) && Client.User.IsInFunction(c.FunctionalityName));
    }
  }
}
