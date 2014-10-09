using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Products;
using PetaPoco;

namespace Concentrator.Plugins.ConnectorProductSync.Repositories
{
  public class ProductGroupMappingRepository : IProductGroupMappingRepository
  {
    private IDatabase petaPoco;

    public ProductGroupMappingRepository(IDatabase petaPoco)
    {
      this.petaPoco = petaPoco;
    }

    public List<ProductGroupMapping> GetListOfProductGroupMappingByConnector(int connectorID)
    {
      List<ProductGroupMapping> productGroupMappins = petaPoco.Fetch<ProductGroupMapping>(string.Format(@"
        SELECT *
        FROM ProductGroupMapping
        WHERE ConnectorID = {0}
      ", connectorID));

      return productGroupMappins;
    }

  }
}
