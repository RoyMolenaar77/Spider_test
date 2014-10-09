using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Faq
{
  public class Faq : AuditObjectBase<Faq>
  {
    public int FaqID { get; set; }
    
    public bool? Mandatory { get; set; }

    public virtual ICollection<FaqProduct> FaqProducts { get; set; }

    public virtual ICollection<FaqTranslation> FaqTranslations { get; set; }

    public override System.Linq.Expressions.Expression<Func<Faq, bool>> GetFilter()
    {
      return null;
    }
  }
}
