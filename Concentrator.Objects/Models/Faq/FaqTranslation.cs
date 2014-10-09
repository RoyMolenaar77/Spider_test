using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Localization;

namespace Concentrator.Objects.Models.Faq
{
  public class FaqTranslation : AuditObjectBase<FaqTranslation>
  {
    public int FaqID { get; set; }

    public int LanguageID { get; set; }

    public string Question { get; set; }

    public virtual Language Language { get; set; }

    public virtual Faq Faq { get; set; }

    public override System.Linq.Expressions.Expression<Func<FaqTranslation, bool>> GetFilter()
    {
      return null;
    }
  }
}
