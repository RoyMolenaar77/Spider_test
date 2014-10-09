using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Models.Products;

namespace Concentrator.Objects.Models.Faq
{
  public class FaqProduct : AuditObjectBase<FaqProduct>
  {
    public int FaqID { get; set; }

    public int LanguageID { get; set; }

    public int ProductID { get; set; }

    public string Answer { get; set; }

    public virtual Faq Faq { get; set; }

    public Language Language { get; set; }

    public Product Product { get; set; }

    public override System.Linq.Expressions.Expression<Func<FaqProduct, bool>> GetFilter()
    {
      return null;
    }
  }
}
