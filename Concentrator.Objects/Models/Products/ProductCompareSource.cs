using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Slurp;
using Concentrator.Objects.Models.Contents;

namespace Concentrator.Objects.Models.Products
{
  public class ProductCompareSource : BaseModel<ProductCompareSource>
  {
    public Int32 ProductCompareSourceID { get; set; }

    public String Source { get; set; }

    public Int32? ProductCompareSourceParentID { get; set; }

    public String ProductCompareSourceType { get; set; }

    public Boolean IsActive { get; set; }

    public virtual ICollection<ProductCompareSource> ChildProductCompareSources { get; set; }

    public virtual ProductCompareSource ParentProductCompareSource { get; set; }

    public virtual ICollection<ProductCompetitorMapping> ProductCompetitorMappings { get; set; }

    public virtual ICollection<SlurpQueue> SlurpQueues { get; set; }

    public virtual ICollection<SlurpSchedule> SlurpSchedules { get; set; }

    public virtual ICollection<ProductCompare> ProductCompares { get; set; }
    public virtual ICollection<ContentPrice> ContentPrices { get; set; }

    public override System.Linq.Expressions.Expression<Func<ProductCompareSource, bool>> GetFilter()
    {
      return null;
    }
  }
}