using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Model.Users;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.Objects.Models
{
  public class UserRole : BaseModel<UserRole>
  {
    public Int32 UserID { get; set; }

    public Int32 RoleID { get; set; }

    public Int32 VendorID { get; set; }    

    public virtual Role Role { get; set; }

    public virtual User User { get; set; }

    public virtual Vendor Vendor { get; set; }

    public override System.Linq.Expressions.Expression<Func<UserRole, bool>> GetFilter()
    {
      return null;
    }
  }
}