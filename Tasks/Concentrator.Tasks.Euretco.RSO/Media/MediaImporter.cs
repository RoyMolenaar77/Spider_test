#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Concentrator.Objects.Ftp;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Media;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Tasks.Euretco.Rso;
using Concentrator.Tasks.Stores;

#endregion

namespace Concentrator.Tasks.Euretco.RSO.MediaImporter
{
  [Task("EDI Product Media Importer")]
  public sealed class MediaImporter : RsoTaskBase
  {
    private const String ErrorDirectoryName = "Error";
    private const String SuccessDirectoryName = "Success";

    #region Pricat Color Mapping

    private IDictionary<String, String> PricatColorMapping { get; set; }

    private void LoadPricatColorMapping()
    {
      PricatColorMapping = PricatColors.ToDictionary(
        pricatColor => pricatColor.ColorCode,
        pricatColor => pricatColor.Filter);
    }

    #endregion

    #region Pricat Brand Mapping

    private IDictionary<String, String> PricatBrandMapping { get; set; }

    private void LoadPricatBrandMapping()
    {
      PricatBrandMapping = PricatBrands.ToDictionary(
        pricatBrand => pricatBrand.Alias,
        pricatBrand => pricatBrand.Name,
        StringComparer.CurrentCultureIgnoreCase);
    }

    #endregion

    #region Pricat Media

    private const String ImageMediaTypeName = "Image";

    private const String Image360MediaTypeName = "Image_360";

    private const String ConcentratorMediaDirectoryKey = "Concentrator Media Directory";

    private const String ConcentratorProductMediaDirectoryKey = "Concentrator Product Media Directory";
    private MediaType ImageMediaType { get; set; }
    private MediaType Image360MediaType { get; set; }
    private String ConcentratorMediaDirectory { get; set; }

    private String ConcentratorProductMediaDirectory { get; set; }

    private Boolean LoadMediaTypes()
    {
      ImageMediaType = Unit.Scope.Repository<MediaType>().GetSingle(item => item.Type == ImageMediaTypeName);

      if (ImageMediaType == null)
      {
        TraceError("Unable to load the media type '{0}', it does not exists.", ImageMediaTypeName);
      }

      Image360MediaType = Unit.Scope.Repository<MediaType>().GetSingle(item => item.Type == Image360MediaTypeName);

      if (Image360MediaType == null)
      {
        TraceError("Unable to load the media type '{0}', it does not exists.", Image360MediaTypeName);
      }

      return ImageMediaType != null && Image360MediaType != null;
    }

    #endregion

    #region Pricat Media Directories

    private static readonly Regex DirectoryNameRegex = new Regex(@"
^
(?<Brand>\w{3})
(?<Model>[\w\d-]+)
_
(?<Color>\d+)
$"
                                                                 , RegexOptions.Compiled
                                                                   | RegexOptions.IgnoreCase
                                                                   | RegexOptions.IgnorePatternWhitespace);

    private static readonly Regex DirectoryFileNameRegex = new Regex(@"^img_\d+_\d+_(?<Sequence>\d+)\.jpe?g$"
                                                                     , RegexOptions.Compiled
                                                                       | RegexOptions.IgnoreCase);

    private void ProcessMediaDirectories(FtpClient client)
    {
      TraceVerbose("Processing media directories...");

      var directoryNameMatchGroups = client.Directories
                                           .Select(directoryName => DirectoryNameRegex.Match(directoryName))
                                           .Where(match => match.Success)
                                           .GroupBy(match => new
                                             {
                                               BrandCode = match.Groups["Brand"].Value,
                                               ColorCode = match.Groups["Color"].Value,
                                               ModelName = match.Groups["Model"].Value
                                             })
                                           .ToArray();

      var productRepository = Unit.Scope.Repository<Product>().Include(product => product.Brand);

      foreach (var directoryNameMatchGroup in directoryNameMatchGroups)
      {
        TraceVerbose("Processing '{0}{1} {2}'..."
                     , directoryNameMatchGroup.Key.BrandCode
                     , directoryNameMatchGroup.Key.ModelName
                     , directoryNameMatchGroup.Key.ColorCode);

        var brandCode = directoryNameMatchGroup.Key.BrandCode;
        string brandName;

        if (!PricatBrandMapping.TryGetValue(brandCode, out brandName))
        {
          TraceError("There is no brand with the alias '{0}'.", brandCode);

          continue;
        }

        var vendorItemNumber = String.Join(" ", directoryNameMatchGroup.Key.ModelName, directoryNameMatchGroup.Key.ColorCode);
        var configurableProduct =
          productRepository.GetSingle(product => product.IsConfigurable && product.Brand.Name == brandName && product.VendorItemNumber == vendorItemNumber);

        if (configurableProduct == null)
        {
          TraceWarning("There is no configurable product with the brand '{0}' with the vendor item number '{1}'.", brandName, vendorItemNumber);

          continue;
        }

        foreach (var directoryNameMatch in directoryNameMatchGroup)
        {
          var remoteDirectory = directoryNameMatch.Value;

          client.Update(CombineUri(remoteDirectory, "images"));

          var fileNameSequenceLookup = client.Files
                                             .Select(file => DirectoryFileNameRegex.Match(file.FileName))
                                             .Where(match => match.Success)
                                             .Select(match => new
                                               {
                                                 FileName = match.Value,
                                                 Sequence = match.Groups["Sequence"].Value.ParseToInt()
                                               })
                                             .Where(item => item.Sequence.HasValue)
                                             .OrderBy(item => item.Sequence.Value)
                                             .ToDictionary(item => CombineUri(remoteDirectory, "images", item.FileName), item => item.Sequence.Value);

          var allFilesSuccessfullyDownloaded = true;

          var colorCode = directoryNameMatchGroup.Key.ColorCode.PadLeft(3, '0');

          foreach (var remoteFileName in fileNameSequenceLookup.Keys)
          {
            TraceVerbose("Processing '{0}'...", remoteFileName);

            var sequence = fileNameSequenceLookup[remoteFileName];

            var localFileName = String.Format("{0}_{1}_{2}_360_{3}{4}"
                                              , directoryNameMatchGroup.Key.BrandCode
                                              , directoryNameMatchGroup.Key.ModelName
                                              , colorCode + PricatColors
                                                              .Where(pricatColor => pricatColor.ColorCode == colorCode)
                                                              .Select(pricatColor => "_" + pricatColor.Filter)
                                                              .FirstOrDefault() ?? String.Empty
                                              , sequence
                                              , Path.GetExtension(remoteFileName))
                                      .ToLower();

            var relativeLocalFilePath = Path.Combine(RsoConstants.SubDirectoryProductMedia,
                                                     localFileName.Substring(0, 1),
                                                     localFileName.Substring(1, 1),
                                                     localFileName);

            var absoluteLocalFilePath = Path.Combine(ConcentratorMediaDirectory, relativeLocalFilePath);

            EnsureLocalDirectory(Path.GetDirectoryName(absoluteLocalFilePath));

            if (!DownloadFile(client, remoteFileName, absoluteLocalFilePath)
                || !SynchronizeProductMedia(new[] { configurableProduct }, relativeLocalFilePath, Image360MediaType, sequence, localFileName))
            {
              allFilesSuccessfullyDownloaded = false;
            }
          }

          if (allFilesSuccessfullyDownloaded)
          {
            TraceVerbose("Saving the product media...");

            string newRemoteDirectory;

            try
            {
              Unit.Save();

              newRemoteDirectory = CombineUri(SuccessDirectoryName, remoteDirectory);
            }
            catch (DataException exception)
            {
              TraceError("Unable to save the database changes. {0}", exception.InnerException.Message);

              newRemoteDirectory = CombineUri(ErrorDirectoryName, remoteDirectory);
            }

            try
            {
              client.RenameFile(remoteDirectory, newRemoteDirectory);
            }
            catch (Exception exception)
            {
              TraceError("Unable to move the remote directory '{0}' to the backup directory '{1}'. {2}"
                         , remoteDirectory
                         , newRemoteDirectory
                         , exception.Message);
            }
          }
        }
      }
    }

    #endregion

    #region Pricat Media Files

    private static readonly Regex FileNameRegex = new Regex(@"
        ^
        (?<Brand>\w[\w&\.]\w)
        (?<Model>[^_]+)
        _
        (?<Color>[^_]+)
        _
        (?<Sequence>\d+)
        \.jpe?g$"
                                                            , RegexOptions.Compiled
                                                              | RegexOptions.IgnoreCase
                                                              | RegexOptions.IgnorePatternWhitespace);

    [Resource]
    private static readonly String MergeProductMedia = null;

    private void ProcessMediaFiles(FtpClient client)
    {
      TraceVerbose("Processing media files...");

      var fileNameMatchGroups = client.Files
                                      .Select(file => FileNameRegex.Match(file.FileName))
                                      .Where(match => match.Success)
                                      .GroupBy(match => new
                                        {
                                          BrandCode = match.Groups["Brand"].Value,
                                          ColorCode = match.Groups["Color"].Value,
                                          ModelName = match.Groups["Model"].Value
                                        })
                                      .ToArray();

      var productRepository = Unit.Scope.Repository<Product>().Include(product => product.Brand);

      foreach (var fileNameMatchGroup in fileNameMatchGroups)
      {
        TraceVerbose("Processing '{0}{1} {2}'..."
                     , fileNameMatchGroup.Key.BrandCode
                     , fileNameMatchGroup.Key.ModelName
                     , fileNameMatchGroup.Key.ColorCode);

        var brandCode = fileNameMatchGroup.Key.BrandCode;
        string brandName;

        if (!PricatBrandMapping.TryGetValue(brandCode, out brandName))
        {
          TraceError("There is no brand with the alias '{0}'.", brandCode);

          continue;
        }

        var vendorItemNumber = String.Join(" ", fileNameMatchGroup.Key.ModelName, fileNameMatchGroup.Key.ColorCode);
        var configurableProduct =
          productRepository.GetSingle(product => product.IsConfigurable && product.Brand.Name == brandName && product.VendorItemNumber == vendorItemNumber);

        if (configurableProduct == null)
        {
          TraceWarning("There is no configurable product with the brand '{0}' with the vendor item number '{1}'.", brandName, vendorItemNumber);

          continue;
        }

        foreach (var fileNameMatch in fileNameMatchGroup)
        {
          var colorCode = fileNameMatch.Groups["Color"].Value.PadLeft(3, '0');
          var remoteFileName = CombineUri(fileNameMatch.Value);

          TraceVerbose("Processing '{0}'...", remoteFileName);

          // The Parse-method is safe to use, because the FileNameRegex only matches digits.
          var sequence = Int32.Parse(fileNameMatch.Groups["Sequence"].Value);

          var localFileName = String.Format("{0}_{1}_{2}_{3}{4}"
                                            , fileNameMatchGroup.Key.BrandCode
                                            , fileNameMatchGroup.Key.ModelName
                                            , colorCode + PricatColors
                                                            .Where(pricatColor => pricatColor.ColorCode == colorCode)
                                                            .Select(pricatColor => "_" + pricatColor.Filter)
                                                            .FirstOrDefault() ?? String.Empty
                                            , sequence
                                            , Path.GetExtension(fileNameMatch.Value))
                                    .ToLower();

          var relativeLocalFilePath = Path.Combine(RsoConstants.SubDirectoryProductMedia,
                                                   localFileName.Substring(0, 1),
                                                   localFileName.Substring(1, 1),
                                                   localFileName);

          var absoluteLocalFilePath = Path.Combine(ConcentratorMediaDirectory, relativeLocalFilePath);

          EnsureLocalDirectory(Path.GetDirectoryName(absoluteLocalFilePath));

          if (DownloadFile(client, remoteFileName, absoluteLocalFilePath))
          {
            var newRemoteFile = SynchronizeProductMedia(
              new[] { configurableProduct },
              relativeLocalFilePath,
              ImageMediaType,
              sequence,
              localFileName)
                                  ? CombineUri(SuccessDirectoryName, fileNameMatch.Value)
                                  : CombineUri(ErrorDirectoryName, fileNameMatch.Value);

            try
            {
              client.RenameFile(remoteFileName, newRemoteFile);
            }
            catch (Exception exception)
            {
              TraceError("Unable to move the remote file '{0}' to the backup directory '{1}'. {2}"
                         , remoteFileName
                         , newRemoteFile
                         , exception.Message);
            }
          }
        }
      }
    }

    private Boolean SynchronizeProductMedia(IEnumerable<Product> products, String mediaPath, MediaType mediaType, Int32 sequence, string fileName)
    {
      using (var productTable = products
        .Select(product => new { Value = product.ProductID })
        .ToDataTable())
      {
        try
        {
          Database.Execute(MergeProductMedia
                           , productTable.AsParameter("[dbo].[Int32Table]")
                           , DefaultVendor.VendorID
                           , mediaType.TypeID
                           , mediaPath
                           , sequence
                           , fileName);

          return true;
        }
        catch (DataException exception)
        {
          TraceError("Unable to merge the product media. {0}", exception.InnerException.Message);

          return false;
        }
      }
    }

    #endregion

    #region Pricat Media Settings

    private PricatMediaSettingStore PricatMediaSettings { get; set; }

    private Boolean LoadPricatMediaSettings()
    {
      PricatMediaSettings = new PricatMediaSettingStore(DefaultVendor, TraceSource);

      return PricatMediaSettings.Load();
    }

    private class PricatMediaSettingStore : VendorSettingStoreBase
    {
      public PricatMediaSettingStore(Vendor vendor, TraceSource traceSource = null)
        : base(vendor, traceSource)
      {
      }

      private String _server;

      [VendorSetting("Pricat Media Server")]
      public String Server
      {
        get
        {
#if DEBUG
          _server = _server.Replace("Staging", "Test");
#endif
          return _server;
        }
        set { _server = value; }
      }

      [VendorSetting("Pricat Media Username")]
      public String UserName { get; set; }

      [VendorSetting("Pricat Media Password")]
      public String Password { get; set; }
    }

    #endregion

    #region Product Attributes

    private MediaProductAttributeStore ProductAttributes { get; set; }

    private Boolean LoadProductAttributes()
    {
      ProductAttributes = new MediaProductAttributeStore(TraceSource);

      return ProductAttributes.Load();
    }

    private class MediaProductAttributeStore : ProductAttributeStore
    {
      public MediaProductAttributeStore(TraceSource traceSource = null)
        : base(traceSource)
      {
      }

      [ProductAttribute("Color_Code")]
      public ProductAttributeMetaData ColorCodeAttribute { get; private set; }
    }

    #endregion

    private static String CombineUri(params String[] segments)
    {
      return String.Join("/", segments.Select(segment => segment.Trim('/', ' ')));
    }

    // Downloads the remote file and copies the same content to the each of the local files specified by local files names
    private Boolean DownloadFile(FtpClient client, String remoteFileName, params String[] localFileNames)
    {
      using (var remoteFileStream = client.DownloadFile(remoteFileName))
      {
        foreach (var localFileName in localFileNames)
        {
          try
          {
            using (var localFileStream = File.Open(localFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
              remoteFileStream.SeekBegin();
              remoteFileStream.CopyTo(localFileStream);
            }
          }
          catch (IOException exception)
          {
            TraceError("Unable to open or create the file '{0}'. {1}", localFileName, exception.Message);

            return false;
          }
        }
      }

      return true;
    }

    private void EnsureLocalDirectory(String directory)
    {
      if (!Directory.Exists(directory))
      {
        Directory.CreateDirectory(directory);
      }
    }

    private Boolean EnsureRemoteDirectories(FtpClient client, params String[] directories)
    {
      foreach (var directory in directories)
      {
        if (!client.Directories.Contains(directory))
        {
          TraceInformation("Creating '{0}'-directory in {1}.", directory, client.BaseUri.AbsolutePath);

          try
          {
            client.CreateDirectory(directory);
          }
          catch (Exception exception)
          {
            TraceError("Unable to create '{0}'-directory in '{1}'. {2}", directory, client.BaseUri.AbsolutePath, exception.Message);

            return false;
          }
        }
      }

      return true;
    }

    protected override void ExecutePricatTask()
    {
      if (!EnsureDirectories())
        return;

      if (LoadMediaTypes() && LoadProductAttributes() && LoadPricatMediaSettings())
      {
        LoadPricatBrandMapping();
        LoadPricatColorMapping();

        var client = new FtpClient(new Uri(PricatMediaSettings.Server), PricatMediaSettings.UserName, PricatMediaSettings.Password);

        client.Update();

        if (EnsureRemoteDirectories(client, ErrorDirectoryName, SuccessDirectoryName))
        {
          //ProcessMediaFiles(client);
          RemoveOldImagesToErrorDirectory(client);

          ProcessMediaDirectories(client);
        }
      }
    }

    //TODO: remove this feature to FtpClient
    private void RemoveOldImagesToErrorDirectory(FtpClient client)
    {
      const int removeImageOlderThenInDays = 14;

      var dateOffset = DateTime.Now.AddDays(-removeImageOlderThenInDays);
      var listOfFiles = client.Files;

      foreach (var file in listOfFiles)
      {
        if (DateTime.Compare(dateOffset, file.CreationTime) > 0)
        {
          var remoteFile = CombineUri(file.FileName);
          var newRemoteFile = CombineUri(ErrorDirectoryName, file.FileName);

          TraceInformation("Moving the remote file '{0}' to the error directory. Image is older then '{1}' days on the ftp.", file.FileName, removeImageOlderThenInDays);

          try
          {
            client.RenameFile(remoteFile, newRemoteFile);
          }
          catch (Exception exception)
          {
            TraceError("Unable to move the remote file '{0}' to the backup directory '{1}'. {2}"
                       , remoteFile
                       , newRemoteFile
                       , exception.Message);
          }
        }
      }
    }

    private bool EnsureDirectories()
    {
      ConcentratorMediaDirectory = ConfigurationManager.AppSettings[ConcentratorMediaDirectoryKey];

      if (ConcentratorMediaDirectory.IsNullOrWhiteSpace())
      {
        TraceError("The application setting '{0}' is unspecified or is invalid.", ConcentratorMediaDirectoryKey);

        return false;
      }
      else if (!Path.IsPathRooted(ConcentratorMediaDirectory) || !Directory.Exists(ConcentratorMediaDirectory))
      {
        TraceError("The application setting '{0}' must be a rooted path to an existing directory.", ConcentratorMediaDirectoryKey);

        return false;
      }

      ConcentratorProductMediaDirectory = ConfigurationManager.AppSettings[ConcentratorProductMediaDirectoryKey];

      if (ConcentratorProductMediaDirectory == null)
      {
        TraceError("The application setting '{0}' is unspecified or is invalid.", ConcentratorProductMediaDirectoryKey);

        return false;
      }
      else if (!ConcentratorProductMediaDirectory.IsNullOrWhiteSpace() && Path.IsPathRooted(ConcentratorProductMediaDirectory))
      {
        TraceError("The application setting '{0}' must be a relative path that will be combined with the application setting '{1}'."
                   , ConcentratorProductMediaDirectoryKey
                   , ConcentratorMediaDirectoryKey);

        return false;
      }

      return true;
    }

    private IEnumerable<Product> GetMatchingProducts(String brandCode, String modelName, String colorCode)
    {
      var brandName = String.Empty;

      if (!PricatBrandMapping.TryGetValue(brandCode, out brandName))
      {
        TraceError("There is no brand with the alias '{0}'.", brandCode);

        return Enumerable.Empty<Product>();
      }

      var vendorItemNumber = String.Format("{0} {1}", modelName, colorCode);

      // Get all products that match the brand, color code attribute and model name
      var products = Unit.Scope
                         .Repository<Product>()
                         .Include(product => product.Brand)
                         .Include(product => product.ProductAttributeValues)
                         .Include(product => product.ProductDescriptions)
                         .Include(product => product.ProductMedias)
                         .Include(product => product.RelatedProductsRelated)
                         .GetAll(product => !product.IsConfigurable && product.Brand.Name == brandName && product.VendorItemNumber == vendorItemNumber)
                         .ToArray();

      if (!products.Any())
      {
        TraceInformation("There are no products with the brand '{0} - {1}'.", brandName, vendorItemNumber);
      }

      return products;
    }
  }
}