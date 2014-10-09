using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Management
{
  public class ManagementModuleItem : BaseModel<ManagementModuleItem>
  {
    public Int32 ManagementModuleItemID { get; set; }
          
    public String Name { get; set; }
          
    public Int32 RoleID { get; set; }
          
    public String IconClass { get; set; }
          
    public Boolean IsVisible { get; set; }
          
    public Int32 ModuleID { get; set; }
          
    public String JSAction { get; set; }
          
    public virtual ManagementModule ManagementModule { get;set;}


    public override System.Linq.Expressions.Expression<Func<ManagementModuleItem, bool>> GetFilter()
    {
      return null;
    }
  }
}