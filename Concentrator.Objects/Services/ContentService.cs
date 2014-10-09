using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Services.Base;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.Web;
using System.Timers;
using System.Diagnostics;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Results;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Objects.Services
{
  public class ContentService : Service<Content>, IContentService
  {
    #region IContentService Members

    public KeyValuePair<int, IQueryable<MissingContent>> GetMissing(int[] connectors = null, int[] vendors = null, DateTime? beforeDate = null, DateTime? afterDate = null, DateTime? onDate = null, bool? isActive = null, int[] productGroups = null, int[] brands = null, int? lowerStockCount = null, int? greaterStockCount = null, int? equalStockCount = null, int[] statuses = null)
    {
      var missingContent = Repository<MissingContent>().Include(c => c.Brand, c => c.ProductGroup, c => c.Product, c => c.Vendor, c => c.Connector)
        .GetAllAsQueryable();

      if (connectors == null || connectors.Length == 0)
      {
        Client.User.ConnectorID.ThrowIf(c => !c.HasValue, "Connector needs to be specified for this operation");
        connectors = new int[] { Client.User.ConnectorID.Value };
      }
      
      missingContent = missingContent.Where(c => connectors.Contains(c.ConnectorID));

      var total = missingContent.Count();

      if (isActive.HasValue)
        missingContent = missingContent.Where(c => c.isActive == isActive);

      if (vendors != null && vendors.Count() > 0)
        missingContent = missingContent.Where(c => c.ContentVendorID.HasValue && vendors.Contains(c.ContentVendorID.Value));
      else
      {
        if (Client.User.ConnectorID.HasValue)
        {
          vendors = Repository<ContentVendorSetting>().GetAll(x => x.ConnectorID == Client.User.ConnectorID.Value).Select(x => x.VendorID).ToArray();
          missingContent = missingContent.Where(c => c.ContentVendorID.HasValue && vendors.Contains(c.ContentVendorID.Value));
        }
      }

      if (brands != null && brands.Count() > 0)
      {
        var br = Repository<Brand>().GetAllAsQueryable(c => brands.Any(m => m == c.BrandID)).Select(c => c.Name);
        missingContent = missingContent.Where(c => br.Any(m => m == c.BrandName));
      }

      if (beforeDate.HasValue && afterDate.HasValue)
      {
        missingContent = missingContent.Where(x => x.CreationTime < beforeDate.Value && x.CreationTime > afterDate.Value);
      }

      if (beforeDate.HasValue)
      {
        DateTime beginOfDay = new DateTime(beforeDate.Value.Year, beforeDate.Value.Month, beforeDate.Value.Day, 0, 0, 0);
        missingContent = missingContent.Where(x => x.CreationTime < beginOfDay);
      }

      if (afterDate.HasValue)
      {
        DateTime beginOfDay = new DateTime(afterDate.Value.Year, afterDate.Value.Month, afterDate.Value.Day, 0, 0, 0).AddDays(1);
        missingContent = missingContent.Where(x => x.CreationTime > beginOfDay);
      }

      if (onDate.HasValue)
      {
        DateTime beginOfDay = new DateTime(onDate.Value.Year, onDate.Value.Month, onDate.Value.Day, 0, 0, 0);
        DateTime endOfDay = beginOfDay.AddDays(1);
        missingContent = missingContent.Where(x => x.CreationTime > beginOfDay && x.CreationTime < endOfDay);
      }

      if (productGroups != null && productGroups.Count() > 0)
      {
        missingContent = missingContent.Where(c => productGroups.Any(m => ((c.ProductGroupID == m))));
      }

      if (statuses != null && statuses.Count() != 0)
      {
        missingContent = missingContent.Where(c => c.Vendor.VendorStocks.Any(s => (s.ProductID == c.Product.ProductID) && ((s.ConcentratorStatusID.HasValue)? statuses.Contains(s.ConcentratorStatusID.Value) : false)));
      }

      return new KeyValuePair<int, IQueryable<MissingContent>>(total, missingContent);
    }

    public Models.Results.IncompleteMappingInfo GetIncompleteMappingInfo(int connectorID)
    {
      var t = Repository<MissingContent>().GetAllAsQueryable(c => c.ConnectorID == connectorID);

      var info = new IncompleteMappingInfo()
      {
        ProductMatches = Repository<ProductMatch>().Count(p => !p.isMatched),
        UnmatchedProductGroupsCount = Repository<ProductGroupVendor>().Count(p => p.ProductGroupID == -1),
        UnmatchedVendorStatuses = Repository<VendorProductStatus>().Count(p => p.ConcentratorStatusID == -1),
        UnmatchedConnectorStatuses = Repository<ConnectorProductStatus>().Count(p => p.ConcentratorStatusID == -1),
        UnmatchedBrands = Repository<BrandVendor>().Count(p => p.BrandID == -1),
        ProductsNoImages = t.Count(m => !m.Image),
        ProductsSmallImages = Repository<ProductMedia>().Count(s => s.Size < 10),
        MissingSpecifications = t.Count(m => !m.Specifications)
      };
      return info;
    }

    #endregion
  }
}
