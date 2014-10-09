using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using CsvHelper;

namespace Concentrator.Tasks.Vlisco.Importers
{
  using Common;
  using Models;
  using Objects.Models;
  using Objects.Models.Attributes;
  using Objects.Models.Localization;
  using Objects.Models.Products;
  using Objects.Models.Vendors;
  using Objects.Monitoring;
  using Objects.Vendors.Bulk;

  [Task(Constants.Vendor.Vlisco + " Article Importer Task")]
  public class ArticleImporterTask : VendorTaskBase
  {
    private FeeblMonitoring _monitoring;

    private ArticlePropertyStore PropertyStore
    {
      get;
      set;
    }

    [VendorSetting(Constants.Vendor.Setting.Location, true)]
    private String Location
    {
      get;
      set;
    }

    private IEnumerable<Language> Languages
    {
      get;
      set;
    }

    private IDictionary<String, Vendor> TariffVendors
    {
      get;
      set;
    }

    private Int32 VendorID
    {
      get
      {
        return Context.VendorID;
      }
    }

    private String VendorName
    {
      get
      {
        return Context.Name;
      }
    }

    public ArticleImporterTask()
    {
      PropertyStore = new ArticlePropertyStore();
    }

    protected override void ExecuteVendorTask()
    {
      _monitoring = new FeeblMonitoring();
      _monitoring.Notify(Name, 0);

      if (Location.IsNullOrWhiteSpace())
      {
        TraceError("The connector setting '{0}' does not exists or is empty!", Constants.Vendor.Setting.Location);
      }
      else
      {
        var locationInfo = new DirectoryInfo(Location);

        if (!locationInfo.FullName.EndsWith("$") && !locationInfo.Exists)
        {
          TraceError("The directory '{0}' does not exists!", locationInfo.FullName);
        }
        else if (!locationInfo.FullName.EndsWith("$") && !locationInfo.HasAccess(FileSystemRights.FullControl))
        {
          TraceError("The user '{0}' has insufficient access over the directory '{1}'!", WindowsIdentity.GetCurrent().Name, locationInfo.FullName);
        }
        else if (ProductAttributeHelper.Bind(PropertyStore, Context, TraceSource))
        {
          Languages = Unit.Scope
            .Repository<Language>()
            .GetAll(language => language.Name == Constants.Language.English)
            .ToArray();

          TariffVendors = Unit.Scope
            .Repository<Vendor>()
            .Include(vendor => vendor.VendorSettings)
            .GetAll(vendor => vendor.ParentVendorID == VendorID)
            .AsEnumerable()
            .Where(vendor
              => vendor.GetVendorSetting(Constants.Vendor.Setting.IsTariff, false)
              && !vendor.GetVendorSetting(Constants.Vendor.Setting.CountryCode).IsNullOrWhiteSpace()
              && !vendor.GetVendorSetting(Constants.Vendor.Setting.CurrencyCode).IsNullOrWhiteSpace())
            .ToDictionary(GetTariffCode);

          var files = locationInfo.GetFiles("*.csv", SearchOption.TopDirectoryOnly);

          TraceInformation("Found {0} CSV-files for import!", files.Length);

          foreach (var fileInfo in files)
          {
            TraceInformation("Importing the file '{0}'...", fileInfo);

            var articles = GetArticles(fileInfo.FullName);

            if (articles != null)
            {
              var enumerableArticles = articles as Article[] ?? articles.ToArray();
              ImportTariffVendors(enumerableArticles);

              var vendorAssortmentItems = GetVendorAssortments(enumerableArticles).ToArray();

              var success = true;

              foreach (var vendorAssortmentGrouping in vendorAssortmentItems.GroupBy(vendorAssortmentItem => vendorAssortmentItem.VendorProduct.VendorID))
              {
                TraceInformation("Importing assortment for vendor '{0}'...", vendorAssortmentGrouping.Key != VendorID
                  ? Unit.Scope.Repository<Vendor>().GetSingle(vendor => vendor.VendorID == vendorAssortmentGrouping.Key).Name
                  : VendorName);

                var bulkConfig = new VendorAssortmentBulkConfiguration
                {
                  IsPartialAssortment = true
                };

                using (var bulk = new VendorAssortmentBulk(vendorAssortmentGrouping, vendorAssortmentGrouping.Key, VendorID, bulkConfig))
                {
                  try
                  {
                    bulk.Init(Unit.Context);
                    bulk.Sync(Unit.Context);
                  }
                  catch (Exception exception)
                  {
                    success = false;
                    TraceCritical(exception);
                  }
                }
              }

              fileInfo.CopyTo(fileInfo.FullName + (success ? ".processed" : ".failed"), true);
              fileInfo.Delete();
            }
          }

          ImportProductConfiguration(PropertyStore.ColorCode, PropertyStore.SizeCode);
          ImportProductGroups();
        }
      }
      _monitoring.Notify(Name, 1);
    }

    private IEnumerable<Article> GetArticles(String filePath)
    {
      var articleList = new List<Article>();

      try
      {
        using (var streamReader = new StreamReader(filePath))
        using (var csvReader = new CsvReader(streamReader))
        {
          csvReader.Configuration.Delimiter = ";";
          csvReader.Configuration.RegisterClassMap<ArticleMapping>();

          TraceInformation("Reading article information from '{0}'...", filePath);

          while (csvReader.Read())
          {
            try
            {
              articleList.Add(csvReader.GetRecord<Article>());
            }
            catch (CsvHelperException exception)
            {
              var stringBuilder = new StringBuilder();

              stringBuilder.AppendFormat("Unable to read the article at row {0}.", csvReader.Row);

              if (exception.Data.Contains("CsvHelper"))
              {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine(exception.Data["CsvHelper"].ToString());
              }

              TraceWarning(stringBuilder.ToString());
            }
          }

          TraceInformation("Reading article information from '{0}'... {1} articles read!", Location, articleList.Count);
        }

        return articleList;
      }
      catch (Exception exception)
      {
        TraceCritical(exception);
      }

      return null;
    }

    private String GetTariffCode(Article article)
    {
      return GetTariffCode(article.CountryCode, article.CurrencyCode);
    }

    private String GetTariffCode(Vendor vendor)
    {
      var countryCode = vendor.GetVendorSetting(Constants.Vendor.Setting.CountryCode);
      var currencyCode = vendor.GetVendorSetting(Constants.Vendor.Setting.CurrencyCode);

      return GetTariffCode(countryCode, currencyCode);
    }

    private String GetTariffCode(String countryCode, String currencyCode)
    {
      return String.Join(Constants.CurrencySymbol, countryCode, currencyCode);
    }

    private IEnumerable<VendorAssortmentBulk.VendorAssortmentItem> GetVendorAssortments(IEnumerable<Article> articles)
    {
      foreach (var articleGrouping in articles.GroupBy(article => article.ArticleCode))
      {
        TraceInformation("Importing article '{0}'...", articleGrouping.Key);

        yield return GetVendorAssortmentForConfigurableProduct(articleGrouping.First());

        foreach (var article in articleGrouping.Distinct(ArticleColorSizeComparer.Default))
        {
          if (!article.ColorCode.Equals(Constants.IgnoreCode))
          {
            yield return GetVendorAssortmentForConfigurableProductWithColorLevel(article); //Relate Configurable product with Color product.
            yield return GetVendorAssortmentForSimpleProductWithColorLevel(article); // Relate Style product to the Color product
          }

          yield return GetVendorAssortmentForSimpleProduct(article);
        }

        foreach (var article in articleGrouping.Distinct(ArticleCommercialComparer.Default))
        {
          yield return GetVendorAssortmentForCommercialProduct(article);
          yield return GetVendorAssortmentForCommercialColorProduct(article);
        }
      }
    }

    private VendorAssortmentBulk.VendorProduct GetVendorAssortmentColorProduct(Article article, Boolean isConfigurable, Vendor vendor)
    {
      var colorVendorItemNumber = article.GetColorVendorItemNumber();

      return new VendorAssortmentBulk.VendorProduct
      {
        Barcode = !isConfigurable && vendor == Context ? colorVendorItemNumber : null,
        CustomItemNumber = colorVendorItemNumber,
        DefaultVendorID = VendorID,
        IsConfigurable = Convert.ToInt32(isConfigurable),
        LongDescription = article.DescriptionLong,
        ShortDescription = article.DescriptionShort,
        VendorID = vendor.VendorID,
        VendorBrandCode = article.ArticleCode.Substring(0, 2),
        VendorItemNumber = colorVendorItemNumber,
        VendorProductGroupCode1 = !article.CategoryCode.IsNullOrWhiteSpace()
          ? article.CategoryCode
          : Constants.DiversCode,
        VendorProductGroupCodeName1 = !article.CategoryCode.IsNullOrWhiteSpace()
          ? article.CategoryName
          : Constants.DiversName,
        VendorProductGroupCode2 = !article.FamilyCode.IsNullOrWhiteSpace()
          ? article.FamilyCode
          : Constants.DiversCode,
        VendorProductGroupCodeName2 = !article.FamilyCode.IsNullOrWhiteSpace()
          ? article.FamilyName
          : Constants.DiversName,
        VendorProductGroupCode3 = !article.SubfamilyCode.IsNullOrWhiteSpace()
          ? article.SubfamilyCode
          : Constants.DiversCode,
        VendorProductGroupCodeName3 = !article.SubfamilyCode.IsNullOrWhiteSpace()
          ? article.SubfamilyName
          : Constants.DiversName
      };
    }

    private VendorAssortmentBulk.VendorProduct GetVendorAssortmentProduct(Article article, Boolean isConfigurable, Vendor vendor)
    {
      var vendorItemNumber = isConfigurable ? article.ArticleCode : article.GetVendorItemNumber();

      return new VendorAssortmentBulk.VendorProduct
      {
        Barcode = !isConfigurable && vendor == Context ? article.Barcode : null,
        CustomItemNumber = vendorItemNumber,
        DefaultVendorID = VendorID,
        IsConfigurable = Convert.ToInt32(isConfigurable),
        LongDescription = article.DescriptionLong,
        ShortDescription = article.DescriptionShort,
        VendorID = vendor.VendorID,
        VendorBrandCode = article.ArticleCode.Substring(0, 2),
        VendorItemNumber = vendorItemNumber,
        VendorProductGroupCode1 = !article.CategoryCode.IsNullOrWhiteSpace()
          ? article.CategoryCode
          : Constants.DiversCode,
        VendorProductGroupCodeName1 = !article.CategoryCode.IsNullOrWhiteSpace()
          ? article.CategoryName
          : Constants.DiversName,
        VendorProductGroupCode2 = !article.FamilyCode.IsNullOrWhiteSpace()
          ? article.FamilyCode
          : Constants.DiversCode,
        VendorProductGroupCodeName2 = !article.FamilyCode.IsNullOrWhiteSpace()
          ? article.FamilyName
          : Constants.DiversName,
        VendorProductGroupCode3 = !article.SubfamilyCode.IsNullOrWhiteSpace()
          ? article.SubfamilyCode
          : Constants.DiversCode,
        VendorProductGroupCodeName3 = !article.SubfamilyCode.IsNullOrWhiteSpace()
          ? article.SubfamilyName
          : Constants.DiversName
      };
    }

    private VendorAssortmentBulk.VendorProduct GetColorVendorAssortmentProduct(Article article, bool isConfigurable, Vendor vendor)
    {
      var colorCustomItemNumber = article.GetColorVendorItemNumber();

      var returnProduct = new VendorAssortmentBulk.VendorProduct
      {
        Barcode = !isConfigurable && vendor == Context ? article.Barcode : null,
        CustomItemNumber = colorCustomItemNumber,
        DefaultVendorID = VendorID,
        IsConfigurable = Convert.ToInt32(isConfigurable),
        LongDescription = article.DescriptionLong,
        ShortDescription = article.DescriptionShort,
        VendorID = vendor.VendorID,
        VendorBrandCode = article.ArticleCode.Substring(0, 2),
        VendorItemNumber = colorCustomItemNumber,
        VendorProductGroupCode1 = !article.CategoryCode.IsNullOrWhiteSpace()
          ? article.CategoryCode
          : Constants.DiversCode,
        VendorProductGroupCodeName1 = !article.CategoryCode.IsNullOrWhiteSpace()
          ? article.CategoryName
          : Constants.DiversName,
        VendorProductGroupCode2 = !article.FamilyCode.IsNullOrWhiteSpace()
          ? article.FamilyCode
          : Constants.DiversCode,
        VendorProductGroupCodeName2 = !article.FamilyCode.IsNullOrWhiteSpace()
          ? article.FamilyName
          : Constants.DiversName,
        VendorProductGroupCode3 = !article.SubfamilyCode.IsNullOrWhiteSpace()
          ? article.SubfamilyCode
          : Constants.DiversCode,
        VendorProductGroupCodeName3 = !article.SubfamilyCode.IsNullOrWhiteSpace()
          ? article.SubfamilyName
          : Constants.DiversName
      };

      return returnProduct;
    }

    private List<VendorAssortmentBulk.VendorProductDescription> GetVendorAssortmentProductDescriptions(Article article, string customItemNumber)
    {
      return Languages
        .Select(language => new VendorAssortmentBulk.VendorProductDescription
        {
          DefaultVendorID = VendorID,
          CustomItemNumber = customItemNumber,
          VendorID = VendorID,
          LanguageID = language.LanguageID,
          ShortContentDescription = article.DescriptionShort,
          LongContentDescription = article.DescriptionLong,
          ShortSummaryDescription = String.Empty,
          LongSummaryDescription = String.Empty,
          ModelName = article.ArticleCode,
          ProductName = article.ArticleCode
        })
        .ToList();
    }

    private VendorAssortmentBulk.VendorAssortmentItem GetVendorAssortmentForConfigurableProduct(Article article)
    {
      return new VendorAssortmentBulk.VendorAssortmentItem
      {
        VendorProduct = GetVendorAssortmentProduct(article, true, Context),
        VendorProductDescriptions = GetVendorAssortmentProductDescriptions(article, article.ArticleCode),
        VendorImportAttributeValues = new List<VendorAssortmentBulk.VendorImportAttributeValue>
        {
          new VendorAssortmentBulk.VendorImportAttributeValue
          {
            AttributeID = PropertyStore.Collection.AttributeID,
            CustomItemNumber = article.ArticleCode,
            DefaultVendorID = VendorID,
            VendorID = VendorID,
            Value = article.Collection
          },
          new VendorAssortmentBulk.VendorImportAttributeValue
          {
            AttributeID = PropertyStore.LabelCode.AttributeID,
            CustomItemNumber = article.ArticleCode,
            DefaultVendorID = VendorID,
            VendorID = VendorID,
            Value = article.LabelCode
          },
          new VendorAssortmentBulk.VendorImportAttributeValue
          {
            AttributeID = PropertyStore.MaterialCode.AttributeID,
            CustomItemNumber = article.ArticleCode,
            DefaultVendorID = VendorID,
            VendorID = VendorID,
            Value = article.MaterialCode
          },
          new VendorAssortmentBulk.VendorImportAttributeValue
          {
            AttributeID = PropertyStore.OriginCode.AttributeID,
            CustomItemNumber = article.ArticleCode,
            DefaultVendorID = VendorID,
            VendorID = VendorID,
            Value = article.OriginCode
          },
          new VendorAssortmentBulk.VendorImportAttributeValue
          {
            AttributeID = PropertyStore.ReplenishmentMaximum.AttributeID,
            CustomItemNumber = article.ArticleCode,
            DefaultVendorID = VendorID,
            VendorID = VendorID,
            Value = article.ReplenishmentMaximum.ToString(Constants.VendorAssortmentImportCulture)
          },
          new VendorAssortmentBulk.VendorImportAttributeValue
          {
            AttributeID = PropertyStore.ReplenishmentMinimum.AttributeID,
            CustomItemNumber = article.ArticleCode,
            DefaultVendorID = VendorID,
            VendorID = VendorID,
            Value = article.ReplenishmentMinimum.ToString(Constants.VendorAssortmentImportCulture)
          },
          new VendorAssortmentBulk.VendorImportAttributeValue
          {
            AttributeID = PropertyStore.ShapeCode.AttributeID,
            CustomItemNumber = article.ArticleCode,
            DefaultVendorID = VendorID,
            VendorID = VendorID,
            Value = article.ShapeCode
          },
          new VendorAssortmentBulk.VendorImportAttributeValue
          {
            AttributeID = PropertyStore.SupplierCode.AttributeID,
            CustomItemNumber = article.ArticleCode,
            DefaultVendorID = VendorID,
            VendorID = VendorID,
            Value = article.SupplierCode
          },
          new VendorAssortmentBulk.VendorImportAttributeValue
          {
            AttributeID = PropertyStore.SupplierName.AttributeID,
            CustomItemNumber = article.ArticleCode,
            DefaultVendorID = VendorID,
            VendorID = VendorID,
            Value = article.SupplierName
          },
          new VendorAssortmentBulk.VendorImportAttributeValue
          {
            AttributeID = PropertyStore.StockType.AttributeID,
            CustomItemNumber = article.ArticleCode,
            DefaultVendorID = VendorID,
            VendorID = VendorID,
            Value = article.StockType
          },
        }
      };
    }

    private VendorAssortmentBulk.VendorAssortmentItem GetVendorAssortmentForSimpleProductWithColorLevel(Article article)
    {
      var vendorItemnumber = article.GetVendorItemNumber();
      var colorVendorItemNumber = article.GetColorVendorItemNumber();

      var simpleProduct = new VendorAssortmentBulk.VendorAssortmentItem
      {
        RelatedProducts = new List<VendorAssortmentBulk.VendorImportRelatedProduct>
        {
          new VendorAssortmentBulk.VendorImportRelatedProduct
          {
            CustomItemNumber = colorVendorItemNumber,
            DefaultVendorID = VendorID,
            IsConfigured = 1,
            RelatedCustomItemNumber = vendorItemnumber,
            RelatedProductType = Constants.Relation.Style,
            VendorID = VendorID
          }
        },
        VendorProduct = GetVendorAssortmentColorProduct(article, true, Context),
        VendorProductDescriptions = GetVendorAssortmentProductDescriptions(article, vendorItemnumber),
        VendorImportAttributeValues = new List<VendorAssortmentBulk.VendorImportAttributeValue>
        {
          new VendorAssortmentBulk.VendorImportAttributeValue
          {
            AttributeID = PropertyStore.ReferenceCode.AttributeID,
            CustomItemNumber = vendorItemnumber,
            DefaultVendorID = VendorID,
            VendorID = VendorID,
            Value = article.ReferenceCode
          }
        }
      };

      if (!article.ColorCode.Equals(Constants.IgnoreCode))
      {
        simpleProduct.VendorImportAttributeValues.Add(new VendorAssortmentBulk.VendorImportAttributeValue
        {
          AttributeID = PropertyStore.ColorCode.AttributeID,
          CustomItemNumber = vendorItemnumber,
          DefaultVendorID = VendorID,
          VendorID = VendorID,
          Value = article.ColorCode
        });

        simpleProduct.VendorImportAttributeValues.Add(new VendorAssortmentBulk.VendorImportAttributeValue
        {
          AttributeID = PropertyStore.ColorName.AttributeID,
          CustomItemNumber = vendorItemnumber,
          DefaultVendorID = VendorID,
          VendorID = VendorID,
          Value = article.ColorName
        });
      }

      if (!article.SizeCode.Equals(Constants.IgnoreCode))
      {
        simpleProduct.VendorImportAttributeValues.Add(new VendorAssortmentBulk.VendorImportAttributeValue
        {
          AttributeID = PropertyStore.SizeCode.AttributeID,
          CustomItemNumber = vendorItemnumber,
          DefaultVendorID = VendorID,
          VendorID = VendorID,
          Value = article.SizeCode
        });
      }
      return simpleProduct;
    }

    private VendorAssortmentBulk.VendorAssortmentItem GetVendorAssortmentForConfigurableProductWithColorLevel(Article article)
    {
      var colorVendorItemNumber = article.GetColorVendorItemNumber();

      var colorProduct = new VendorAssortmentBulk.VendorAssortmentItem
      {
        RelatedProducts = new List<VendorAssortmentBulk.VendorImportRelatedProduct>
        {
          new VendorAssortmentBulk.VendorImportRelatedProduct
          {
            CustomItemNumber = article.ArticleCode,
            DefaultVendorID = VendorID,
            IsConfigured = 0,
            RelatedCustomItemNumber = colorVendorItemNumber,
            RelatedProductType = Constants.Relation.Color,
            VendorID = VendorID
          }
        },
        VendorProduct = GetVendorAssortmentProduct(article, false, Context),
        VendorProductDescriptions = GetVendorAssortmentProductDescriptions(article, colorVendorItemNumber),
        VendorImportAttributeValues = new List<VendorAssortmentBulk.VendorImportAttributeValue>
        {
          new VendorAssortmentBulk.VendorImportAttributeValue
          {
            AttributeID = PropertyStore.ReferenceCode.AttributeID,
            CustomItemNumber = colorVendorItemNumber,
            DefaultVendorID = VendorID,
            VendorID = VendorID,
            Value = article.ReferenceCode
          }
        }
      };

      if (!article.ColorCode.Equals(Constants.IgnoreCode))
      {
        colorProduct.VendorImportAttributeValues.Add(new VendorAssortmentBulk.VendorImportAttributeValue
        {
          AttributeID = PropertyStore.ColorCode.AttributeID,
          CustomItemNumber = colorVendorItemNumber,
          DefaultVendorID = VendorID,
          VendorID = VendorID,
          Value = article.ColorCode
        });

        colorProduct.VendorImportAttributeValues.Add(new VendorAssortmentBulk.VendorImportAttributeValue
        {
          AttributeID = PropertyStore.ColorName.AttributeID,
          CustomItemNumber = colorVendorItemNumber,
          DefaultVendorID = VendorID,
          VendorID = VendorID,
          Value = article.ColorName
        });
      }

      //if (!article.SizeCode.Equals(Constants.IgnoreCode))
      //{
      //  colorProduct.VendorImportAttributeValues.Add(new VendorAssortmentBulk.VendorImportAttributeValue
      //  {
      //    AttributeID = PropertyStore.SizeCode.AttributeID,
      //    CustomItemNumber = colorVendorItemNumber,
      //    DefaultVendorID = VendorID,
      //    VendorID = VendorID,
      //    Value = article.SizeCode
      //  });
      //}

      return colorProduct;
    }

    private VendorAssortmentBulk.VendorAssortmentItem GetVendorAssortmentForSimpleProduct(Article article)
    {
      var vendorItemNumber = article.GetVendorItemNumber();

      var simpleProduct = new VendorAssortmentBulk.VendorAssortmentItem
      {
        RelatedProducts = new List<VendorAssortmentBulk.VendorImportRelatedProduct>
        {
          new VendorAssortmentBulk.VendorImportRelatedProduct
          {
            CustomItemNumber = article.ArticleCode,
            DefaultVendorID = VendorID,
            IsConfigured = 1,
            RelatedCustomItemNumber = vendorItemNumber,
            RelatedProductType = Constants.Relation.Style,
            VendorID = VendorID
          }
        },
        VendorProduct = GetVendorAssortmentProduct(article, false, Context),
        VendorProductDescriptions = GetVendorAssortmentProductDescriptions(article, vendorItemNumber),
        VendorImportAttributeValues = new List<VendorAssortmentBulk.VendorImportAttributeValue>
        {
          new VendorAssortmentBulk.VendorImportAttributeValue
          {
            AttributeID = PropertyStore.ReferenceCode.AttributeID,
            CustomItemNumber = vendorItemNumber,
            DefaultVendorID = VendorID,
            VendorID = VendorID,
            Value = article.ReferenceCode
          }
        }
      };

      if (!article.ColorCode.Equals(Constants.IgnoreCode))
      {
        simpleProduct.VendorImportAttributeValues.Add(new VendorAssortmentBulk.VendorImportAttributeValue
        {
          AttributeID = PropertyStore.ColorCode.AttributeID,
          CustomItemNumber = vendorItemNumber,
          DefaultVendorID = VendorID,
          VendorID = VendorID,
          Value = article.ColorCode
        });

        simpleProduct.VendorImportAttributeValues.Add(new VendorAssortmentBulk.VendorImportAttributeValue
        {
          AttributeID = PropertyStore.ColorName.AttributeID,
          CustomItemNumber = vendorItemNumber,
          DefaultVendorID = VendorID,
          VendorID = VendorID,
          Value = article.ColorName
        });
      }

      if (!article.SizeCode.Equals(Constants.IgnoreCode))
      {
        simpleProduct.VendorImportAttributeValues.Add(new VendorAssortmentBulk.VendorImportAttributeValue
        {
          AttributeID = PropertyStore.SizeCode.AttributeID,
          CustomItemNumber = vendorItemNumber,
          DefaultVendorID = VendorID,
          VendorID = VendorID,
          Value = article.SizeCode
        });
      }

      return simpleProduct;
    }

    private VendorAssortmentBulk.VendorAssortmentItem GetVendorAssortmentForCommercialProduct(Article article)
    {
      var vendorItemNumber = article.GetVendorItemNumber();
      var tariffVendor = TariffVendors[GetTariffCode(article.CountryCode, article.CurrencyCode)];

      return new VendorAssortmentBulk.VendorAssortmentItem
      {
        VendorProduct = GetVendorAssortmentProduct(article, false, tariffVendor),
        VendorImportPrices = new List<VendorAssortmentBulk.VendorImportPrice>
        {
          new VendorAssortmentBulk.VendorImportPrice
          {
            CustomItemNumber = vendorItemNumber,
            CommercialStatus = Constants.Status.Default,
            DefaultVendorID = VendorID,
            VendorID = tariffVendor.VendorID,
            CostPrice = article.CostPrice.ToString(Constants.VendorAssortmentImportCulture),
            Price = article.Price.ToString(Constants.VendorAssortmentImportCulture),
            TaxRate = article.Tax.ToString(Constants.VendorAssortmentImportCulture)
          }
        }
      };
    }

    private VendorAssortmentBulk.VendorAssortmentItem GetVendorAssortmentForCommercialColorProduct(Article article)
    {
      var colorVendorItemNumber = article.GetColorVendorItemNumber();
      var tariffVendor = TariffVendors[GetTariffCode(article.CountryCode, article.CurrencyCode)];

      return new VendorAssortmentBulk.VendorAssortmentItem
      {
        VendorProduct = GetVendorAssortmentColorProduct(article, true, tariffVendor),
        VendorImportPrices = new List<VendorAssortmentBulk.VendorImportPrice>
        {
          new VendorAssortmentBulk.VendorImportPrice
          {
            CustomItemNumber = colorVendorItemNumber,
            CommercialStatus = Constants.Status.Default,
            DefaultVendorID = VendorID,
            VendorID = tariffVendor.VendorID,
            CostPrice = article.CostPrice.ToString(Constants.VendorAssortmentImportCulture),
            Price = article.Price.ToString(Constants.VendorAssortmentImportCulture),
            TaxRate = article.Tax.ToString(Constants.VendorAssortmentImportCulture)
          }
        }
      };
    }

    [Resource]
    private static readonly String MergeProductConfiguration = null;

    private void ImportProductConfiguration(params ProductAttributeMetaData[] productAttributes)
    {
      TraceInformation("Importing product configurations...");

      foreach (var productAttribute in productAttributes)
      {
        var count = Database.Execute(MergeProductConfiguration, productAttribute.AttributeID);

        if (count > 0)
        {
          TraceVerbose("{0} changes made for attribute '{1}'.", count, productAttribute.AttributeCode);
        }
      }
    }

    private void ImportProductGroups()
    {
      TraceInformation("Importing new product groups...");

      var existingProductGroupLanguages = Unit.Scope
        .Repository<ProductGroupLanguage>()
        .Include(productGroupLanguage => productGroupLanguage.Language)
        .Include(productGroupLanguage => productGroupLanguage.ProductGroup)
        .GetAll(productGroupLanguage => Constants.Language.English.Equals(productGroupLanguage.Language.Name))
        .ToList();

      var unmatchedProductGroupVendors = Unit.Scope
        .Repository<ProductGroupVendor>()
        .GetAll(productGroupVendor => productGroupVendor.ProductGroupID == Constants.ProductGroup.UnknownID)
        .ToArray();

      foreach (var unmatchedProductGroupVendor in unmatchedProductGroupVendors)
      {
        var vendorName = unmatchedProductGroupVendor.VendorName.Trim();

        TraceInformation("Importing '{0}'...", vendorName);

        var existingProductGroupLanguage = existingProductGroupLanguages.FirstOrDefault(productGroupLanguage => productGroupLanguage.Name.Equals(vendorName));

        if (existingProductGroupLanguage == null)
        {
          var productGroup = new ProductGroup
          {
            ProductGroupLanguages = new List<ProductGroupLanguage>(),
            Score = 0
          };

          var productGroupLanguage = new ProductGroupLanguage
          {
            Language = Languages.Single(language => Constants.Language.English.Equals(language.Name)),
            Name = vendorName,
            ProductGroup = productGroup
          };

          productGroup.ProductGroupLanguages.Add(productGroupLanguage);
          existingProductGroupLanguages.Add(productGroupLanguage);

          unmatchedProductGroupVendor.ProductGroup = productGroup;
        }
        else
        {
          unmatchedProductGroupVendor.ProductGroup = existingProductGroupLanguage.ProductGroup;
        }
      }

      Unit.Save();
    }

    private void ImportTariffVendors(IEnumerable<Article> articles)
    {
      TraceInformation("Importing new tariff vendors...");

      var vendorUserRoles = Context.UserRoles.ToArray();

      foreach (var tariff in articles.Select(article => new { article.CountryCode, article.CurrencyCode }).Distinct())
      {
        var tariffCode = GetTariffCode(tariff.CountryCode, tariff.CurrencyCode);
        var tariffVendor = default(Vendor);

        if (!TariffVendors.TryGetValue(tariffCode, out tariffVendor))
        {
          var tariffVendorName = String.Join(Constants.CountrySymbol, VendorName, tariffCode);

          TraceInformation("Creating new tariff vendor '{0}'...", tariffVendorName);

          tariffVendor = new Vendor
          {
            Description = tariffVendorName,
            Name = tariffVendorName,
            IsActive = true,
            ParentVendorID = VendorID,
            UserRoles = vendorUserRoles
              .Select(vendorUserRole => new UserRole
              {
                RoleID = vendorUserRole.RoleID,
                UserID = vendorUserRole.UserID
              })
              .ToList(),
            VendorSettings = new List<VendorSetting>
            {
              new VendorSetting
              {
                SettingKey = Constants.Vendor.Setting.CountryCode,
                Value = tariff.CountryCode
              },
              new VendorSetting
              {
                SettingKey = Constants.Vendor.Setting.CurrencyCode,
                Value = tariff.CurrencyCode
              },
              new VendorSetting
              {
                SettingKey = Constants.Vendor.Setting.IsTariff,
                Value = Boolean.TrueString
              }
            },
            VendorType = (Int32)VendorType.HasFinancialProcess
          };

          Unit.Scope.Repository<Vendor>().Add(tariffVendor);
          Unit.Save();

          TariffVendors[tariffCode] = tariffVendor;
        }
      }
    }

    protected override Boolean ValidateContext()
    {
      return Context.Name.Equals(Constants.Vendor.Vlisco, StringComparison.OrdinalIgnoreCase);
    }
  }
}