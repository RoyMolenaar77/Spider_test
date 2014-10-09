using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Localization;

namespace Concentrator.Objects.Models.Attributes
{
  public class ProductAttributeGroupName : BaseModel<ProductAttributeGroupName>
  {
    public String Name { get; set; }
          
    public Int32 ProductAttributeGroupID { get; set; }
          
    public Int32 LanguageID { get; set; }
          
    public virtual Language Language { get;set;}
            
    public virtual ProductAttributeGroupMetaData ProductAttributeGroupMetaData { get;set;}


    public override System.Linq.Expressions.Expression<Func<ProductAttributeGroupName, bool>> GetFilter()
    {
      return null;
    }
  }
}