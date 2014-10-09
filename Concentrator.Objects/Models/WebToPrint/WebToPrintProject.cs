using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.WebToPrint
{
  public class WebToPrintProject : BaseModel<WebToPrintProject>
  {
    public int ProjectID { get; set; }
    public int UserID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public virtual ICollection<WebToPrintComposite> WebToPrintComposites { get; set; }
    public virtual ICollection<WebToPrintDocument> WebToPrintDocuments { get; set; }
    public virtual ICollection<WebToPrintPage> WebToPrintPages { get; set; }
    public virtual ICollection<WebToPrintQueue> WebToPrintQueues { get; set; }
    public virtual ICollection<WebToPrintLayout> WebToPrintLayouts { get; set; }
    public virtual User User {get; set;}

    public override System.Linq.Expressions.Expression<Func<WebToPrintProject, bool>> GetFilter()
    {
      return null;
    }
  }
}
