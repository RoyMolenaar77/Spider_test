using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Templates
{
  public class ExportTemplateColumn : AuditObjectBase<ExportTemplateColumn>
  {
    public int ExportTemplateColumnID { get; set; }
    public string Name { get; set; }
    public string SortOrder { get; set; }
    public string FilterOperator { get; set; }
    public string Value { get; set; }
    public string FilterType { get; set; }
    public int ExportTemplateID { get; set; }
    public virtual ExportTemplate ExportTemplate { get; set; }



    public override System.Linq.Expressions.Expression<Func<ExportTemplateColumn, bool>> GetFilter()
    {
      return null;
    }
  }
}
