using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Products;

namespace Concentrator.Plugins.ConnectorProductSync.Repositories
{
  public interface IProductGroupRepository
  {
    List<ProductGroup> GetListOfProductGroups();
    List<ProductGroup> GetListOfActiveProductGroups();
    List<ProductGroupLanguage> GetListOfProductGroupLanguagesByProductGroupID(int productGroupID);
    List<ProductGroupVendor> GetListOfMappedVendorProductGroupsByProductGroupID(int productGroupID);
  }
}
