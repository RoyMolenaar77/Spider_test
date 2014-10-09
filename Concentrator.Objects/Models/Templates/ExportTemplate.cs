using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Management;

namespace Concentrator.Objects.Models.Templates
{
  public class ExportTemplate : AuditObjectBase<ExportTemplate>
  {
    public int ExportTemplateID { get; set; }

    public int UserID { get; set; }
    public virtual User User { get; set; }

    public int ManagementPageID { get; set; }
    public virtual ManagementPage ManagementPage { get; set; }

    public string TemplateName { get; set; }

    public virtual ICollection<ExportTemplateColumn> ExportTemplateColumns { get; set; }

    public override System.Linq.Expressions.Expression<Func<ExportTemplate, bool>> GetFilter()
    {
      return null;
    }
  }
}
