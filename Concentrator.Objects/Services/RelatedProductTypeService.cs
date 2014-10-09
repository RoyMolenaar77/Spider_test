using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Services.Base;

namespace Concentrator.Objects.Services
{
  public class RelatedProductTypeService : Service<RelatedProductType>
  {
    public override IQueryable<RelatedProductType> Search(string queryTerm)
    {
      queryTerm = queryTerm.IfNullOrEmpty("").ToLower();

      return base.GetAll(c => c.Type.ToLower().Contains(queryTerm));
    }
  }
}
