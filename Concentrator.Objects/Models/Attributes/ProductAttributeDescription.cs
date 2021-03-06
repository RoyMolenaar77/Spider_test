using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Localization;

namespace Concentrator.Objects.Models.Attributes
{
  public class ProductAttributeDescription : BaseModel<ProductAttributeDescription>
  {
    public Int32 AttributeID { get; set; }
          
    public Int32 LanguageID { get; set; }
          
    public String Description { get; set; }
          
    public virtual Language Language { get;set;}
            
    public virtual ProductAttributeMetaData ProductAttributeMetaData { get;set;}


    public override System.Linq.Expressions.Expression<Func<ProductAttributeDescription, bool>> GetFilter()
    {
      return null;
    }
  }
}