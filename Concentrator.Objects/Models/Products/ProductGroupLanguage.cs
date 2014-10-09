using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Localization;

namespace Concentrator.Objects.Models.Products
{
  public class ProductGroupLanguage : BaseModel<ProductGroupLanguage>
  {
    public Int32 ProductGroupID { get; set; }
          
    public Int32 LanguageID { get; set; }
          
    public String Name { get; set; }
          
    public virtual Language Language { get;set;}
            
    public virtual ProductGroup ProductGroup { get;set;}


    public override System.Linq.Expressions.Expression<Func<ProductGroupLanguage, bool>> GetFilter()
    {
      return null;
    }
  }
}