using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
                  
namespace Concentrator.Objects.Models.Dashboards
{
  public class UserPortalPortlet : BaseModel<UserPortalPortlet>
  {
    public Int32 UserID { get; set; }
          
    public Int32 PortalID { get; set; }
          
    public Int32 PortletID { get; set; }
          
    public Int32 Column { get; set; }
          
    public Int32 Row { get; set; }
          
    public virtual Portlet Portlet { get;set;}
            
    public virtual UserPortal UserPortal { get;set;}


    public override System.Linq.Expressions.Expression<Func<UserPortalPortlet, bool>> GetFilter()
    {
      return null;
    }
  }
}