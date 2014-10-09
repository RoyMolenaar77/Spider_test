using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Statuses;

namespace Concentrator.Objects.Models.Contents
{
  public class ContentPublicationRule : AuditObjectBase<ContentPublicationRule>
  {
    public int ContentPublicationRuleID { get; set; }

    public int ProductContentID { get; set; }

    public int? StatusID { get; set; }

    public bool Publish { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public int RuleIndex { get; set; }

    public int? UpdateFrequentie { get; set; }

    public int? PublicationType { get; set; }

    public virtual AssortmentStatus AssortmentStatus { get; set; }

    public virtual ContentProduct ContentProduct { get; set; }

    public override System.Linq.Expressions.Expression<Func<ContentPublicationRule, bool>> GetFilter()
    {
      return null;
    }
  }
}
