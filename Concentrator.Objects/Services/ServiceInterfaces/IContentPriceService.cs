using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Prices;
using Concentrator.Objects.Models.Results;

namespace Concentrator.Objects.Services.ServiceInterfaces
{
  public interface IContentPriceService
  {
    /// <summary>
    /// Returns prices for a specific mapping level
    /// </summary>
    /// <param name="productGroupMappingID"></param>
    /// <returns></returns>
    IQueryable<ContentPrice> GetPerProductGroupMapping(int productGroupMappingID);

    /// <summary>
    /// Creates a contentprice for a specific productgroup mapping
    /// </summary>
    /// <param name="productGroupMappingID"></param>
    /// <param name="price">The content price object</param>
    void CreateForProductGroupMapping(int productGroupMappingID, ContentPrice price);

    /// <summary>
    /// Creates a contentprice for a specific mastergroup mapping
    /// </summary>
    /// <param name="productGroupMappingID"></param>
    /// <param name="price">The content price object</param>
    void CreateForMasterGroupMapping(int masterGroupMappingID, ContentPrice price);

    /// <summary>
    /// Calculate the price for a specific product and connector
    /// </summary>
    /// <param name="productID"></param>
    /// <param name="formula"></param>
    /// <param name="connectorID"></param>
    /// <returns></returns>
    PriceResult CalculatePrice(int productID, string formula, int? connectorID);

    /// <summary>
    /// Retrieves the calculated price result for a product
    /// </summary>
    /// <param name="productID"></param>
    /// <returns></returns>
    List<ProductPriceResult> GetCalculatedPrice(int productID);
  }
}
