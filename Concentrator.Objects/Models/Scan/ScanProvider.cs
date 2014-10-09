using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Scan 
{
  public class ScanProvider : BaseModel<ScanProvider>
  {
    public Int32 ScanProviderID { get; set; }
          
    public String Name { get; set; }
          
    public String Url { get; set; }
          
    public Int32 PriceType { get; set; }
          
    public Boolean IncludeShippingCost { get; set; }
          
    public virtual ScanProvider ScanProvider1 { get;set;}
            
    public virtual ScanProvider ScanProvider2 { get;set;}


    public override System.Linq.Expressions.Expression<Func<ScanProvider, bool>> GetFilter()
    {
      return null;
    }
  }
}