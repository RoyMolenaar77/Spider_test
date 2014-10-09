using System.Linq;
using Concentrator.Objects.Models.Products;

namespace Concentrator.Objects.Services.ServiceInterfaces
{
  public interface IMasterGroupMappingService
  {
    IQueryable<VendorProducts> GetListOfMasterGroupMappingProducts(int? masterGroupMappingID, bool? isBlocked, string productID, string searchTerm, bool? isProductMapped, bool? hasProductAnImage, int? connectorID);
  }
}
