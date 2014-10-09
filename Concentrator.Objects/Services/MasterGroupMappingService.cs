using System;
using System.Globalization;
using System.Linq;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.Services.Base;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.MastergroupMapping;

namespace Concentrator.Objects.Services
{
  public class MasterGroupMappingService : Service<MasterGroupMapping>, IMasterGroupMappingService
  {
    public IQueryable<VendorProducts> GetListOfMasterGroupMappingProducts(int? masterGroupMappingID, bool? isBlocked, string productID, string searchTerm, bool? isProductMapped, bool? hasProductAnImage, int? connectorID)
    {
      try
      {
        using (var pDb = new PetaPoco.Database(Environments.Environments.Current.Connection, "System.Data.SqlClient"))
        {
          var filterMasterGroupMappingID = string.Format("@@MasterGroupMappingID = {0}", masterGroupMappingID.HasValue ? masterGroupMappingID.Value.ToString(CultureInfo.InvariantCulture) : "NULL");
          var filterIsBlocked = string.Format("@@IsBlocked = {0}", isBlocked.HasValue ? (isBlocked.Value ? "1" : "0") : "NULL");
          var filterIsProductMapped = string.Format("@@IsProductMapped = {0}", isProductMapped.HasValue ? (isProductMapped.Value ? "1" : "0") : "NULL");
          var filterHasProductAnImage = string.Format("@@HasProductAnImage = {0}", hasProductAnImage.HasValue ? (hasProductAnImage.Value ? "1" : "0") : "NULL");

          var filterProductID = string.Format("@@ProductID = {0}", productID != null ? "'" + productID + "'" : "NULL");
          var filterSearchTerm = string.Format("@@SearchTerm = {0}", searchTerm != null ? "'" + searchTerm + "'" : "NULL");
          var connectorSearchTerm = string.Format("@@ConnectorID = {0}", connectorID.HasValue ? connectorID.Value.ToString() : "NULL");

          var query = string.Format("exec sp_MasterGroupMapping_FetchProducts {0}, {1}, {2}, {3}, {4}, {5}, {6}",
            filterMasterGroupMappingID, filterIsBlocked, filterProductID, filterSearchTerm, filterIsProductMapped, filterHasProductAnImage, connectorSearchTerm);
          var listOfProducts = pDb.Query<VendorProducts>(query);

          return listOfProducts.AsQueryable();
        }
      }
      catch (Exception ex)
      {
        throw new Exception("Failed to load master group mapping data", ex);
      }
    }
  }
}
