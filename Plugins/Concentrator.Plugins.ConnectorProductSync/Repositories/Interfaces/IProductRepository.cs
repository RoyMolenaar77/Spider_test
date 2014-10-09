using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Plugins.ConnectorProductSync.Models;

namespace Concentrator.Plugins.ConnectorProductSync.Repositories
{
  public interface IProductRepository
  {
    List<Product> GetListOfAllProducts();
    List<ContentInfo> GetListOfMappedProductByConnector(Connector connector);
    List<Product> GetListOfMappedProductsByMasterGroupMapping(int masterGroupMappingID);

    List<VendorProductInfo> GetListOfProductsByConnectorPublicationRule(ConnectorPublicationRule connectorpublicationRule);
  }
}
