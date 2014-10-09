using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using Concentrator.Objects.Environments;
using Concentrator.Objects.Ftp;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Tasks.Euretco.RSO.Navision.Helpers;
using Concentrator.Tasks.Euretco.RSO.Navision.Models;
using Concentrator.Tasks.Euretco.Rso;
using Concentrator.Tasks.Stores;
using PetaPoco;

namespace Concentrator.Tasks.Euretco.RSO.Navision.Exporters
{
  [Task("Navision Product Exporter")]
  public sealed class ProductExporter : RsoTaskBase
  {
    #region Additional commandline switches

    #endregion

    #region Variables

    [Resource]
    private static readonly String GetConfigurableProducts = null;

    [Resource]
    private static readonly String GetSimpleProducts = null;

    [Resource]
    private static readonly String GetProductAttributes = null;

    private const string NavisionColor = "10";

    private static readonly Dictionary<String, String> SizeSorting = new Dictionary<string, string>()
		{
			{ "XS"    , "300" } ,
			{ "S"     , "310" } ,
			{ "M"     , "320" } ,
			{ "L"     , "330" } ,
			{ "XL"    , "340" } ,
			{ "2XL"   , "350" } ,
			{ "XXL"   , "360" } ,
			{ "3XL"   , "370" } ,
			{ "XXXL"  , "380" } ,
			{ "4XL"   , "390" } ,
			{ "XXXXL" , "400" } ,
		};
    #endregion

    #region Connection

    private string _connection;

    private string Connection
    {
      get
      {
        if (string.IsNullOrEmpty(_connection))
        {
          _connection = Environments.Current.Connection;
        }

        return _connection;
      }
    }

    #endregion

    #region Default language

    private Language DefaultLanguage { get; set; }

    private String DefaultLanguageName
    {
      get { return RsoConstants.DefaultLanguage; }
    }

    private Boolean LoadDefaultLanguage()
    {
      DefaultLanguage = Unit.Scope.Repository<Language>().GetSingle(item => item.Name == DefaultLanguageName);

      if (DefaultLanguage == null)
      {
        TraceError("The language '{0}' could not be found.", DefaultLanguageName);
        return false;
      }

      return true;
    }

    #endregion

    #region Default Vendor

    private Vendor DefaultVendor { get; set; }

    private String DefaultVendorName
    {
      get { return RsoConstants.VendorName; }
    }

    private Boolean LoadDefaultVendor()
    {
      DefaultVendor = Unit.Scope
                          .Repository<Vendor>()
                          .Include(vendor => vendor.VendorSettings)
                          .GetSingle(vendor => vendor.Name == DefaultVendorName);

      if (DefaultVendor == null)
      {
        TraceError("The vendor '{0}' could not be found.", DefaultVendorName);
        return false;
      }

      return true;
    }

    #endregion

    #region Default Connector

    private Connector DefaultConnector { get; set; }

    private String DefaultConnectorName
    {
      get { return RsoConstants.ConnectorName; }
    }

    private Boolean LoadDefaultConnector()
    {
      DefaultConnector = Unit.Scope
                             .Repository<Connector>()
                             .Include(connector => connector.ConnectorSettings)
                             .GetSingle(connector => connector.Name == DefaultConnectorName);

      if (DefaultConnector == null)
      {
        TraceError("The connector '{0}' could not be found.", DefaultConnectorName);
        return false;
      }

      return true;
    }

    #endregion

    #region Product Attribute Meda Data

    private class RsoProductAttributeStore : ProductAttributeStore
    {
      //[ProductAttribute("Collection_Code", true)]
      //public ProductAttributeMetaData CollectionCodeAttribute
      //{
      //  get;
      //  private set;
      //}

      [ProductAttribute("Color", true)]
      public ProductAttributeMetaData ColorAttribute
      {
        get;
        private set;
      }

      //[ProductAttribute("Color_Navision", true)]
      //public ProductAttributeMetaData ColorNavisionAttribute
      //{
      //  get;
      //  private set;
      //}

      //[ProductAttribute("Country_Origin", true)]
      //public ProductAttributeMetaData CountryOriginAttribute
      //{
      //  get;
      //  private set;
      //}

      //[ProductAttribute("Country_Purchased", true)]
      //public ProductAttributeMetaData CountryPurchasedAttribute
      //{
      //  get;
      //  private set;
      //}

      //[ProductAttribute("Delivery_Code", true)]
      //public ProductAttributeMetaData DeliveryCodeAttribute
      //{
      //  get;
      //  private set;
      //}

      [ProductAttribute("Product_Code", true)]
      public ProductAttributeMetaData ProductCode
      {
        get;
        private set;
      }

      //[ProductAttribute("Quality", true)]
      //public ProductAttributeMetaData QualityAttribute
      //{
      //  get;
      //  private set;
      //}

      //[ProductAttribute("Requisition_Method", true)]
      //public ProductAttributeMetaData RequisitionMethodAttribute
      //{
      //  get;
      //  private set;
      //}

      //[ProductAttribute("PURCHASING_CODE", true)]
      //public ProductAttributeMetaData PurchasingCodeAttribute
      //{
      //  get;
      //  private set;
      //}

      //[ProductAttribute("Season_Code", true)]
      //public ProductAttributeMetaData SeasonCodeAttribute
      //{
      //  get;
      //  private set;
      //}

      [ProductAttribute("Size", false)]
      public ProductAttributeMetaData SizeAttribute
      {
        get;
        private set;
      }

      //[ProductAttribute("Status_Code", true)]
      //public ProductAttributeMetaData StatusCodeAttribute
      //{
      //  get;
      //  private set;
      //}

      [ProductAttribute("Subsize", false)]
      public ProductAttributeMetaData SubsizeAttribute
      {
        get;
        private set;
      }

      //[ProductAttribute("Tariff_Code", true)]
      //public ProductAttributeMetaData TariffCodeAttribute
      //{
      //  get;
      //  private set;
      //}

      //[ProductAttribute("Theme", true)]
      //public ProductAttributeMetaData ThemeCodeAttribute
      //{
      //  get;
      //  private set;
      //}

      //[ProductAttribute("Unit_Gross_Weight", true)]
      //public ProductAttributeMetaData UnitGrossWeightAttribute
      //{
      //  get;
      //  private set;
      //}

      //[ProductAttribute("Unit_Netto_Weight", true)]
      //public ProductAttributeMetaData UnitNettoWeightAttribute
      //{
      //  get;
      //  private set;
      //}

      public RsoProductAttributeStore(TraceSource traceSource = null)
        : base(traceSource)
      {
      }
    }

    private Boolean LoadProductAttributes()
    {
      ProductAttributes = new RsoProductAttributeStore(TraceSource);

      return ProductAttributes.Load();
    }

    private RsoProductAttributeStore ProductAttributes
    {
      get;
      set;
    }

    #endregion

    #region Database cache

    private IEnumerable<ConfigurableProductModel> _currentConfigurableProducts = Enumerable.Empty<ConfigurableProductModel>();
    private Dictionary<int, List<SimpleProductModel>> _currentSimpleProducts = new Dictionary<int, List<SimpleProductModel>>();
    private Dictionary<int, List<ProductAttributeModel>> _currentProductAttributes = new Dictionary<int, List<ProductAttributeModel>>();

    #endregion

    #region Cache Data
    /// <summary>
    ///   Cache some relevant data from a Concentrator database, used for lookup during further processing
    /// </summary>
    /// <param name="connectorID">Connector ID from context</param>
    /// <param name="languageID">Language ID from context</param>
    /// <param name="vendorID">Vendor ID from context</param>
    /// <returns>Was caching successful?</returns>
    private bool LoadDatabaseCache()
    {
      var success = false;
      try
      {
        TraceVerbose("Caching product data...");

        using (var pDb = new Database(Connection, Database.MsSqlClientProvider))
        {
          _currentConfigurableProducts = pDb
            .Query<ConfigurableProductModel>(String.Format(GetConfigurableProducts, DefaultVendor.VendorID, DefaultLanguage.LanguageID))
            .ToList();

          TraceVerbose("Found '{0}' Configurable products.", _currentConfigurableProducts.Count());

          if (_currentConfigurableProducts.Any())
          {
            //TODO: change const value 2 => RelatedProductType
            _currentSimpleProducts = pDb
              .Query<SimpleProductModel>(String.Format(GetSimpleProducts, DefaultVendor.VendorID, 2))
              .GroupBy(x => x.ConfigurableProductID)
              .ToDictionary(x => x.Key, y => y.ToList());

            TraceVerbose("Found '{0}' Simple products.", _currentSimpleProducts.SelectMany(x=>x.Value).Count());
            
            _currentProductAttributes = pDb
              .Query<ProductAttributeModel>(string.Format(GetProductAttributes, DefaultVendor.VendorID, DefaultLanguage.LanguageID))
              .GroupBy(x => x.ProductID)
              .ToDictionary(x => x.Key, y => y.ToList());

            TraceVerbose("Fount '{0}' Product and '{1}' Product Attribute Values", 
              _currentProductAttributes.Count, 
              _currentProductAttributes.SelectMany(x=>x.Value).Count());

            success = true;
          }
        }
      }
      catch (Exception ex)
      {
        TraceError("Data retrieval aborted: ", ex.Message);
      }
      return success;
    }
    #endregion

    #region Load BrandCodes

    private ILookup<String, String> BrandCodes
    {
      get;
      set;
    }

    private void LoadPricatBrandCodes()
    {
      BrandCodes = PricatBrands.ToLookup(pricatBrand => pricatBrand.Name, pricatBrand => pricatBrand.Code, StringComparer.OrdinalIgnoreCase);

      foreach (var brandName in BrandCodes.Where(group => group.Count() > 1).Select(group => group.Key))
      {
        TraceWarning("The brand '{0}' has multiple brand codes.", brandName);
      }
    }

    #endregion

    #region Navision Settings

    private NavisionSettingStore NavisionSettings { get; set; }

    private Boolean LoadNavisionSettings()
    {
      NavisionSettings = new NavisionSettingStore(DefaultConnector, TraceSource);

      return NavisionSettings.Load();
    }

    private class NavisionSettingStore : ConnectorSettingStoreBase
    {
      public NavisionSettingStore(Connector connector, TraceSource traceSource)
        : base(connector, traceSource)
      {
      }

      [ConnectorSetting("Navision Product Export Server")]
      public String Server
      {
        get;
        set;
      }

      [ConnectorSetting("Navision Product Export Username")]
      public String UserName
      {
        get;
        set;
      }

      [ConnectorSetting("Navision Product Export Password")]
      public String Password
      {
        get;
        set;
      }

      [ConnectorSetting("Navision Archive")]
      public String ArchiveDirectory
      {
        get;
        set;
      }
    }

    #endregion

    #region Private Functions

    private Boolean EnsureArchiveDirectory()
    {
      try
      {
        var archiveDirectory = NavisionSettings.ArchiveDirectory;

        if (!Directory.Exists(archiveDirectory))
        {
          Directory.CreateDirectory(archiveDirectory);
        }

        var uploadedProducts = Path.Combine(NavisionSettings.ArchiveDirectory, "UploadedProducts");

        if (!Directory.Exists(uploadedProducts))
        {
          Directory.CreateDirectory(uploadedProducts);
        }
      }
      catch (Exception)
      {
        TraceError("unable to create archive directory '{0}'", NavisionSettings.ArchiveDirectory);
        return false;
      }

      return true;
    }

    /// <summary>
    ///   Check whether uploading of a file is required, checks are based on presence and contentmatch
    /// </summary>
    /// <returns>Is upload required?</returns>
    private bool UploadRequired(XmlDocument article, string vendorItemNumber)
    {
      var uploadIt = true;

      var localFileName = GetArchiveFullFileName(vendorItemNumber);

      if (File.Exists(localFileName))
      {
        try
        {
          var archivedDom = new XmlDocument();
          archivedDom.Load(localFileName);
          uploadIt = (article.InnerXml != archivedDom.InnerXml);
        }
        catch (Exception)
        {
        }
      }

      return uploadIt;
    }

    private string GetArchiveFullFileName(string vendorItemNumber)
    {
      var localFileName = string.Format(RsoConstants.ExportFileNameTemplate, vendorItemNumber);

      var archivedFilename = Path.Combine(
        NavisionSettings.ArchiveDirectory,
        "UploadedProducts",
        Path.GetFileName(localFileName));

      return archivedFilename;
    }

    private Boolean TryGetAttributeValue(Int32 productID, ProductAttributeMetaData productAttribute, Language language, out string result)
    {
      result = String.Empty;

      if (_currentProductAttributes.ContainsKey(productID))
      {
        var productAttributeValue = _currentProductAttributes[productID]
          .SingleOrDefault(x => x.AttributeID == productAttribute.AttributeID);

        if (productAttributeValue != null)
        {
          result = productAttributeValue.AttributeValue;

          return true;
        }
      }

      return false;
    }

    private String ReplaceNavisionIllegalCharacters(String input)
    {
      return input
        .Replace(" \"", "i")
        .Replace(" ½", ".5")
        .Replace(" ⅓", ".3")
        .Replace(" ⅔", ".6")
        .Replace(" ¼", ".25")
        .Replace(" ¾", ".75");
    }
    #endregion

    #region Document Generation

    private XmlDocumentModel CreateDocumentModel()
    {
      // Save the current culture so it can be restored
      var previousCulture = CultureInfo.CurrentCulture;

      Thread.CurrentThread.CurrentCulture = CultureInfo
        .GetCultures(CultureTypes.NeutralCultures)
        .Single(culture => culture.NativeName == DefaultLanguage.Name);

      try
      {
        var documentContent = CreateArticles();

        return documentContent;

      }
      catch (Exception exception)
      {
        TraceError("Unable to create the export document for Connector '{0}'. {1}", DefaultConnector.Name, exception.Message);
      }
      finally
      {
        // Restore the culture settings to the previous state
        Thread.CurrentThread.CurrentCulture = previousCulture;
      }

      return null;
 
    }

    private XmlDocumentModel CreateArticles()
    {
      TraceVerbose("Retrieving the configurable products...");

      var xmlDocument = new XmlDocumentModel();

      foreach (var configurableProduct in _currentConfigurableProducts)
      {
        TraceVerbose("Processing {0}: {1}", configurableProduct.ProductID, configurableProduct.VendorItemNumber);

        var articleInformation = CreateArticleInformation(configurableProduct);

        var articleXml = ProductExporterHelper.SerializeModel(articleInformation);

        if (UploadRequired(articleXml, configurableProduct.VendorItemNumber))
        {
          xmlDocument.Articles.Add(articleInformation);

          TraceVerbose("XML-file '{0}.xml' changed; upload required.", configurableProduct.VendorItemNumber);
        }
        else
        {
          TraceVerbose("XML-file '{0}.xml' already processed before; upload not required.", configurableProduct.VendorItemNumber);
        }
      }

      return xmlDocument;
    }

    private Article CreateArticleInformation(ConfigurableProductModel configurableProduct)
    {
      var brandCode = BrandCodes[configurableProduct.BrandName].FirstOrDefault() ?? String.Empty;
      string productCode;

      TryGetAttributeValue(configurableProduct.ProductID, ProductAttributes.ProductCode, DefaultLanguage, out productCode);
      
      var article = new Article
        {
          ArticleNumber = configurableProduct.ProductID.ToString(CultureInfo.InvariantCulture).Wrap(20),
          ArticleDescription = (configurableProduct.ProductName ?? String.Empty).Wrap(50),
          ArticleDescription2 = (configurableProduct.ShortDescription ?? String.Empty).Wrap(50),
          SupplierNumber = (DefaultVendor.BackendVendorCode ?? String.Empty),
          SupplierItemNumber = configurableProduct.VendorItemNumber.Wrap(20),
          ItemCategory = productCode.Wrap(2),
          ProductCode = productCode.Wrap(10),
          Brand = brandCode.Wrap(10) ?? String.Empty,
          BuyingPrice = configurableProduct.CostPrice.ToString("N2"),
          SalesPrice = configurableProduct.Price.ToString("N2"),
          Variants = CreateVariants(configurableProduct)
        };

      return article;
    }

    private List<VariantModel> CreateVariants(ConfigurableProductModel configurableProduct)
    {
      var variants = new List<VariantModel>();

      if (_currentSimpleProducts.ContainsKey(configurableProduct.ProductID))
      {
        foreach (var currentSimpleProduct in _currentSimpleProducts[configurableProduct.ProductID])
        {
          variants.Add(CreateVariant(currentSimpleProduct));
        }
      }

      return variants;
    }

    private VariantModel CreateVariant(SimpleProductModel currentSimpleProduct)
    {
      var variant = new VariantModel
        {
          PimArticleNumber = currentSimpleProduct.SimpleProductID.ToString(CultureInfo.InvariantCulture).Wrap(30),
          VariantDescription = currentSimpleProduct.SimpleVendorItemNumber.Wrap(50),
          BarcodeInfo = CreateBarcodeInformation(currentSimpleProduct),
          ColorInfo = CreateColorInformation(currentSimpleProduct),
          SizeInfo = CreateSizeInformation(currentSimpleProduct)
        };

      return variant;
    }

    private BarcodeInfoModel CreateBarcodeInformation(SimpleProductModel currentSimpleProduct)
    {
      var barcodeInfo = new BarcodeInfoModel
        {
        EanCode = currentSimpleProduct.Barcode.Wrap(13)
      };

      return barcodeInfo;
    }

    private ColorInfoModel CreateColorInformation(SimpleProductModel currentSimpleProduct)
    {
      var colorInfo = new ColorInfoModel();

      string attributeValue;

      if (TryGetAttributeValue(currentSimpleProduct.ConfigurableProductID, ProductAttributes.ColorAttribute, DefaultLanguage, out attributeValue))
      {
        colorInfo.Color = NavisionColor.Wrap(2);
        colorInfo.ColorDescription = attributeValue;
        colorInfo.ColorSorting = NavisionColor.Wrap(3);

        return colorInfo;
      }

      colorInfo.Color = String.Empty;
      colorInfo.ColorDescription = String.Empty;
      colorInfo.ColorSorting = "000";

      return colorInfo;
    }

    private SizeInfoModel CreateSizeInformation(SimpleProductModel currentSimpleProduct)
    {
      var sizeInfo = new SizeInfoModel();

      string sizeValue;
      if (TryGetAttributeValue(currentSimpleProduct.SimpleProductID, ProductAttributes.SizeAttribute, DefaultLanguage, out sizeValue))
      {
        sizeValue = ReplaceNavisionIllegalCharacters(sizeValue);

        string subSizeValue;
        if (TryGetAttributeValue(currentSimpleProduct.SimpleProductID, ProductAttributes.SubsizeAttribute, DefaultLanguage, out subSizeValue))
        {
          sizeValue += subSizeValue;
        }

        if (sizeValue.Length > 7)
        {
          sizeValue = sizeValue
            .Replace("-", String.Empty)
            .Replace(" ", String.Empty);
        }

        if (sizeValue.Length > 7)
        {
          TraceWarning("For the product '{0}' has a size and subsize ", currentSimpleProduct.SimpleVendorItemNumber);
        }

        sizeInfo.Size = sizeValue.Wrap(7);
        sizeInfo.SizeDescription = sizeValue.Wrap(7);

        Int32 sizeSorting;

        if (Int32.TryParse(sizeValue, out sizeSorting))
        {
          sizeInfo.SizeSorting = sizeSorting.ToString(CultureInfo.InvariantCulture).Wrap(3);
        }
        else
        {
          sizeInfo.SizeSorting = SizeSorting.ContainsKey(sizeValue) ? SizeSorting[sizeValue] : "100";
        }
      }

      return sizeInfo;
    }

    #endregion

    protected override void ExecutePricatTask()
    {
      EmbeddedResourceHelper.Bind(this);

      if (LoadDefaultLanguage() && LoadDefaultVendor() && LoadDefaultConnector() && LoadNavisionSettings() && EnsureArchiveDirectory() && LoadProductAttributes())
      {
        if (LoadDatabaseCache())
        {
          LoadPricatBrandCodes();

          IFtpClient ftpClient;

          try
          {
            ftpClient = FtpClientFactory.Create(new Uri(NavisionSettings.Server), NavisionSettings.UserName, NavisionSettings.Password);
            ftpClient.Update();
          }
          catch (Exception ex)
          {
            TraceError("Unable to establish FTP connection to remote server \"{0}\" for exporting BizTalk files.", NavisionSettings.Server);
            return;
          }

          ExportProducts(ftpClient);
        }
        else
        {
          TraceError("Could not cache Concentrator database data, further processing not possible now.");
        }
      }
      else
      {
        TraceError("Could not load context data, further processing not possible now.");
      }

    }

    private void ExportProducts(IFtpClient ftpClient)
    {
      var document = CreateDocumentModel();

      if (document.Articles.Count > 0)
      {
        TraceVerbose("'{0}' articles are changed; These articles will be uploaded.", document.Articles.Count);

        var xmlProduct = ProductExporterHelper.SerializeModel(document);

        var fileName = String.Format("PIM_AI_{0:yyyyMMddHHmmss}.xml", DateTime.Now);

        try
        {
          using (var memoryStream = new MemoryStream())
          {
            TraceVerbose("Uploading XML-file \"{0}\" to remote server \"{1}\" ...", fileName, NavisionSettings.Server);
            
            xmlProduct.Save(memoryStream);

            memoryStream.Position = 0;

            ftpClient.UploadFile(fileName, memoryStream);
          }

          foreach (var article in document.Articles)
          {
            var localProductXmlFilename = GetArchiveFullFileName(article.SupplierItemNumber);

            var articleXml = ProductExporterHelper.SerializeModel(article);

            articleXml.Save(localProductXmlFilename);
          }
        }
        catch (IOException exception)
        {
          TraceError("Unable to save the file to the ftp directory '{0}'. {1}", NavisionSettings.Server, exception.Message);
        }

        try
        {
          TraceVerbose("Uploading XML-file '{0}' to local directory '{1}' ...", fileName, NavisionSettings.ArchiveDirectory);

          xmlProduct.Save(Path.Combine(NavisionSettings.ArchiveDirectory, fileName));
        }
        catch (IOException exception)
        {
          TraceWarning("Unable to save the file to the archive directory '{0}'. {1}", NavisionSettings.ArchiveDirectory, exception.Message);
        }
      }
      else
      {
        TraceVerbose("Upload not requiered. There is no article change.");
      }
    }
  }
}
