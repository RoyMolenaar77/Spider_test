using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Services.DTO;
using Concentrator.Objects.Models.Complex;

namespace Concentrator.Objects.Services.ServiceInterfaces
{
    public interface IFilterService
    {
        /// <summary>
        /// gets a list of attributes and there values for a specific list of products
        /// </summary>
        /// <param name="productAttributeGroupID"></param>
        /// <returns></returns>
      List<AttributeGroup> getFiltersByProducts(List<ProductDto> products);


        /// <summary>
        /// gets all the products specific to a list of product id's that are store behind the scenes
        /// </summary>
        /// <param name="productListIDs"></param>
        /// <returns></returns>
      List<ProductDto> GetProductsWithFilter(List<int> productListIDs);
       


        /// <summary>
        /// this method gets all the products from the DB with is specific ProductGroupID
        /// //it  gets them from the AssortmentContentView table
        /// NOTE: add the ProductGroupID
        /// </summary>
        /// <param name="ProductGroupID"></param>
        /// <returns></returns>
      List<ProductDto> GetAllProductsByGroup();




      /// <summary>
      /// gets all the details for a product by a specific product id
      /// </summary>
      /// <param name="ProductGroupID"></param>
      /// <returns></returns>
      ProductDetailDto getProductDetailsByID(int productID);

      /// <summary>
      /// Returns the grouping of attribute values
      /// </summary>
      /// <param name="connectorID"></param>
      /// <param name="languageID"></param>
      /// <returns></returns>
      List<AttributeValueGroupingResult> GetProductAttributeValueGrouping(int? connectorID, int languageID);
    }
}
