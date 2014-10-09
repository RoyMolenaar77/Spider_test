using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Products;

namespace Concentrator.Plugins.ConnectorProductSync.Repositories
{
  public interface IProductGroupMappingRepository
  {
    List<ProductGroupMapping> GetListOfProductGroupMappingByConnector(int connectorID);
  }
}
