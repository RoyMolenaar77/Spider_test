using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Attributes;

namespace Concentrator.Plugins.Relation
{

  public class Processor : ConcentratorPlugin
  {
    public int vendorID;
    public int relatedProductTypeID;

    public override string Name
    {
      get { return "Product Group Mapping Relation"; }
    }

    protected override void Process()
    {
      log.Info("Starting product group mapping relation...");

      using (var unit = GetUnitOfWork())
      {
        vendorID = unit.Scope.Repository<Vendor>().GetSingle(x => x.Name == "PFA CC").VendorID;
        
        var productGroupMapping = (from x in unit.Scope.Repository<ProductGroupMapping>().GetAll()
                                   select new
                                   {
                                     x.ProductGroupMappingID,
                                     Relation = x.Relation
                                   }).ToList();

        foreach (var item in productGroupMapping)
        {
          if (item.Relation == null)
            continue;

          var relationList = new List<Int32>();

          relationList = item.Relation.Split(',').Select(f => int.Parse(f)).ToList();


          var relatedProducts = (from i in unit.Scope.Repository<ContentProductGroup>().GetAll()
                                 where relationList.Contains(i.ProductGroupMappingID)
                                 select new
                                 {
                                   i.ProductID
                                 }).ToList();

          if (relatedProducts.Count == 0)
            continue;

          var baseProducts = (from i in unit.Scope.Repository<ContentProductGroup>().GetAll(x => x.ProductGroupMappingID == item.ProductGroupMappingID)
                              select new
                              {
                                i.ProductID
                              }).ToList();

          if (baseProducts.Count == 0)
            continue;

          var relatedBrandList = new List<Product>();
          var baseBrandList = new List<Product>();

          //voor elke base product ga je deze mappen met een related product
          //dan ga je in de db kijken of deze relatie al bestaat
          //zo niet dan voeg je deze toe.
          foreach (var product in baseProducts)
          {
            var relatedProductID = relatedProducts.Select(x => x.ProductID).FirstOrDefault();
            var productExists = unit.Scope.Repository<RelatedProduct>().GetSingle(x => x.RelatedProductID == product.ProductID && relatedProductID == x.ProductID);

            if (productExists != null || product.ProductID == relatedProductID)
              continue;

            //TODO: check vendorID
            var relatedProduct = new RelatedProduct()
            {
              RelatedProductID = product.ProductID,
              ProductID = relatedProductID,
              VendorID = vendorID,
              RelatedProductTypeID = relatedProductTypeID
            };

            unit.Scope.Repository<RelatedProduct>().Add(relatedProduct);
          }
        }

        RelateByBrand(unit);

        unit.Save();
      }
    }



    private void RelateByBrand(IUnitOfWork unit)
    {
      var productList = (from p in unit.Scope.Repository<Product>().GetAll(x => x.BrandID != -1)
                         select new
                         {
                           p.ProductID,
                           p.BrandID,
                         }).GroupBy(z => z.BrandID).ToList();

      foreach (var product in productList)
      {
        var brandID = product.Select(g => g.BrandID).FirstOrDefault();
        var relatedProductID = unit.Scope.Repository<Product>().GetSingle(x => x.BrandID == brandID).ProductID;
        var productID = product.Select(s => s.ProductID).FirstOrDefault();
        var alreadyExists = unit.Scope.Repository<RelatedProduct>().GetSingle(x => x.ProductID == productID && x.RelatedProductID == relatedProductID);

        if (alreadyExists != null || productID == relatedProductID)
          continue;

        // TODO: check vendorID
        var relatedBrand = new RelatedProduct()
        {
          ProductID = productID,
          RelatedProductID = relatedProductID,
          VendorID = vendorID,
          RelatedProductTypeID = relatedProductTypeID
        };

        unit.Scope.Repository<RelatedProduct>().Add(relatedBrand);
      }
    }
  }
}
