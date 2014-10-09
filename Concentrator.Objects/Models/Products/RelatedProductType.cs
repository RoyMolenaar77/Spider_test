using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Products
{
  public class RelatedProductType : BaseModel<RelatedProductType>
  {
    public Int32 RelatedProductTypeID
    {
      get;
      set;
    }

    public String Type
    {
      get;
      set;
    }

    public bool IsConfigured
    {
      get;
      set;
    }

    public virtual ICollection<RelatedProduct> RelatedProducts
    {
      get;
      set;
    }

    public int? TypeMapsToMagentoTypeID
    {
      get;
      set;
    }

    public override System.Linq.Expressions.Expression<Func<RelatedProductType, bool>> GetFilter()
    {
      return null;
    }
  }
}