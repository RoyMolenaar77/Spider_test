#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Concentrator.Objects.Environments;
using Concentrator.Objects.Ftp;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Tasks.Euretco.Rso.BizTalk.ProductExport.Enumeration;
using Concentrator.Tasks.Euretco.Rso.BizTalk.ProductExport.Models;
using Concentrator.Tasks.Euretco.Rso.BizTalk.ProductExport.Properties;
using PetaPoco;
using Attribute = Concentrator.Tasks.Euretco.Rso.BizTalk.ProductExport.Models.Attribute;

#endregion

namespace Concentrator.Tasks.Euretco.Rso.BizTalk.ProductExport
{
  /// <summary>
  ///   Class that exports Concentrator products as a set of XML-files
  /// </summary>
  [Task("RSO Product Exporter")]
  public class ProductExporter : TaskBase
  {
    #region Additional commandline parameters

    /// <summary>
    ///  
    /// </summary>
    [CommandLineParameter("/FullExport", "/FE")]
    internal static Boolean EnableFullExport { get; set; }

    /// <summary>
    /// Gets a collection of product ID's that where explicitily supplied via the command-line.
    /// </summary>
    [CommandLineParameter("/Products", "/Product", "/P")]
    internal static Int32[] ProductIDs { get; set; }

    #endregion

    #region Constants

    private const String ErrorDirectoryName = "Error";
    private const String SuccessDirectoryName = "Success";
    [Resource]
    private static readonly String GetConfigurableProducts = null;
    [Resource]
    private static readonly String GetProducts = null;
    [Resource]
    private static readonly String GetAttributes = null;
    [Resource]
    private static readonly String GetRelatedProducts = null;
    [Resource]
    private static readonly String GetCategoryInfo = null;
    [Resource]
    private static readonly String GetProductWithImage = null;

    private readonly Dictionary<string, string> _customAttributeCodeMap = new Dictionary<string, string>
      {
        {"Color_Supplier", "color_supplier"},
        {"Delivery_Time", "delivery_time"},
        {"Url Key", "url_key"},
        {"Product_Code", "productcode"},
        {"ProductType", "producttype"}
      };

    private readonly Dictionary<string, string> _delimiterByAttributeCodes = new Dictionary<string, string>
      {
        {"Gender", ","}
      };

    private readonly List<string> _variantAttributes = new List<string>
      {
        "Size"
      };

    private readonly Dictionary<string, FieldRoutingType> _attributeFieldRouting = new Dictionary<string, FieldRoutingType>
      {
        {"Sport", FieldRoutingType.MultiValue},
        {"Gender", FieldRoutingType.MultiValue},
        {"Color", FieldRoutingType.MultiValue},
        {"Material", FieldRoutingType.TextValue},
        {"Collection", FieldRoutingType.TextValue},
        {"mpn", FieldRoutingType.TextValue},
        {"Color_Code", FieldRoutingType.TextValue},
        {"Lining", FieldRoutingType.TextValue},
        {"Ademend", FieldRoutingType.TextValue},
        {"Fit", FieldRoutingType.TextValue},
        {"Area", FieldRoutingType.TextValue},
        {"Color_Supplier", FieldRoutingType.TextValue},
        {"Delivery_Time", FieldRoutingType.TextValue},
        {"Size_Supplier", FieldRoutingType.TextValue},
        {"Seizoen", FieldRoutingType.TextValue},
        {"WashingInstructions", FieldRoutingType.TextValue},
        {"Video", FieldRoutingType.TextValue},
        {"Url Key", FieldRoutingType.TextValue},
        {"Product_Code", FieldRoutingType.TextValue},
        {"ProductType", FieldRoutingType.TextValue}
      };

    #endregion

    #region Configurable items

    private string _connection;

    protected string Connection
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

    #region Context data

    #region Default language

    protected Language DefaultLanguage { get; private set; }

    protected virtual String DefaultLanguageName
    {
      get { return Resources.DefaultLanguage; }
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

    protected Vendor DefaultVendor { get; private set; }

    protected virtual String DefaultVendorName
    {
      get { return Resources.DefaultVendor; }
    }

    protected Boolean LoadDefaultVendor()
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

    protected Connector DefaultConnector { get; private set; }

    protected virtual String DefaultConnectorName
    {
      get { return Resources.DefaultConnector; }
    }

    protected Boolean LoadDefaultConnector()
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

    #region Database cache

    private IEnumerable<ConfiguredProductDBModel> _currentProducts = Enumerable.Empty<ConfiguredProductDBModel>();
    private IEnumerable<ProductAttributeValueDBModel> _productAttributes = Enumerable.Empty<ProductAttributeValueDBModel>();
    private IEnumerable<ProductDBModel> _products = Enumerable.Empty<ProductDBModel>();
    private IEnumerable<RelatedProductDBModel> _relatedProducts = Enumerable.Empty<RelatedProductDBModel>();
    private IEnumerable<int> _configurableProductWithImage = Enumerable.Empty<int>();

    #endregion

    #endregion

    #region Business logic

    /// <summary>
    ///   Transfer a local file to a remote FTP client
    /// </summary>
    /// <param name="ftpClient">FTP client</param>
    /// <param name="localProductDomFilename">local product filename</param>
    /// <param name="remoteProductDomFilename">remote product filename</param>
    private void TransferProductFile(IFtpClient ftpClient, string localProductDomFilename, string remoteProductDomFilename)
    {
      FileStream fileStream = null;
      try
      {
        using (fileStream = File.OpenRead(localProductDomFilename))
        {
          TraceVerbose("Uploading XML-file \"{0}\" to remote server \"{1}\" ...",
                       Path.GetFileName(localProductDomFilename), ExportSettings.Server);
          ftpClient.UploadFile(remoteProductDomFilename, fileStream);
        }
        LocalArchive(localProductDomFilename, SuccessDirectoryName);
      }
      catch (Exception ex)
      {
        TraceError("Error occured during transfer of XML-file \"{0}\" to remote server \"{1}\". Error: {1}",
                   Path.GetFileName(localProductDomFilename), ExportSettings.Server, ex.Message);

        if (fileStream != null)
        {
          fileStream.Close();
          fileStream.Dispose();
        }
        LocalArchive(localProductDomFilename, ErrorDirectoryName);
      }
    }

    /// <summary>
    ///   Check whether uploading of a file is required, checks are based on presence and contentmatch
    /// </summary>
    /// <param name="localFileName">local filename</param>
    /// <returns>Is upload required?</returns>
    private bool UploadRequired(string localFileName)
    {
      var uploadIt = true;

      var archivedFilename = Path.Combine(DeriveArchiveFolder(localFileName, SuccessDirectoryName), Path.GetFileName(localFileName));

      if (File.Exists(localFileName) && File.Exists(archivedFilename))
      {
        try
        {
          var localDom = new XmlDocument();
          var archivedDom = new XmlDocument();
          localDom.Load(localFileName);
          archivedDom.Load(archivedFilename);
          uploadIt = (localDom.InnerXml != archivedDom.InnerXml);
        }
        catch (Exception)
        {
        }
      }

      return uploadIt;
    }

    /// <summary>
    ///   Archive a local file in a local archiving structure
    /// </summary>
    /// <param name="localFileName">local filename, present in some archive root</param>
    /// <param name="subFolderName">subfolder underneath some archive root</param>
    private void LocalArchive(string localFileName, string subFolderName)
    {
      try
      {
        var path = DeriveArchiveFolder(localFileName, subFolderName);
        if (!Directory.Exists(path))
        {
          Directory.CreateDirectory(path);
        }
        var targetFilename = Path.Combine(path, Path.GetFileName(localFileName));
        if (File.Exists(targetFilename))
        {
          File.Delete(targetFilename);
        }
        File.Move(localFileName, targetFilename);
      }
      catch (Exception ex)
      {
        TraceError("Could not archive XML-file \"{0}\" locally. Error: {1}",
                   Path.GetFileName(localFileName), ex.Message);
      }
    }

    /// <summary>
    ///   Derive an archiving file path based on a file that's already present underneath an archiving root and an archive subfolder
    /// </summary>
    /// <param name="localFilename">local filename, present unerneath an archiving root</param>
    /// <param name="subFoldername">subfolder of an archiving root</param>
    /// <returns>Derived archiving file path</returns>
    private string DeriveArchiveFolder(string localFilename, string subFoldername)
    {
      return Path.Combine(Path.GetDirectoryName(localFilename), subFoldername);
    }

    /// <summary>
    ///   Create an arbitrary serializable product model, based on a product and connector
    /// </summary>
    /// <param name="product">product from database</param>
    /// <param name="connectorID">connector ID from context</param>
    /// <returns>serializable product model</returns>
    private ProductBizTalkModel CreateProductBizTalkModel(ProductDBModel product, int connectorID)
    {
      var productModel = MapSimpleProperties(product);
      DerivePropertiesFromProductAttributes(ref productModel, product);

      productModel.Attributes = new List<Attribute>();
      productModel.Attributes.AddRange(DeriveArtificialAttributes(product, connectorID));
      productModel.Attributes.AddRange(CreateAttributes(product));

      productModel.Variants = CreateVariantSection(product).ToList();

      return productModel;
    }

    /// <summary>
    ///   Maps simple product properties into a product model
    /// </summary>
    /// <param name="product">product from database</param>
    /// <returns>Partially filled product model</returns>
    private static ProductBizTalkModel MapSimpleProperties(ProductDBModel product)
    {
      return new ProductBizTalkModel
        {
          AttributeSet = "Default",
          Websites = new List<string> { "base" },
          TaxClass = "high",
          Visibility = 4,
          Sku = product.VendorItemNumber,
          Name = product.ProductName,
          Description = product.LongContentDescription,
          ShortDescription = product.ShortContentDescription,
          LongSummary = product.LongSummaryDescription,
          ShortSummary = product.ShortSummaryDescription,
          Price = product.Price,
          Status = product.IsActive ? 1 : 0
        };
    }

    /// <summary>
    ///   These product attributes are promoted from the attribute list in the output to dedicated properties
    /// </summary>
    /// <param name="productModel">IN: a product output model, OUT: enriched product output model</param>
    /// <param name="product">product from database</param>
    private void DerivePropertiesFromProductAttributes(ref ProductBizTalkModel productModel, ProductDBModel product)
    {
      //try
      //{
      //  productModel.SpecialPriceStartDate =
      //    DateTime.Parse(_productAttributes.Where(x => x.AttributeCode == "Special_From_Date").Select(x => x.Value).FirstOrDefault());
      //}
      //catch (Exception ex)
      //{
      //  TraceError("Could not extract \"Special_From_Date\" for product \"{0}\". Error: {1}", product.ProductID, ex.Message);
      //}

      //try
      //{
      //  productModel.SpecialPriceEndDate = DateTime.Parse(_productAttributes.Where(x => x.AttributeCode == "Special_To_Date").Select(x => x.Value).FirstOrDefault());
      //}
      //catch (Exception ex)
      //{
      //  TraceError("Could not extract \"Special_To_Date\" for product \"{0}\". Error: {1}", product.ProductID, ex.Message);
      //}

      try
      {
        productModel.Weight = _productAttributes.Where(x => x.AttributeCode == "weight").Select(x => x.Value).FirstOrDefault();
      }
      catch (Exception ex)
      {
        TraceError("Could not extract \"weight\" for product \"{0}\". Error: {1}", product.ProductID, ex.Message);
      }
    }

    /// <summary>
    ///   Derive artificial product attributes (in the sense that they're not part of the Concentrator product attribute datamodel) from other product properties
    ///   The codes used here are not part of the product attribute set OR redefined to be sourced from another source!
    ///   Note they are also localized!
    /// </summary>
    /// <param name="product">product from database</param>
    /// <param name="connectorID">connector ID from context</param>
    /// <returns>A list of products attributes, to fit in a product output model</returns>
    private IEnumerable<Attribute> DeriveArtificialAttributes(ProductDBModel product, int connectorID)
    {
      var generatedAttributes = new List<Attribute>();

      generatedAttributes.Add(new Attribute
        {
          Code = "artikelcode",
          TextValue = new List<string> { product.VendorItemNumber.Split(" ").First() },
          MultiValue = null
        });

      generatedAttributes.Add(new Attribute
        {
          Code = "merk",
          TextValue = new List<string> { product.BrandName },
          MultiValue = null
        });

      #region Category processing

      var masterGroupMappingID = _currentProducts
        .Where(x => x.ProductID == product.ProductID)
        .Select(x => x.MasterGroupMappingID)
        .FirstOrDefault();

      var categoryHierarchy = DeriveCategoryPath(masterGroupMappingID, connectorID);

      if (categoryHierarchy.Any())
      {
        generatedAttributes.Add(ConvertHierarchyToCategoryAttribute(categoryHierarchy));
      }

      #endregion

      return generatedAttributes;
    }

    private Attribute ConvertHierarchyToCategoryAttribute(IEnumerable<CategoryLevelInfo> categoryHierarchy)
    {
      var returnAttribute = new Attribute
      {
        Code = "categorie",
        MultiValue = new List<string>()
      }; 
      
      foreach (var categoryLevelInfo in categoryHierarchy.Take(3))
      {
        returnAttribute.MultiValue.Add(categoryLevelInfo.LevelName);
      }
      
      return returnAttribute;
    }

    /// <summary>
    ///   Create product attributes suited to fit in a product model from regular product attributes
    /// </summary>
    /// <param name="product">product from database</param>
    /// <returns>A list of products attributes, to fit in a product output model</returns>
    private IEnumerable<Attribute> CreateAttributes(ProductDBModel product)
    {
      var productAttributesByProduct = from attribute in _productAttributes
                                       where attribute.ProductID == product.ProductID
                                       select attribute;

      var unmappedCodes = new Dictionary<string, int>(); // used for development purposes

      var generatedAttributes = new List<Attribute>();

      foreach (var attr in productAttributesByProduct)
      {
        if (_attributeFieldRouting.ContainsKey(attr.AttributeCode))
        {
          switch (_attributeFieldRouting[attr.AttributeCode])
          {
            case FieldRoutingType.TextValue:
              generatedAttributes.Add(new Attribute
                {
                  Code = GetCustomizedAttributeCode(attr.AttributeCode, attr.Name).ToLower(),
                  TextValue = new List<string> { attr.Value },
                  MultiValue = null
                });
              break;
            case FieldRoutingType.MultiValue: // Note that when attribute value options are really supported, the processing underneath has to change!
              generatedAttributes.Add(new Attribute
                {
                  Code = GetCustomizedAttributeCode(attr.AttributeCode, attr.Name).ToLower(),
                  TextValue = null,
                  MultiValue = Regex.Split(attr.Value, GetDelimiterByAttributeCode(attr.AttributeCode)).Select(p => p.Trim()).ToList()
                });
              break;
          }

          // attributes to be duplicated here...
          // refactor this with a triple association like {"Color", "kleur", "hoofdkleur"}, lookup by attributeCode etc.
          if (attr.AttributeCode == "Color")
          {
            generatedAttributes.Add((from duplicatableAttribute in generatedAttributes
                                     where duplicatableAttribute.Code == "kleur"
                                     select new Attribute
                                       {
                                         Code = "hoofdkleur",
                                         MultiValue = duplicatableAttribute.MultiValue,
                                         TextValue = duplicatableAttribute.TextValue
                                       }).FirstOrDefault());
          }
        }
        else
        {
          unmappedCodes[attr.AttributeCode] = 0; // see what is missing...
        }
      }

      return generatedAttributes;
    }

    /// <summary>
    ///   Create a list of simple product data (called variants in the terminology of the product output model)
    /// </summary>
    /// <param name="product">product from database</param>
    /// <returns>A list of variant structures, to fit in a product output model</returns>
    private IEnumerable<Variant> CreateVariantSection(ProductDBModel product)
    {
      return from variant in _relatedProducts
             where variant.ConfigurableProductID == product.ProductID
             select new Variant
               {
                 Sku = variant.Barcode,
                 Status = variant.IsActive ? 1 : 0,
                 Visibility = 1,
                 PriceCorrection = (int)(variant.Price - product.Price),
                 Options = (from relatedProductAttribute in _productAttributes
                            where relatedProductAttribute.ProductID == variant.SimpleProductID &&
                                  _variantAttributes.Contains(relatedProductAttribute.AttributeCode)
                            select new Option
                              {
                                Code = relatedProductAttribute.Name.ToLower(),
                                Value = relatedProductAttribute.Value,
                              }).ToList()
               };
    }

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

        using (var pDb = new Database(Connection, Resources.PetaPocoProvider))
        {
          _currentProducts = pDb
            .Query<ConfiguredProductDBModel>(String.Format(GetConfigurableProducts, DefaultConnector.ConnectorID))
            .ToList();

          _configurableProductWithImage = pDb
            .Query<int>(string.Format(GetProductWithImage, DefaultConnector.ConnectorID))
            .ToList();

          if (ProductIDs.Any())
          {
            _currentProducts = _currentProducts
              .Where(x => ProductIDs.Contains(x.ProductID))
              .ToList();
          }

          if (_currentProducts.Any())
          {
            var productsQuery = String.Format(GetProducts,
                                              DefaultConnector.ConnectorID,
                                              DefaultLanguage.LanguageID,
                                              DefaultVendor.VendorID,
                                              ProductExporterHelper.SerializeProductsToSqlParm(_currentProducts, false));

            _products = pDb
              .Query<ProductDBModel>(productsQuery)
              .ToList();

            var relatedProductsQuery = String.Format(GetRelatedProducts,
                                                     DefaultVendor.VendorID,
                                                     ProductExporterHelper.SerializeProductsToSqlParm(_currentProducts, false));

            _relatedProducts = pDb
              .Query<RelatedProductDBModel>(relatedProductsQuery)
              .ToList();

            List<Int32> listOfAllProducts = _products
              .Select(x => x.ProductID)
              .ToList();
            
            listOfAllProducts.AddRange(
                _relatedProducts.Select(x=>x.SimpleProductID)
              );

            var productAttributesQuery = String.Format(GetAttributes,
                                           DefaultVendor.VendorID,
                                           DefaultLanguage.LanguageID,
                                           ProductExporterHelper.SerializeProductsToSqlParm(listOfAllProducts));

            _productAttributes = pDb
              .Query<ProductAttributeValueDBModel>(productAttributesQuery)
              .ToList();

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

    #region Product Export Settings

    private ProductExporterSettingsStore ExportSettings { get; set; }

    private Boolean LoadProductExporterSettings()
    {
      ExportSettings = new ProductExporterSettingsStore(DefaultConnector, TraceSource);
      return ExportSettings.Load();
    }

    #endregion

    /// <summary>
    ///   Landing point, execution starts here
    /// </summary>
    protected override void ExecuteTask()
    {
      EmbeddedResourceHelper.Bind(this);

      if (LoadDefaultVendor() && LoadDefaultLanguage() && LoadDefaultConnector() && LoadProductExporterSettings())
      {
        if (LoadDatabaseCache())
        {
          IFtpClient ftpClient;

          try
          {
            ftpClient = FtpClientFactory.Create(new Uri(ExportSettings.Server), ExportSettings.UserName, ExportSettings.Password);
            ftpClient.Update();
          }
          catch (Exception ex)
          {
            TraceError("Unable to establish FTP connection to remote server \"{0}\" for exporting BizTalk files.", ExportSettings.Server);
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
      var skipCounter = 0;
      var uploadCounter = 0;
      var failureCounter = 0;
      var productCounter = _products.Count();

      foreach (var product in _products)
      {
        TraceVerbose("Processing product \"{0}\".", product.VendorItemNumber);

        if (_configurableProductWithImage.Contains(product.ProductID))
        {
          var productModel = CreateProductBizTalkModel(product, DefaultConnector.ConnectorID);

          var productDom = ProductExporterHelper.SerializeModel(productModel);

          var localProductDomFilename = Path.Combine(ExportSettings.LocalExportFolder, String.Format(Resources.ExportFileNameTemplate, product.VendorItemNumber));
          var remoteProductDomFilename = Path.GetFileName(localProductDomFilename);

          productDom.Save(localProductDomFilename);

          if (File.Exists(localProductDomFilename))
          {
            if (UploadRequired(localProductDomFilename))
            {
              TransferProductFile(ftpClient, localProductDomFilename, remoteProductDomFilename);
              uploadCounter++;
            }
            else
            {
              TraceVerbose("Skipping Product: {0}... This product already processed before; upload not required.", product.VendorItemNumber);
              skipCounter++;
            }
          }
          else
          {
            TraceError("XML-file \"{0}\" not ready for transfer to remote server \"{1}\"",
                       Path.GetFileName(localProductDomFilename), ExportSettings.Server);
            failureCounter++;
          }
        }
        else
        {
          skipCounter++;
          TraceVerbose("Skipping Product: {0}... This product has no media!", product.VendorItemNumber);
        }

        TraceVerbose("Progress: {0} failure | {1} skipped | {2} uploaded | {3} Products", failureCounter, skipCounter, uploadCounter, productCounter);
      }
    }

    #region Private support methods

    /// <summary>
    ///   Customize product attribute code
    /// </summary>
    /// <param name="attributeCode">Attribute code</param>
    /// <param name="attributeName">Attribute name</param>
    /// <returns>Customized attribute code, to fit in a product output model</returns>
    private string GetCustomizedAttributeCode(string attributeCode, string attributeName)
    {
      return _customAttributeCodeMap.ContainsKey(attributeCode) ? _customAttributeCodeMap[attributeCode] : attributeName;
    }

    /// <summary>
    ///   Get required delimiter to split a singular text value into a multivalue list of strings
    /// </summary>
    /// <param name="attributeCode">Attribute code</param>
    /// <returns>Delimiter, default is ","</returns>
    private string GetDelimiterByAttributeCode(string attributeCode)
    {
      return _delimiterByAttributeCodes.ContainsKey(attributeCode) ? _delimiterByAttributeCodes[attributeCode] : ";";
    }

    /// <summary>
    ///   Derive a product category path, based on a mastergroup mapping ID and connector ID
    ///   Crawls up from a specified level upto it's root level
    /// </summary>
    /// <param name="mastergroupMappingID">Mastergroup mapping ID</param>
    /// <param name="connectorID">Connector ID from context</param>
    /// <returns>A category path</returns>
    private List<CategoryLevelInfo> DeriveCategoryPath(int mastergroupMappingID, int connectorID)
    {
      var categoryPath = new List<CategoryLevelInfo>();

      if (mastergroupMappingID != 0)
      {
        var runningMastergroupMappingID = mastergroupMappingID;
        CategoryLevelInfo categoryInfo;
        do
        {
          categoryInfo = GetCategoryLevelInfo(runningMastergroupMappingID, connectorID);
          categoryPath.Add(categoryInfo);
          runningMastergroupMappingID = categoryInfo.ParentMasterGroupMappingID;
        } while (categoryInfo.ParentMasterGroupMappingID != 0);
      }
      return categoryPath.OrderBy(x => x.LevelID).ToList();
    }

    /// <summary>
    ///   Get category level info from the Concentrator database
    /// </summary>
    /// <param name="mastergroupMappingID">mastergroup mapping ID</param>
    /// <param name="connectorID">Connector ID from context</param>
    /// <returns>category level info for the specified mastergroup mapping ID</returns>
    private CategoryLevelInfo GetCategoryLevelInfo(int mastergroupMappingID, int connectorID)
    {
      var categoryInfo = new CategoryLevelInfo();

      try
      {
        using (var pDb = new Database(Connection, Resources.PetaPocoProvider))
        {
          categoryInfo = pDb.Query<CategoryLevelInfo>(String.Format(GetCategoryInfo, connectorID, mastergroupMappingID)).FirstOrDefault();
        }
      }
      catch (Exception ex)
      {
        TraceError("Could not retrieve category information for mastergroup mapping ID {0} and connector ID {1}", mastergroupMappingID, connectorID);
      }

      return categoryInfo;
    }

    #endregion
  }
}