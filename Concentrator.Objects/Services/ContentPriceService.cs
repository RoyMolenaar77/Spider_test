using System;
using System.Collections.Generic;
using System.Linq;

using Concentrator.Configuration;
using Concentrator.Objects.Enumerations;
using Concentrator.Objects.Logic;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.MastergroupMapping;
using Concentrator.Objects.Models.Prices;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Results;
using Concentrator.Objects.Services.Base;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Services
{
  public class ContentPriceService : Service<ContentPrice>, IContentPriceService
  {
    public override IQueryable<ContentPrice> GetAll(System.Linq.Expressions.Expression<Func<ContentPrice, bool>> predicate = null)
    {
      return Client.User.ConnectorID.HasValue
        ? base.GetAll(predicate).Where(c => c.ConnectorID == Client.User.ConnectorID)
        : base.GetAll(predicate);
    }

    public override void Create(ContentPrice model)
    {
      if (ConcentratorSection.Default.Management.Commercial.FixedPriceRequiresProduct && model.FixedPrice.HasValue && !model.ProductID.HasValue)
      {
        throw new InvalidOperationException("If a fixed price is specified, a product needs to be set");
      }

      base.Create(model);
    }

    #region IContentPriceService Members

    public IQueryable<ContentPrice> GetPerProductGroupMapping(int productGroupMappingID)
    {
      return base.GetAll().Where(c => c.ProductGroupID == Repository<ProductGroupMapping>().GetSingle(p => p.ProductGroupMappingID == productGroupMappingID).ProductGroupID);
    }

    public void CreateForProductGroupMapping(int productGroupMappingID, ContentPrice price)
    {
      price.ProductGroupID = Repository<ProductGroupMapping>().GetSingle(c => c.ProductGroupMappingID == productGroupMappingID).ProductGroupID;
      Create(price);
    }

    public void CreateForMasterGroupMapping(int masterGroupMappingID, ContentPrice price)
    {
      price.ProductGroupID = Repository<MasterGroupMapping>().GetSingle(c => c.MasterGroupMappingID == masterGroupMappingID).ProductGroupID;
      Create(price);
    }

    public PriceResult CalculatePrice(int productID, string formula, int? connectorID)
    {
      connectorID.ThrowIf(c => !c.HasValue, "Connector must be supplied");

      ContentLogic logic = new ContentLogic(Scope, connectorID.Value);

      var price = logic.RoundPrice(productID, formula, connectorID.Value);

      return new Models.Prices.PriceResult() { CalculatePrice = price.Key, CostPrice = price.Value };
    }

    public List<ProductPriceResult> GetCalculatedPrice(int productID)
    {

      var products = (from p in Repository<Content>().GetAll()
                      let vendorass = p.Product.VendorAssortments.FirstOrDefault(x => x.VendorID == p.ContentProduct.VendorID)//.VendorPrice.FirstOrDefault().Price)
                      let vendorprice = vendorass != null ? vendorass.VendorPrices.FirstOrDefault() : null
                      let price = vendorprice != null ? vendorprice.Price : 0m
                      where p.ProductID == productID
                      select new ProductPriceResult
                      {
                        CustomItemNumber = vendorass.CustomItemNumber,
                        CostPrice = vendorprice.Price,
                        CalculatedPrice = 0.0m,//priceRule.Value,
                        Marge = 0, //- priceRule.Value
                        ProductStatus = vendorprice.AssortmentStatus.ConnectorProductStatus.FirstOrDefault(y => y.ConnectorID == p.ConnectorID) != null ?
                        vendorprice.AssortmentStatus.ConnectorProductStatus.FirstOrDefault(y => y.ConnectorID == p.ConnectorID).ConnectorStatus : string.Empty,
                        ShortDescription = p.ShortDescription,
                        LongDescription = p.LongDescription,
                        PriceRuleID = 0,//priceRule.Key,
                        ConnectorDescription = p.Connector.Name,
                        ConnectorID = p.ConnectorID
                      }).ToList();

      var connectorRepo = Repository<Connector>();

      foreach (var product in products)
      {
        ContentLogic logic = new ContentLogic(Scope, product.ConnectorID);

        var connector = connectorRepo.GetSingle(c => c.ConnectorID == product.ConnectorID);

        var pRule = logic.GetCalculationPriceAndRule(productID, 1, connector, PriceRuleType.UnitPrice);
        product.CalculatedPrice = pRule.Value;
        product.PriceRuleID = pRule.Key;
        product.Marge = pRule.Value - product.CostPrice.Value;
      }
      return products.ToList();
    }

    #endregion
  }
}
