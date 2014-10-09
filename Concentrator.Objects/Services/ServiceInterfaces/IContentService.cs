using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Results;

namespace Concentrator.Objects.Services.ServiceInterfaces
{
  public interface IContentService
  {
    /// <summary>
    /// Gets content for connector and information on what is missing from this content. Filterable     
    /// </summary>
    /// <param name="connectors"></param>
    /// <param name="vendors"></param>
    /// <param name="beforeDate"></param>
    /// <param name="afterDate"></param>
    /// <param name="onDate"></param>
    /// <param name="isActive"></param>
    /// <param name="productGroups"></param>
    /// <param name="brands"></param>
    /// <param name="lowerStockCount"></param>
    /// <param name="greaterStockCount"></param>
    /// <param name="equalStockCount"></param>
    /// <returns>Total records, filtered collection</returns>
    KeyValuePair<int, IQueryable<MissingContent>> GetMissing(int[] connectors = null, int[] vendors = null, DateTime? beforeDate= null, DateTime? afterDate = null, DateTime? onDate = null, bool? isActive = null, int[] productGroups = null, int[] brands = null, int? lowerStockCount = null, int? greaterStockCount = null, int? equalStockCount = null, int[] statuses = null);

    /// <summary>
    /// Retrieves incomplete mapping info
    /// </summary>
    /// <returns></returns>
    IncompleteMappingInfo GetIncompleteMappingInfo(int connectorID);
  }
}
