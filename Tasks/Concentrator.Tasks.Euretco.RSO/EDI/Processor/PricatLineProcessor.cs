#region Usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Tasks.Euretco.Rso.EDI.Models;
using Concentrator.Tasks.Models;
using MyVendorAssortmentBulk = Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk;

#endregion

namespace Concentrator.Tasks.Euretco.Rso.EDI.Processor
{
  public class PricatLineProcessor : IPricatLineProcessor
  {
    public IEnumerable<MyVendorAssortmentBulk.VendorAssortmentItem> ProcessPricatGroupedLines(
      IUnitOfWork unit,
      TraceSource traceSource,
      Vendor vendor,
      String vendorItemNumber,
      Language defaultLanguage,
      IEnumerable<PricatLine> pricatLines,
      PricatProductAttributeStore productAttributes,
      IDictionary<String, String> pricatColorMapping,
      IEnumerable<PricatSize> pricatSizes)
    {
      #region Configurable product part

      var firstLine = pricatLines.First();

      var brandID = unit.Scope.Repository<Brand>()
                        .GetAll(brand => brand.Name == firstLine.Brand)
                        .Select(brand => brand.BrandID)
                        .SingleOrDefault();

      if (brandID == default(Int32))
      {
        brandID = -1;
      }

      var translatedColor = pricatColorMapping[firstLine.ColorCode].ToTitle();

      var configurableAssortmentItem = new MyVendorAssortmentBulk.VendorAssortmentItem
        {
          VendorProduct = new MyVendorAssortmentBulk.VendorProduct
            {
              IsConfigurable = 1,
              DefaultVendorID = vendor.VendorID,
              VendorID = vendor.VendorID,
              CustomItemNumber = vendorItemNumber,
              VendorItemNumber = vendorItemNumber,
              ShortDescription = firstLine.Description,
              VendorProductGroupCode1 = firstLine.ArticleGroupCode,
              VendorProductGroupCodeName1 = firstLine.ArticleGroupCode,
              VendorBrandCode = firstLine.Brand
            },
          VendorImportAttributeValues = new List<MyVendorAssortmentBulk.VendorImportAttributeValue>
            {
              CreateVendorProductAttributeValue(vendor.VendorID, vendorItemNumber, productAttributes.ProductCodeAttribute.AttributeID, firstLine.ArticleGroupCode,
                                                defaultLanguage),
              CreateVendorProductAttributeValue(vendor.VendorID, vendorItemNumber, productAttributes.ColorAttribute.AttributeID, translatedColor, defaultLanguage),
              CreateVendorProductAttributeValue(vendor.VendorID, vendorItemNumber, productAttributes.ColorCodeAttribute.AttributeID, firstLine.ColorCode,
                                                defaultLanguage),
              CreateVendorProductAttributeValue(vendor.VendorID, vendorItemNumber, productAttributes.ColorSupplierAttribute.AttributeID, firstLine.ColorSupplier,
                                                defaultLanguage),
              CreateVendorProductAttributeValue(vendor.VendorID, vendorItemNumber, productAttributes.SizeRulerAttribute.AttributeID, firstLine.SizeRulerSupplier,
                                                defaultLanguage)
            },
          VendorImportPrices = new List<MyVendorAssortmentBulk.VendorImportPrice>
            {
              new MyVendorAssortmentBulk.VendorImportPrice
                {
                  CustomItemNumber = vendorItemNumber,
                  MinimumQuantity = 0,
                  CommercialStatus = "Unavailable",
                  CostPrice = firstLine.NettoPrice,
                  Price = firstLine.AdvicePrice,
                  SpecialPrice = firstLine.LabelPrice,
                  TaxRate = firstLine.VAT,
                  VendorID = vendor.VendorID,
                  DefaultVendorID = vendor.VendorID,
                }
            },
          VendorProductDescriptions = new List<MyVendorAssortmentBulk.VendorProductDescription>
            {
              new MyVendorAssortmentBulk.VendorProductDescription
                {
                  DefaultVendorID = vendor.VendorID,
                  VendorID = vendor.VendorID,
                  CustomItemNumber = vendorItemNumber,
                  LanguageID = defaultLanguage.LanguageID,
                  ModelName = firstLine.Model,
                  ProductName = firstLine.Description,
                  ShortContentDescription = firstLine.Description,
                }
            },
          VendorImportStocks = new List<MyVendorAssortmentBulk.VendorImportStock>
            {
              new MyVendorAssortmentBulk.VendorImportStock
                {
                  CustomItemNumber = vendorItemNumber,
                  DefaultVendorID = vendor.VendorID,
                  QuantityOnHand = 0,
                  StockStatus = "Configurable",
                  StockType = "Assortment",
                  VendorID = vendor.VendorID,
                }
            }
        };

      traceSource.TraceVerbose("Bulk importing '{0}'.", vendorItemNumber);

      yield return configurableAssortmentItem;

      #endregion

      #region Simple products part

      foreach (var pricatLine in pricatLines)
      {
        var translatedSize = TranslateSize(pricatSizes, pricatLine.Brand, pricatLine.Model, pricatLine.ArticleGroupCode, pricatLine.SizeSupplier);

        var simpleItemNumber = String.Join(" ", vendorItemNumber, translatedSize, pricatLine.SubsizeSupplier).Trim();

        configurableAssortmentItem.RelatedProducts.Add(new MyVendorAssortmentBulk.VendorImportRelatedProduct
          {
            DefaultVendorID = vendor.VendorID,
            VendorID = vendor.VendorID,
            CustomItemNumber = vendorItemNumber,
            IsConfigured = 1,
            RelatedCustomItemNumber = simpleItemNumber,
            RelatedProductType = "Configured",
          });

        traceSource.TraceVerbose("Bulk importing '{0}'.", simpleItemNumber);

        yield return new MyVendorAssortmentBulk.VendorAssortmentItem
          {
            VendorProduct = new MyVendorAssortmentBulk.VendorProduct
              {
                DefaultVendorID = vendor.VendorID,
                VendorID = vendor.VendorID,
                Barcode = pricatLine.ArticleID,
                IsConfigurable = 0,
                CustomItemNumber = simpleItemNumber,
                VendorItemNumber = simpleItemNumber,
                ShortDescription = pricatLine.Description,
                VendorProductGroupCode1 = firstLine.ArticleGroupCode,
                VendorProductGroupCodeName1 = firstLine.ArticleGroupCode,
                VendorBrandCode = pricatLine.Brand
              },
            VendorBarcode = new MyVendorAssortmentBulk.VendorBarcode
              {
                DefaultVendorID = vendor.VendorID,
                VendorID = vendor.VendorID,
                Barcode = pricatLine.ArticleID,
                CustomItemNumber = simpleItemNumber,
                //BarcodeType = 0,      //TODO?
              },
            VendorProductDescriptions = new List<MyVendorAssortmentBulk.VendorProductDescription>
              {
                new MyVendorAssortmentBulk.VendorProductDescription
                  {
                    DefaultVendorID = vendor.VendorID,
                    VendorID = vendor.VendorID,
                    CustomItemNumber = simpleItemNumber,
                    LanguageID = defaultLanguage.LanguageID,
                    ModelName = pricatLine.Model,
                    ProductName = pricatLine.Description,
                    ShortContentDescription = pricatLine.Description
                  }
              },
            VendorImportAttributeValues = new List<MyVendorAssortmentBulk.VendorImportAttributeValue>
              {
                CreateVendorProductAttributeValue(vendor.VendorID, simpleItemNumber, productAttributes.SizeAttribute.AttributeID, translatedSize, defaultLanguage),
                CreateVendorProductAttributeValue(vendor.VendorID, simpleItemNumber, productAttributes.SubsizeAttribute.AttributeID, pricatLine.SubsizeSupplier,
                                                  defaultLanguage),
                CreateVendorProductAttributeValue(vendor.VendorID, simpleItemNumber, productAttributes.SizeSupplierAttribute.AttributeID, pricatLine.SizeSupplier,
                                                  defaultLanguage)
              },
            VendorImportPrices = new List<MyVendorAssortmentBulk.VendorImportPrice>
              {
                new MyVendorAssortmentBulk.VendorImportPrice
                  {
                    DefaultVendorID = vendor.VendorID,
                    VendorID = vendor.VendorID,
                    CustomItemNumber = simpleItemNumber,
                    MinimumQuantity = 0,
                    CommercialStatus = "Active",
                    CostPrice = pricatLine.NettoPrice,
                    Price = pricatLine.AdvicePrice,
                    SpecialPrice = pricatLine.LabelPrice,
                    TaxRate = pricatLine.VAT
                  }
              },
            VendorImportStocks = new List<MyVendorAssortmentBulk.VendorImportStock>
              {
                new MyVendorAssortmentBulk.VendorImportStock
                  {
                    DefaultVendorID = vendor.VendorID,
                    VendorID = vendor.VendorID,
                    CustomItemNumber = simpleItemNumber,
                    QuantityOnHand = 0,
                    StockType = "Assortment",
                    StockStatus = "Active"
                  }
              },
            ProductGroupVendors = configurableAssortmentItem.ProductGroupVendors
          };
      }

      #endregion
    }

    public String TranslateSize(IEnumerable<PricatSize> pricatSizesLookup, String brand, String model, String group, String size)
    {
      var stringComparer = StringComparison.OrdinalIgnoreCase;

      model = model.TrimStart('0');
      group = group.TrimStart('0');

      var pricatSizes = pricatSizesLookup
        .Where(record => record.BrandName.Equals(brand, stringComparer) && record.From.Equals(size, stringComparer))
        .Where(record => record.ModelName.Equals(model, stringComparer) || record.ModelName.Equals(String.Empty, stringComparer))
        .Where(record => record.GroupCode.Equals(group, stringComparer) || record.GroupCode.Equals(String.Empty, stringComparer))
        .ToArray();

      return pricatSizes.Select(pricatSize => pricatSize.To).FirstOrDefault();
    }

    private MyVendorAssortmentBulk.VendorImportAttributeValue CreateVendorProductAttributeValue(int vendorID, string vendorItemNumber, int attributeID, string val,
                                                                                                Language language)
    {
      return new MyVendorAssortmentBulk.VendorImportAttributeValue
        {
          DefaultVendorID = vendorID,
          VendorID = vendorID,
          CustomItemNumber = vendorItemNumber,
          AttributeID = attributeID,
          Value = val,
          LanguageID = language.LanguageID.ToString()
        };
    }
  }
}