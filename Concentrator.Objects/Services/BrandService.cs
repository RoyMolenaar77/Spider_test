using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Services.Base;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;

namespace Concentrator.Objects.Services
{
  public class BrandService : Service<Brand>
  {
    public override IQueryable<Brand> Search(string queryTerm)
    {
      var query = queryTerm.IfNullOrEmpty("").ToLower();
      return Repository().GetAllAsQueryable(c => c.Name.ToLower().Contains(query));
    }

    public override IQueryable<Brand> GetAll(System.Linq.Expressions.Expression<Func<Brand, bool>> predicate = null)
    {
      var user = Repository<User>().GetSingle(c => c.UserID == Client.User.UserID);

      return base.GetAll(predicate).Where(c => c.BrandVendors.Any(l => user.UserRoles.Select(m => m.VendorID).Contains(l.VendorID)));
    }
  }
}
