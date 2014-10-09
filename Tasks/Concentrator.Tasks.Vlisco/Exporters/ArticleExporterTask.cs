using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using CsvHelper;

namespace Concentrator.Tasks.Vlisco.Exporters
{
  using Common;
  using Models;
  using Objects.DataAccess.Repository;
  using Objects.Enumerations;
  using Objects.Models.Attributes;
  using Objects.Models.Contents;
  using Objects.Models.Connectors;
  using Objects.Models.Vendors;
  using Objects.Monitoring;
  using Objects.Sql;

  [Task(Constants.Vendor.Vlisco + " Article Exporter Task")]
  public class ArticleExporterTask : ConnectorTaskBase
  {
    private readonly FeeblMonitoring Monitoring = new FeeblMonitoring();

    private class ArticleAttributeModel
    {
      public Int32 ProductID
      {
        get;
        set;
      }

      public String AttributeCode
      {
        get;
        set;
      }

      public Int32 AttributeID
      {
        get;
        set;
      }

      public String AttributeValue
      {
        get;
        set;
      }
    }

    private class ArticleBarcodeModel
    {
      public String Barcode
      {
        get;
        set;
      }

      public Int32 ProductID
      {
        get;
        set;
      }
    }

    private class ArticleDescriptionModel
    {
      public String DescriptionLong
      {
        get;
        set;
      }

      public String DescriptionShort
      {
        get;
        set;
      }

      public Int32 ProductID
      {
        get;
        set;
      }
    }

    private class ArticleCodeModel
    {
      public Int32 BrandID
      {
        get;
        set;
      }

      public String ArticleCode
      {
        get;
        set;
      }

      public Int32 ParentProductID
      {
        get;
        set;
      }

      public Int32 ProductID
      {
        get;
        set;
      }
    }

    private class ArticleGroupModel
    {
      public Int32 CategoryID
      {
        get;
        set;
      }

      public String CategoryCode
      {
        get;
        set;
      }

      public String CategoryName
      {
        get;
        set;
      }

      public Int32 FamilyID
      {
        get;
        set;
      }

      public String FamilyCode
      {
        get;
        set;
      }

      public String FamilyName
      {
        get;
        set;
      }

      public Int32 SubfamilyID
      {
        get;
        set;
      }

      public String SubfamilyCode
      {
        get;
        set;
      }

      public String SubfamilyName
      {
        get;
        set;
      }

      public Int32 ProductID
      {
        get;
        set;
      }
    }

    private class ArticleTariffModel
    {
      public Int32 ProductID
      {
        get;
        set;
      }

      public Int32 VendorID
      {
        get;
        set;
      }

      public Decimal CostPrice
      {
        get;
        set;
      }

      public Decimal UnitPrice
      {
        get;
        set;
      }

      public Decimal TaxRate
      {
        get;
        set;
      }

      public String CountryCode
      {
        get;
        set;
      }

      public String CurrencyCode
      {
        get;
        set;
      }
    }

    private class ContentVendorSettingComparer : IComparer<ContentVendorSetting>, IEqualityComparer<ContentVendorSetting>
    {
      public static readonly ContentVendorSettingComparer Default = new ContentVendorSettingComparer();

      public Int32 Compare(ContentVendorSetting left, ContentVendorSetting right)
      {
        return left.ContentVendorIndex.CompareTo(right.ContentVendorIndex);
      }

      public Boolean Equals(ContentVendorSetting left, ContentVendorSetting right)
      {
        return left.BrandID == right.BrandID
          && left.ProductGroupID == right.ProductGroupID
          && left.ProductID == right.ProductID
          && left.VendorID == right.VendorID;
      }

      public Int32 GetHashCode(ContentVendorSetting contextVendorSetting)
      {
        return contextVendorSetting.BrandID.GetHashCode()
          ^ contextVendorSetting.ProductGroupID.GetHashCode()
          ^ contextVendorSetting.ProductID.GetHashCode()
          ^ contextVendorSetting.VendorID;
      }
    }

    private class VendorAssortmentComparer : IEqualityComparer<VendorAssortment>
    {
      public static readonly VendorAssortmentComparer Default = new VendorAssortmentComparer();

      public Boolean Equals(VendorAssortment left, VendorAssortment right)
      {
        return left.VendorAssortmentID == right.VendorAssortmentID;
      }

      public Int32 GetHashCode(VendorAssortment vendorAssortment)
      {
        return vendorAssortment.VendorAssortmentID;
      }
    }

    private const String IntegerTableTypeName = "[dbo].[IntegerTable]";

    [Resource]
    private static readonly String ArticleAttributeQuery = null;

    [Resource]
    private static readonly String ArticleBarcodeQuery = null;

    [Resource]
    private static readonly String ArticleCodeQuery = null;

    [Resource]
    private static readonly String ArticleDescriptionQuery = null;

    [Resource]
    private static readonly String ArticleGroupQuery = null;

    [Resource]
    private static readonly String ArticleTariffQuery = null;

    [Resource]
    private static readonly String GetAttributeValue = null;

    protected override IRepository<Connector> ContextRepository
    {
      get
      {
        return base.ContextRepository
          .Include(connector => connector.ContentPrices)
          .Include(connector => connector.ContentVendorSettings)
          .Include(connector => connector.ParentConnector);
      }
    }

    [ConnectorSetting(Constants.Connector.Setting.Destination, true)]
    private String Destination
    {
      get;
      set;
    }

    [ConnectorSetting(Constants.Connector.Setting.MultiMagCode, true)]
    private String MultiMagCode
    {
      get;
      set;
    }

    private IEnumerable<ContentPrice> PriceRules
    {
      get;
      set;
    }

    protected override void ExecuteConnectorTask()
    {
      Monitoring.Notify(Name, 0);

      var destination = new DirectoryInfo(Destination);

      if (!destination.FullName.EndsWith("$") && !destination.Exists)
      {
        TraceError("{0}: The directory '{1}' does not exists!", Context.Name, destination.FullName);
      }
      else if (!destination.FullName.EndsWith("$") && !destination.HasAccess(FileSystemRights.FullControl))
      {
        TraceError("{0}: The user '{1}' has insufficient access over the directory '{2}'!"
          , Context.Name
          , WindowsIdentity.GetCurrent().Name
          , destination.FullName);
      }
      else
      {
        PriceRules = Context.ContentPrices
          .OrderByDescending(priceRule => priceRule.ContentPriceRuleIndex)
          .ToArray();

        var differentialProvider = new DifferentialProvider(Path.Combine(Constants.Directories.History, Context.Name));
        var articles = GetArticles()
          .Select(UpdateArticlePricing)
          .ToArray();

        articles = differentialProvider
          .GetDifferential(articles, ArticleCommercialComparer.Default)
          .ToArray();

        if (articles.Any())
        {
          var articleFile = Path.Combine(destination.FullName, String.Format("{0}_{1:yyyyMMddHHmmss}.csv", MultiMagCode, DateTime.Now));

          TraceVerbose("{0}: Writing {1} articles to file '{2}'...", Context.Name, articles.Length, Path.GetFileName(articleFile));

          using (var streamWriter = new StreamWriter(articleFile))
          using (var csvWriter = new CsvWriter(streamWriter))
          {
            csvWriter.Configuration.Delimiter = ";";
            csvWriter.Configuration.RegisterClassMap<ExportArticleMapping>();
            csvWriter.WriteHeader<Article>();
            csvWriter.WriteRecords(articles);

            TraceInformation("{0}: {1} articles written to file '{2}'!", Context.Name, articles.Length, Path.GetFileName(articleFile));
          }
        }
        else
        {
          TraceWarning("{0}: There are no articles generated for export...", Context.Name);
        }
      }

      Monitoring.Notify(Name, 1);
    }

    private IDictionary<Int32, IDictionary<String, String>> GetArticleAttributes(IEnumerable<VendorAssortment> vendorAssortment, IEnumerable<ContentVendorSetting> vendorSettings)
    {
      var result = new Dictionary<Int32, IDictionary<String, String>>();

      using (var productTable = vendorAssortment
        .Select(vendorAssortmentItem => vendorAssortmentItem.ProductID)
        .Distinct()
        .ToDataTable())
      {

        foreach (var vendorSetting in vendorSettings)
        {
          foreach (var articleAttribute in Database.Query<ArticleAttributeModel>(ArticleAttributeQuery
            , productTable.AsParameter(IntegerTableTypeName)
            , vendorSetting.VendorID))
          {
            IDictionary<String, String> attributeStore;

            if (!result.TryGetValue(articleAttribute.ProductID, out attributeStore))
            {
              result[articleAttribute.ProductID] = attributeStore = new Dictionary<String, String>();
            }

            if (!attributeStore.ContainsKey(articleAttribute.AttributeCode))
            {
              attributeStore[articleAttribute.AttributeCode] = articleAttribute.AttributeValue;
            }
          }
        }
      }

      return result;
    }

    private IDictionary<Int32, String> GetArticleBarcodes(IEnumerable<VendorAssortment> vendorAssortment, IEnumerable<ContentVendorSetting> vendorSettings)
    {
      var result = new Dictionary<Int32, String>();

      using (var productTable = vendorAssortment
        .Select(vendorAssortmentItem => vendorAssortmentItem.ProductID)
        .Distinct()
        .ToDataTable())
      {
        foreach (var vendorSetting in vendorSettings)
        {
          foreach (var articleBarcode in Database.Query<ArticleBarcodeModel>(ArticleBarcodeQuery
            , productTable.AsParameter(IntegerTableTypeName)
            , vendorSetting.VendorID))
          {
            if (!result.ContainsKey(articleBarcode.ProductID))
            {
              result[articleBarcode.ProductID] = articleBarcode.Barcode;
            }
          }
        }
      }

      return result;
    }

    private IDictionary<Int32, ArticleCodeModel> GetArticleCodes(IEnumerable<VendorAssortment> vendorAssortment, IEnumerable<ContentVendorSetting> vendorSettings)
    {
      var result = new Dictionary<Int32, ArticleCodeModel>();

      using (var productTable = vendorAssortment
        .Select(vendorAssortmentItem => vendorAssortmentItem.ProductID)
        .Distinct()
        .ToDataTable())
      {
        foreach (var vendorSetting in vendorSettings)
        {
          foreach (var articleCode in Database.Query<ArticleCodeModel>(ArticleCodeQuery
            , productTable.AsParameter(IntegerTableTypeName)
            , vendorSetting.VendorID))
          {
            if (!result.ContainsKey(articleCode.ParentProductID))
            {
              result[articleCode.ParentProductID] = articleCode;
            }
            if (!result.ContainsKey(articleCode.ProductID))
            {
              result[articleCode.ProductID] = articleCode;
            }
          }
        }
      }

      return result;
    }

    private ArticleDescriptionModel[] GetArticleDescriptions(IEnumerable<VendorAssortment> vendorAssortment, IEnumerable<ContentVendorSetting> vendorSettings)
    {
      var result = new Dictionary<Int32, ArticleDescriptionModel>();

      using (var productTable = vendorAssortment
        .Select(vendorAssortmentItem => vendorAssortmentItem.ProductID)
        .Distinct()
        .ToDataTable())
      {
        foreach (var vendorSetting in vendorSettings)
        {
          foreach (var articleDescription in Database.Query<ArticleDescriptionModel>(ArticleDescriptionQuery
            , productTable.AsParameter(IntegerTableTypeName)
            , vendorSetting.VendorID))
          {
            if (!result.ContainsKey(articleDescription.ProductID))
            {
              result[articleDescription.ProductID] = articleDescription;
            }
          }
        }
      }

      return result.Values.ToArray();
    }

    private IEnumerable<ArticleGroupModel> GetArticleGroups(IEnumerable<VendorAssortment> vendorAssortment, IEnumerable<ContentVendorSetting> vendorSettings)
    {
      var result = new Dictionary<Int32, ArticleGroupModel>();

      using (var productTable = vendorAssortment
        .Select(vendorAssortmentItem => vendorAssortmentItem.ProductID)
        .Distinct()
        .ToDataTable())
      {
        foreach (var vendorSetting in vendorSettings)
        {
          foreach (var articleGroup in Database.Query<ArticleGroupModel>(ArticleGroupQuery
            , productTable.AsParameter(IntegerTableTypeName)
            , vendorSetting.VendorID))
          {
            if (!result.ContainsKey(articleGroup.ProductID))
            {
              result[articleGroup.ProductID] = articleGroup;
            }
          }
        }
      }

      return result.Values.ToArray();
    }

    private IEnumerable<ArticleTariffModel> GetArticleTariffs(IEnumerable<VendorAssortment> vendorAssortment)
    {
      using (var vendorAssortmentTable = vendorAssortment
        .Select(vendorAssortmentItem => vendorAssortmentItem.VendorAssortmentID)
        .ToDataTable())
      {
        return Database
         .Query<ArticleTariffModel>(ArticleTariffQuery
           , vendorAssortmentTable.AsParameter(IntegerTableTypeName)
            , Constants.Vendor.Setting.CountryCode
            , Constants.Vendor.Setting.CurrencyCode)
         .ToArray();
      }
    }

    private IEnumerable<Article> GetArticles()
    {
      var vendorAssortment = GetVendorAssortment().ToArray();
      var vendorSettings = GetVendorSettings().ToArray();

      if (!vendorSettings.Any())
      {
        TraceError("{0}: There are no connector vendor settings defined for the connector or for any of its ancestors!", Context.Name);
      }
      else
      {
        TraceVerbose("{0}: Loading content for vendor assortment...", Context.Name);

        var defaultGroup = new ArticleGroupModel
        {
          CategoryCode = Constants.DiversCode,
          CategoryID = Constants.ProductGroup.UnknownID,
          CategoryName = Constants.DiversName,
          FamilyCode = Constants.DiversCode,
          FamilyID = Constants.ProductGroup.UnknownID,
          FamilyName = Constants.DiversName,
          SubfamilyCode = Constants.DiversCode,
          SubfamilyID = Constants.ProductGroup.UnknownID,
          SubfamilyName = Constants.DiversName
        };

        var articleAttributes = GetArticleAttributes(vendorAssortment, vendorSettings);
        var articleBarcodes = GetArticleBarcodes(vendorAssortment, vendorSettings);
        var articleCodes = GetArticleCodes(vendorAssortment, vendorSettings);
        var articleDescriptions = GetArticleDescriptions(vendorAssortment, vendorSettings);
        var articleLongDescriptions = articleDescriptions.ToDictionary(articleDescription => articleDescription.ProductID, articleDescription => articleDescription.DescriptionLong);
        var articleShortDescriptions = articleDescriptions.ToDictionary(articleDescription => articleDescription.ProductID, articleDescription => articleDescription.DescriptionShort);
        var articleGroups = GetArticleGroups(vendorAssortment, vendorSettings).ToDictionary(articleGroup => articleGroup.ProductID);
        var articleTariffs = GetArticleTariffs(vendorAssortment);

        var articles = articleTariffs.Select(articleTariff => new Article
        {
          ProductID = articleTariff.ProductID,
          ArticleID = articleCodes[articleTariff.ProductID].ParentProductID,
          ArticleCode = articleCodes[articleTariff.ProductID].ArticleCode,
          Barcode = articleBarcodes.GetValueOrDefault(articleTariff.ProductID, String.Empty),
          BrandID = articleCodes[articleTariff.ProductID].BrandID,
          CategoryID = articleGroups.GetValueOrDefault(articleTariff.ProductID, defaultGroup).CategoryID,
          CategoryCode = articleGroups.GetValueOrDefault(articleTariff.ProductID, defaultGroup).CategoryCode,
          CategoryName = articleGroups.GetValueOrDefault(articleTariff.ProductID, defaultGroup).CategoryName,
          Collection = articleAttributes[articleTariff.ProductID].GetValueOrDefault(Constants.Attribute.Collection),
          ColorCode = articleAttributes[articleTariff.ProductID].GetValueOrDefault(Constants.Attribute.ColorCode, Constants.IgnoreCode),
          ColorName = articleAttributes[articleTariff.ProductID].GetValueOrDefault(Constants.Attribute.ColorName, Constants.IgnoreCode),
          CostPrice = articleTariff.CostPrice,
          CountryCode = articleTariff.CountryCode,
          CurrencyCode = articleTariff.CurrencyCode,
          DescriptionLong = articleLongDescriptions.GetValueOrDefault(articleTariff.ProductID, String.Empty),
          DescriptionShort = articleShortDescriptions.GetValueOrDefault(articleTariff.ProductID, String.Empty),
          FamilyID = articleGroups.GetValueOrDefault(articleTariff.ProductID, defaultGroup).FamilyID,
          FamilyCode = articleGroups.GetValueOrDefault(articleTariff.ProductID, defaultGroup).FamilyCode,
          FamilyName = articleGroups.GetValueOrDefault(articleTariff.ProductID, defaultGroup).FamilyName,
          LabelCode = articleAttributes[articleTariff.ProductID].GetValueOrDefault(Constants.Attribute.LabelCode, String.Empty),
          MaterialCode = articleAttributes[articleTariff.ProductID].GetValueOrDefault(Constants.Attribute.MaterialCode, String.Empty),
          OriginCode = articleAttributes[articleTariff.ProductID].GetValueOrDefault(Constants.Attribute.OriginCode, String.Empty),
          Price = articleTariff.UnitPrice,
          ReferenceCode = articleAttributes[articleTariff.ProductID].GetValueOrDefault(Constants.Attribute.ReferenceCode, String.Empty),
          ReplenishmentMaximum = articleAttributes[articleTariff.ProductID].GetValueOrDefault(Constants.Attribute.ReplenishmentMaximum, "0").ToInt().GetValueOrDefault(),
          ReplenishmentMinimum = articleAttributes[articleTariff.ProductID].GetValueOrDefault(Constants.Attribute.ReplenishmentMinimum, "0").ToInt().GetValueOrDefault(),
          ShapeCode = articleAttributes[articleTariff.ProductID].GetValueOrDefault(Constants.Attribute.ShapeCode, String.Empty),
          SizeCode = articleAttributes[articleTariff.ProductID].GetValueOrDefault(Constants.Attribute.SizeCode, Constants.IgnoreCode),
          StockType = articleAttributes[articleTariff.ProductID].GetValueOrDefault(Constants.Attribute.StockType, String.Empty),
          SubfamilyID = articleGroups.GetValueOrDefault(articleTariff.ProductID, defaultGroup).SubfamilyID,
          SubfamilyCode = articleGroups.GetValueOrDefault(articleTariff.ProductID, defaultGroup).SubfamilyCode,
          SubfamilyName = articleGroups.GetValueOrDefault(articleTariff.ProductID, defaultGroup).SubfamilyName,
          SupplierCode = articleAttributes[articleTariff.ProductID].GetValueOrDefault(Constants.Attribute.SupplierCode),
          SupplierName = articleAttributes[articleTariff.ProductID].GetValueOrDefault(Constants.Attribute.SupplierName),
          Tax = articleTariff.TaxRate,
          VendorID = articleTariff.VendorID,
          ZoneCode4 = Constants.MissingCode,
          ZoneCode5 = Constants.MissingCode
        });

        return articles;
      }

      return null;
    }

    private IEnumerable<VendorAssortment> GetVendorAssortment()
    {
      TraceVerbose("{0}: Loading vendor assortment items...", Context.Name);

      var query = new QueryBuilder()
        .From("[dbo].[ConnectorPublicationRule]")
        .Where("[ConnectorID] = @ConnectorID")
        .Where("[IsActive] = 1")
        .Where("[FromDate] IS NULL OR [FromDate] <= GetDate()")
        .Where("[ToDate] IS NULL OR [ToDate] > GetDate()")
        .Select("*")
        .OrderBy("[PublicationIndex]", SortingDirection.Descending);

      var connectorPublicationRules = Database
        .Query<ConnectorPublicationRule>(query, Context)
        .ToArray();

      var vendorAssortmentResult = new VendorAssortment[0];

      foreach (var connectorPublicationRule in connectorPublicationRules)
      {
        var assortment = GetVendorAssortmentForConnectorPublicationRule(connectorPublicationRule).ToArray();

        if (assortment.Any())
        {
          switch ((ConnectorPublicationRuleType)connectorPublicationRule.PublicationType)
          {
            case ConnectorPublicationRuleType.Exclude:
              vendorAssortmentResult = vendorAssortmentResult
                .Except(assortment, VendorAssortmentComparer.Default)
                .ToArray();
              break;

            case ConnectorPublicationRuleType.Include:
              vendorAssortmentResult = vendorAssortmentResult
                .Concat(assortment)
                .Distinct(VendorAssortmentComparer.Default)
                .ToArray();
              break;
          }
        }
      }

      TraceVerbose("{0}: {1} vendor assortment items found!", Context.Name, vendorAssortmentResult.Length);

      return vendorAssortmentResult;
    }

    private IEnumerable<VendorAssortment> GetVendorAssortmentForConnectorPublicationRule(ConnectorPublicationRule connectorPublicationRule)
    {
      var queryBuilder = new QueryBuilder()
        .From("[dbo].[VendorAssortment] AS [VA]")
        .Where("[VA].[VendorID] = @VendorID")
        .Where("[VA].[IsActive] = 1");

      if (connectorPublicationRule.ProductID.HasValue)
      {
        queryBuilder.Where("[VA].[ProductID] = @ProductID");
      }

      if (connectorPublicationRule.BrandID.HasValue)
      {
        queryBuilder
          .Join(JoinType.Inner, "[dbo].[Product] AS [P]", "[VA].[ProductID] = [P].[ProductID]")
          .Where("[P].[BrandID] = @BrandID");
      }

      if (connectorPublicationRule.MasterGroupMappingID.HasValue && connectorPublicationRule.MasterGroupMappingID.Value > 0)
      {
        queryBuilder
          .Join(JoinType.Inner, "[dbo].[MasterGroupMappingProduct] AS [MGMP]", "[MGMP].[MasterGroupMappingID] = @MasterGroupMappingID AND [MGMP].[ProductID] = [VA].[ProductID]")
          .Where("MGMP.[IsProductMapped] = 1");

        if (connectorPublicationRule.OnlyApprovedProducts)
        {
          queryBuilder
            .Join(JoinType.Inner, "[dbo].[MasterGroupMapping] AS [MGM]", "[MGMP].[MasterGroupMappingID] = [MGM].[MasterGroupMappingID]")
            .Where("[MGMP].[IsApproved] = 1")
            .Where("[MGM].[ConnectorID] IS NULL");
        }
      }

      if (connectorPublicationRule.PublishOnlyStock.GetValueOrDefault())
      {
        queryBuilder
          .Join(JoinType.Inner, "[dbo].[VendorStock] AS [VS]", "[VA].[ProductID] = VS.[ProductID] AND [VA].[VendorID] = [VS].[VendorID]")
          .Where("[VS].[QuantityOnHand] > 0");
      }

      if (connectorPublicationRule.StatusID.HasValue || connectorPublicationRule.FromPrice.HasValue || connectorPublicationRule.ToPrice.HasValue)
      {
        queryBuilder.Join(JoinType.Inner, "[dbo].[VendorPrice] AS [VP]", "[VA].[VendorAssortmentID] = [VP].[VendorAssortmentID]");

        if (connectorPublicationRule.StatusID.HasValue)
        {
          queryBuilder.Where("[VP].[ConcentratorStatusID] = @StatusID");
        }

        if (connectorPublicationRule.FromPrice.HasValue)
        {
          queryBuilder.Where("[VP].[Price] >= @FromPrice");
        }

        if (connectorPublicationRule.FromPrice.HasValue)
        {
          queryBuilder.Where("[VP].[Price] < @ToPrice");
        }
      }

      if (connectorPublicationRule.AttributeID.HasValue || !String.IsNullOrEmpty(connectorPublicationRule.AttributeValue))
      {
        queryBuilder
          .Join(JoinType.Inner, "[dbo].[ProductAttributeValue] AS [PAV]", "[VA].[ProductID] = [PAV].[ProductID] AND [VA].[VendorID] = [PAV].[VendorID]")
          .Where("[PAV].[AttributeID] = @AttributeID")
          .Where("[PAV].[Value] = @AttributeValue");
      }

      return Database
        .Query<VendorAssortment>(queryBuilder.Select("DISTINCT [VA].*"), connectorPublicationRule)
        .ToArray();
    }

    private IEnumerable<ContentVendorSetting> GetVendorSettings()
    {
      TraceVerbose("{0}: Loading connector vendor settings...", Context.Name);

      var settings = new List<ContentVendorSetting>(Context.ContentVendorSettings);

      for (var currentConnector = Context.ParentConnector; currentConnector != null; currentConnector = currentConnector.ParentConnector)
      {
        settings.AddRange(currentConnector.ContentVendorSettings);
      }

      return settings.Distinct(ContentVendorSettingComparer.Default).OrderBy(vendorSetting => vendorSetting.ContentVendorIndex).ToArray();
    }

    private Boolean CheckPriceRuleForArticle(Article article, ContentPrice priceRule)
    {
      var result = priceRule.VendorID == article.VendorID;

      if (result && priceRule.ProductID.HasValue && priceRule.ProductID != article.ArticleID && priceRule.ProductID != article.ProductID)
      {
        result = false;
      }

      if (result && priceRule.BrandID.HasValue && priceRule.BrandID != article.BrandID)
      {
        result = false;
      }

      if (result && priceRule.AttributeID.HasValue && !priceRule.AttributeValue.IsNullOrWhiteSpace())
      {
        var attributeValues = Database
          .Query<ProductAttributeValue>(GetAttributeValue, priceRule.AttributeID.Value, article.ArticleID, article.ProductID)
          .Where(attributeValue => !attributeValue.LanguageID.HasValue)
          .ToDictionary(attributeValue => attributeValue.ProductID, attributeValue => attributeValue.Value);

        // First check the value on the lowest product level (SKU), then go upwards in the hierarchy (Color or Style)
        if (attributeValues.GetValueOrDefault(article.ProductID) != priceRule.AttributeValue && attributeValues.GetValueOrDefault(article.ArticleID) != priceRule.AttributeValue)
        {
          result = false;
        }
      }

      if (result
        && priceRule.ProductGroupID.HasValue
        && priceRule.ProductGroupID != article.CategoryID
        && priceRule.ProductGroupID != article.FamilyID
        && priceRule.ProductGroupID != article.SubfamilyID)
      {
        result = false;
      }

      return result;
    }

    private Article UpdateArticlePricing(Article article)
    {
      var originalCostPrice = article.CostPrice;
      var originalUnitPrice = article.Price;

      foreach (var priceRule in PriceRules)
      {
        var costPrice = originalCostPrice;
        var unitPrice = originalUnitPrice;

        if (CheckPriceRuleForArticle(article, priceRule))
        {
          switch ((PriceRuleType)priceRule.PriceRuleType)
          {
            case PriceRuleType.CostPrice:
              if (priceRule.FixedPrice.HasValue)
              {
                costPrice = priceRule.FixedPrice.Value;
              }

              switch (priceRule.Margin.FirstOrDefault())
              {
                case '%':
                  article.CostPrice = priceRule.CostPriceIncrease.GetValueOrDefault(Decimal.Zero) * costPrice;
                  break;

                case '-':
                  article.CostPrice = priceRule.CostPriceIncrease.GetValueOrDefault(Decimal.Zero) - costPrice;
                  break;

                case '+':
                  article.CostPrice = priceRule.CostPriceIncrease.GetValueOrDefault(Decimal.Zero) + costPrice;
                  break;

                default:
                  article.CostPrice = costPrice;
                  break;
              }
              break;

            case PriceRuleType.UnitPrice:
              if (priceRule.FixedPrice.HasValue)
              {
                unitPrice = priceRule.FixedPrice.Value;
              }

              article.ApplySaleLogic(article, priceRule); //Fills fields in the article according to the pricerule.

              switch (priceRule.Margin.FirstOrDefault())
              {
                case '%':
                  article.Price = priceRule.UnitPriceIncrease.GetValueOrDefault(Decimal.Zero) * unitPrice;
                  break;

                case '-':
                  article.Price = priceRule.UnitPriceIncrease.GetValueOrDefault(Decimal.Zero) - unitPrice;
                  break;

                case '+':
                  article.Price = priceRule.UnitPriceIncrease.GetValueOrDefault(Decimal.Zero) + unitPrice;
                  break;

                default:
                  article.Price = unitPrice;
                  break;

              }
              break;
          }
        }
      }

      return article;
    }

    protected override Boolean ValidateContext()
    {
      return Context.ConnectorSystemID.HasValue && Constants.Connector.System.MultiMag.Equals(Context.ConnectorSystem.Name, StringComparison.OrdinalIgnoreCase);
    }
  }
}