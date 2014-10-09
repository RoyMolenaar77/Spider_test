using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Services.Base;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Services
{
  public class ProductGroupLanguageService : Service<ProductGroupLanguage>
  {
    public override IQueryable<ProductGroupLanguage> Search(string queryTerm)
    {
      var query = queryTerm.IfNullOrEmpty("").ToLower();
      return Repository().GetAllAsQueryable(c => c.Name.ToLower().Contains(query));
    }
  }
}
