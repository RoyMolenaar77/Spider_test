using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Model.Users;
using Concentrator.Objects.Models.Templates;

namespace Concentrator.Objects.Models.Management
{
  public class ManagementPage : BaseModel<ManagementPage>
  {
    public int PageID { get; set; }

    public String Name { get; set; }

    public String Description { get; set; }

    public int RoleID { get; set; }

    public String JSAction { get; set; }

    public String Icon { get; set; }

    public int GroupID { get; set; }

    public String ID { get; set; }

    public Boolean? isVisible { get; set; }

    public String FunctionalityName { get; set; }

    public virtual ManagementGroup ManagementGroup { get; set; }

    public ICollection<ExportTemplate> ExportTemplates { get; set; }

    public virtual Role Role { get; set; }



    public override System.Linq.Expressions.Expression<Func<ManagementPage, bool>> GetFilter()
    {
      return null;
    }
  }
}