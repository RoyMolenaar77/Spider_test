using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Management
{
  public class ManagementModule : BaseModel<ManagementModule>
  {
    public Int32 ModuleID { get; set; }
          
    public String Name { get; set; }
          
    public String IconClass { get; set; }
          
    public virtual ICollection<ManagementModuleItem> ManagementModuleItems { get;set;}


    public override System.Linq.Expressions.Expression<Func<ManagementModule, bool>> GetFilter()
    {
      return null;
    }
  }
}