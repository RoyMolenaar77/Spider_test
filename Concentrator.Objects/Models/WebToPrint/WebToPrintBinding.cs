using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.WebToPrint
{
  public class WebToPrintBinding : BaseModel<WebToPrintBinding>
  {
    public int BindingID { get; set; }
    public string Name { get; set; }
    public string Query { get; set; }
    public string QueryText { get; set; }
    public virtual ICollection<WebToPrintBindingField> WebToPrintBindingFields { get; set; }

    public override System.Linq.Expressions.Expression<Func<WebToPrintBinding, bool>> GetFilter()
    {
      return null;
    }
  }
}
