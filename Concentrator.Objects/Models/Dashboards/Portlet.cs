using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Model.Users;
                  
namespace Concentrator.Objects.Models.Dashboards 
{
  public class Portlet : BaseModel<Portlet>
  {
    public Int32 PortletID { get; set; }
          
    public String Name { get; set; }
          
    public String Title { get; set; }
          
    public String Description { get; set; }
          
    public virtual ICollection<UserPortalPortlet> UserPortalPortlets { get;set;}

    public virtual ICollection<Role> Roles { get; set; }


    public override System.Linq.Expressions.Expression<Func<Portlet, bool>> GetFilter()
    {
      return null;
    }
  }
}