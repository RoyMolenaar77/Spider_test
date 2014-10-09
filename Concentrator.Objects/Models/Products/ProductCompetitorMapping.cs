using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Products
{
  public class ProductCompetitorMapping : BaseModel<ProductCompetitorMapping>
  {
    public Int32 ProductCompetitorMappingID { get; set; }
          
    public String Competitor { get; set; }
          
    public Int32 ProductCompareSourceID { get; set; }
          
    public Int32? ProductCompetitorID { get; set; }
          
    public Boolean IncludeShippingCost { get; set; }
          
    public Boolean InTaxPrice { get; set; }
          
    public virtual ProductCompareSource ProductCompareSource { get;set;}
            
    public virtual ProductCompetitor ProductCompetitor { get;set;}
            
    public virtual ICollection<ProductCompetitorPrice> ProductCompetitorPrices { get;set;}

    public override System.Linq.Expressions.Expression<Func<ProductCompetitorMapping, bool>> GetFilter()
    {
      return null;
    }
  }
}