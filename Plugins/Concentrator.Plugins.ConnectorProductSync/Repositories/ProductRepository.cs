using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Plugins.ConnectorProductSync.Models;
using System.Globalization;

namespace Concentrator.Plugins.ConnectorProductSync.Repositories
{
  public class ProductRepository : IProductRepository
  {
    private IDatabase petaPoco;

    public ProductRepository(IDatabase petaPoco)
    {
      this.petaPoco = petaPoco;
    }

    public List<Product> GetListOfAllProducts()
    {
      throw new NotImplementedException();
    }

    public List<Product> GetListOfMappedProductsByMasterGroupMapping(int masterGroupMappingID)
    {
      List<Product> products = petaPoco.Fetch<Product>(string.Format(@"
          SELECT * 
          FROM dbo.MasterGroupMappingproduct
          WHERE MasterGroupMappingID = {0}
        ", masterGroupMappingID));

      return products;
    }

    public List<VendorProductInfo> GetListOfProductsByConnectorPublicationRule(ConnectorPublicationRule connectorpublicationRule)
    {
      string additionalFilters = String.Empty;
      string masterGroupMappingFilter = String.Empty;

      string baseQuery = @"
        SELECT VA.VendorID, VA.VendorAssortmentID, min(VP.ConcentratorStatusID) as ConcentratorStatusID, P.BrandID, VA.ProductID, ISNULL(SUM(VS.QuantityOnHand),0) AS QuantityOnHand, max(vp.costprice) as Price, {2} as ConnectorPublicationRuleID
         FROM VendorAssortment VA
	        INNER JOIN Product P ON (VA.ProductID = P.ProductID)
          {3}
	        INNER JOIN VendorStock VS ON (VA.VendorID = VS.VendorID AND VA.ProductID = VS.ProductID)
	        INNER JOIN VendorPrice VP ON (VA.VendorAssortmentID = VP.VendorAssortmentID)
	        WHERE va.IsActive = 1 AND VA.VendorID = {0} {1}
        GROUP BY VA.VendorID, VA.VendorAssortmentID, P.BrandID, VA.ProductID";

      if (connectorpublicationRule.BrandID.HasValue)
      {
        additionalFilters += String.Format(" AND P.BrandID = {0}", connectorpublicationRule.BrandID);
      }

      if (connectorpublicationRule.MasterGroupMappingID.HasValue && connectorpublicationRule.MasterGroupMappingID.Value > 0)
      {
        masterGroupMappingFilter = String.Format("INNER JOIN dbo.MasterGroupMappingProduct mgmp ON p.ProductID = mgmp.ProductID");
        additionalFilters += String.Format(" AND mgmp.MasterGroupMappingID = {0}", connectorpublicationRule.MasterGroupMappingID);
      }

      if (connectorpublicationRule.ProductID.HasValue)
      {
        additionalFilters += String.Format(" AND va.productid = {0}", connectorpublicationRule.ProductID);
      }

      if (connectorpublicationRule.PublishOnlyStock.HasValue && connectorpublicationRule.PublishOnlyStock.Value)
      {
        additionalFilters += " AND VS.QuantityOnHand > 0";
      }

      if (connectorpublicationRule.StatusID.HasValue)
      {
        additionalFilters += String.Format(" AND VP.ConcentratorStatusID = {0}", connectorpublicationRule.StatusID.Value);
      }

      if (connectorpublicationRule.FromPrice.HasValue)
      {
        additionalFilters += String.Format(" AND vp.Price > {0}", connectorpublicationRule.FromPrice.Value.ToString(CultureInfo.InvariantCulture));
      }

      if (connectorpublicationRule.ToPrice.HasValue)
      {
        additionalFilters += String.Format(" AND vp.Price < {0}", connectorpublicationRule.ToPrice.Value.ToString(CultureInfo.InvariantCulture));
      }

      List<VendorProductInfo> products = petaPoco.Query<VendorProductInfo>(String.Format(baseQuery, connectorpublicationRule.VendorID, additionalFilters, connectorpublicationRule.ConnectorPublicationRuleID,masterGroupMappingFilter)).ToList();

      return products;
    }

    public List<ContentInfo> GetListOfMappedProductByConnector(Connector connector)
    {
      List<ContentInfo> products = petaPoco.Fetch<ContentInfo>(string.Format(@"
        SELECT DISTINCT p.ProductID
	        ,m.ConnectorID
	        ,va.ShortDescription
	        ,va.LongDescription
	        ,va.LineType
	        ,va.LedgerClass
	        ,va.ProductDesk
	        ,va.ExtendedCatalog
          ,cpr.ConnectorPublicationRuleID
        FROM MasterGroupMapping m
        INNER JOIN MasterGroupMappingProduct mp ON m.MasterGroupMappingID = mp.MasterGroupMappingID
        INNER JOIN Product p ON p.ProductID = mp.ProductID
        INNER JOIN ConnectorPublicationRule cpr ON mp.ConnectorPublicationRuleID = cpr.ConnectorPublicationRuleID
        INNER JOIN VendorAssortment va ON va.ProductID = p.ProductID
        WHERE m.ConnectorID = {0} and va.VendorID = cpr.VendorID
      ", connector.ConnectorID));

      return products;      
    }
  }
}
