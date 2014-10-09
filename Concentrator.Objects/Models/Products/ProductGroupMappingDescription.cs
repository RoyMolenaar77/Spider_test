using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Scan;
using Concentrator.Objects.Models.Slurp;
using Concentrator.Objects.Models.Magento;
using Concentrator.Objects.Models.Localization;

namespace Concentrator.Objects.Models.Products
{
  public class ProductGroupMappingDescription : BaseModel<ProductGroupMappingDescription>
  {
    public override bool Equals(object obj)
    {
      if(obj is ProductGroupMappingDescription)
        return (this.ProductGroupMappingID == ((ProductGroupMappingDescription)obj).ProductGroupMappingID && this.LanguageID == ((ProductGroupMappingDescription)obj).LanguageID);
      else return false;
    }

    public override int GetHashCode()
    {
      return ProductGroupMappingID;
    }

    public Int32 ProductGroupMappingID { get; set; }
    
    public Int32 LanguageID { get; set; }
    
    public String Description { get; set; }

    public virtual ProductGroupMapping ProductGroupMapping { get; set; }

    public virtual Language Language { get; set; }

    public override System.Linq.Expressions.Expression<Func<ProductGroupMappingDescription, bool>> GetFilter()
    {
      return null;
    }
  }
}