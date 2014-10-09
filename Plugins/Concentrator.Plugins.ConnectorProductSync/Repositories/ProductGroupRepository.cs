using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Products;
using PetaPoco;

namespace Concentrator.Plugins.ConnectorProductSync.Repositories
{
  public class ProductGroupRepository : IProductGroupRepository
  {
    private IDatabase petaPoco;

    public ProductGroupRepository(IDatabase petaPoco)
    {
      this.petaPoco = petaPoco;
    }

    public List<ProductGroup> GetListOfProductGroups()
    {
      List<ProductGroup> listOfAllProductGroups = petaPoco.Fetch<ProductGroup>(@"
        SELECT *
        FROM ProductGroup
      ");

      return listOfAllProductGroups;
    }

    public List<ProductGroup> GetListOfActiveProductGroups()
    {
      List<ProductGroup> listOfActiveProductGroups = petaPoco.Fetch<ProductGroup>(@"
        SELECT DISTINCT pg.*
        FROM ProductGroup pg
        INNER JOIN ProductGroupMapping pgm ON pg.ProductGroupID = pgm.ProductGroupID
      ");

      return listOfActiveProductGroups;
    }

    public List<ProductGroupLanguage> GetListOfProductGroupLanguagesByProductGroupID(int productGroupID)
    {
      List<ProductGroupLanguage> listOfProductGroupLanguages = petaPoco.Fetch<ProductGroupLanguage>(string.Format(@"
        SELECT pgl.*
        FROM ProductGroup pg
        INNER JOIN ProductGroupLanguage pgl ON pg.ProductGroupID = pgl.ProductGroupID
        WHERE pg.ProductGroupID = {0}
      ", productGroupID));

      return listOfProductGroupLanguages;
    }

    public List<ProductGroupVendor> GetListOfMappedVendorProductGroupsByProductGroupID(int productGroupID)
    {
      List<ProductGroupVendor> listOfMappedVendorProductGroups = petaPoco.Fetch<ProductGroupVendor>(string.Format(@"
        SELECT pgv.*
        FROM ProductGroup pg
        INNER JOIN ProductGroupVendor pgv ON pg.ProductGroupID = pgv.ProductGroupID
        WHERE pg.ProductGroupID = {0}
      ", productGroupID));

      return listOfMappedVendorProductGroups;
    }
  }
}
