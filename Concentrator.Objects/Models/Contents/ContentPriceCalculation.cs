using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Contents
{
  public class ContentPriceCalculation : AuditObjectBase<ContentPriceCalculation>
  {
    public Int32 ContentPriceCalculationID { get; set; }
          
    public String Name { get; set; }
          
    public String Calculation { get; set; }
          
    public virtual ICollection<ContentPrice> ContentPrices { get;set;}

    public override System.Linq.Expressions.Expression<Func<ContentPriceCalculation, bool>> GetFilter()
    {
      return null;
    }
  }
}