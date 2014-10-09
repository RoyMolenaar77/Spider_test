using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Services.Base;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Services
{
  public class ProductAttributeGroupNameService : Service<ProductAttributeGroupName>
  {
    //public override IQueryable<ProductAttributeGroupName> Search(string queryTerm)
    //{
    //  queryTerm.IfNullOrEmpty("").ToLower();

    //  return Repository<ProductAttributeName>().GetAllAsQueryable(c => c.LanguageID == Client.User.LanguageID && c.Name.Contains(queryTerm)).Select(c => c.ProductAttributeMetaData);
    //}

    public override IQueryable<ProductAttributeGroupName> Search(string queryTerm)
    {
      var query = queryTerm.IfNullOrEmpty("").ToLower();
      return Repository().GetAllAsQueryable(c => c.Name.ToLower().Contains(query));
    }
  }
}
