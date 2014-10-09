#region Usings

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Concentrator.Objects.Archiving;
using Concentrator.Objects.Ftp;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Vendors.Bulk;
using Concentrator.Tasks.Euretco.Rso.EDI.Factory;
using Concentrator.Tasks.Euretco.Rso.EDI.Models;
using Concentrator.Tasks.Euretco.Rso.EDI.Processor;
using Concentrator.Tasks.Models;
using Concentrator.Tasks.Stores;

#endregion

namespace Concentrator.Tasks.Euretco.Rso.EDI.Importers
{
  [Task("EDI Product Importer")]
  public sealed class PricatImporter : RsoTaskBase
  {
    private const String ErrorDirectoryName = "Error";
    private const String SuccessDirectoryName = "Success";

    // This expression removes the outer double quotes from character-sequences
    private static readonly Regex TrimOuterQuotesRegex = new Regex("(^\"|\"$)", RegexOptions.Compiled);

    private IPricatLineProcessor _pricatLineProcessor;

    #region Product Attributes

    private PricatProductAttributeStore ProductAttributes { get; set; }

    private Boolean LoadProductAttributes()
    {
      ProductAttributes = new PricatProductAttributeStore(TraceSource);

      return ProductAttributes.Load();
    }

    #endregion

    #region Pricat Color Mapping

    private IDictionary<String, String> PricatColorMapping { get; set; }

    private void LoadPricatColorMapping()
    {
      TraceVerbose("Loading PRICAT color mappings...");

      PricatColorMapping = PricatColors.ToDictionary(
        pricatColor => pricatColor.ColorCode.PadLeft(3, '0'),
        pricatColor => pricatColor.Filter,
        StringComparer.CurrentCultureIgnoreCase);
    }

    #endregion

    #region Pricat Size

    // TODO make this a helper function to fix current functional redundancy
    private String TranslateSize(String brand, String model, String group, String size)
    {
      var stringComparer = StringComparison.OrdinalIgnoreCase;

      model = model.TrimStart('0');
      group = group.TrimStart('0');

      var pricatSizes = PricatSizes
        .Where(record => record.BrandName.Equals(brand, stringComparer) && record.From.Equals(size, stringComparer))
        .Where(record => record.ModelName.Equals(model, stringComparer) || record.ModelName.Equals(String.Empty, stringComparer))
        .Where(record => record.GroupCode.Equals(group, stringComparer) || record.GroupCode.Equals(String.Empty, stringComparer))
        .ToArray();

      return pricatSizes.Select(pricatSize => pricatSize.To).FirstOrDefault();
    }

    #endregion

    #region Pricat Vendor Translation

    private IDictionary<String, Vendor> PricatVendorMappings { get; set; }

    private void LoadPricatVendorMapping()
    {
      TraceVerbose("Loading PRICAT vendor mapping...");

      PricatVendorMappings = new Dictionary<String, Vendor>(StringComparer.OrdinalIgnoreCase);

      var vendors = Unit.Scope
                        .Repository<Vendor>()
                        .GetAll()
                        .ToArray();

      foreach (var pricatVendor in PricatVendors)
      {
        var vendor = vendors.FirstOrDefault(v => String.Equals(v.Name, pricatVendor.Name, StringComparison.OrdinalIgnoreCase));

        if (vendor != null)
        {
          PricatVendorMappings[pricatVendor.Barcode] = vendor;
        }
        else
        {
          TraceWarning("Unable to find a vendor with the name '{0}'.", pricatVendor.Name);
        }
      }
    }

    #endregion

    #region Pricat Settings

    private PricatSettingStore PricatSettings { get; set; }

    private Boolean LoadPricatSettings()
    {
      PricatSettings = new PricatSettingStore(DefaultVendor, TraceSource);

      return PricatSettings.Load();
    }

    private class PricatSettingStore : VendorSettingStoreBase
    {
      public PricatSettingStore(Vendor vendor, TraceSource traceSource = null)
        : base(vendor, traceSource)
      {
      }

      private string _server;

      [VendorSetting("Pricat Import Server")]
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

      [VendorSetting("Pricat Import Username")]
      public String UserName { get; set; }

      [VendorSetting("Pricat Import Password")]
      public String Password { get; set; }

      // This actually does not have to be a vendor setting, could have a wider scope....
      [VendorSetting("Pricat Archive Root")]
      public String ArchiveRoot { get; set; }

      [VendorSetting("Pricat Document Archive")]
      public String VendorArchive { get; set; }
    }

    #endregion

    #region Variables

    private static string _currentRemoteFileName;

    #endregion

    protected override void ExecutePricatTask()
    {
      _pricatLineProcessor = PricatLineProcessorFactory.Create();

      if (LoadProductAttributes() &&
          LoadPricatSettings())
      {
        LoadPricatColorMapping();

        var client = FtpClientFactory.Create(new Uri(PricatSettings.Server), PricatSettings.UserName, PricatSettings.Password);
        ISimpleArchiveService archiver = new SimpleArchiveService(PricatSettings.ArchiveRoot, TraceSource);

        client.Update();

        ProcessPricatFiles(client, archiver);
      }
    }

    private void ProcessPricatFiles(IFtpClient client, ISimpleArchiveService archiver)
    {
      var successArchive = Path.Combine(PricatSettings.VendorArchive, SuccessDirectoryName);
      var errorArchive = Path.Combine(PricatSettings.VendorArchive, ErrorDirectoryName);

      var sessionStart = DateTime.Now;

      foreach (var remoteFile in client.Files.Where(file => file.FileName.EndsWith(".csv", StringComparison.CurrentCultureIgnoreCase)))
      {
        var remoteFileName = remoteFile.FileName;

        using (var fileStream = client.DownloadFile(remoteFileName))
        {
          TraceVerbose("Processing file '{0}'...", remoteFileName);

          var lines = ReadRawPricatLines(fileStream);

          _currentRemoteFileName = remoteFileName;

          var importSuccess = ProcessPricatFile(lines);
          var intendedArchive = importSuccess ? successArchive : errorArchive;

          if (archiver.ArchiveLines(lines, intendedArchive, sessionStart, remoteFileName))
          {
            if (intendedArchive == successArchive)
            {
              TraceInformation("The PRICAT-file '{0}' has been archived to the '{1}'-folder.", remoteFileName, intendedArchive);
            }
            else
            {
              TraceWarning("The PRICAT-file '{0}' has been archived to the '{1}'-folder.", remoteFileName, intendedArchive);
            }
          }
          else
          {
            TraceError("Failed to archive the PRICAT-file '{0}' into the '{1}'-folder.", remoteFileName, intendedArchive);
          }

          //Move file on the FTP
          if (!client.TryMoveFile(remoteFileName, importSuccess ? SuccessDirectoryName : ErrorDirectoryName))
            TraceError("Moving file '{0}' to the folder '{1}' on the FTP failed.", remoteFileName, importSuccess ? SuccessDirectoryName : ErrorDirectoryName);
        }
      }

      //TraceVerbose("Generating product attribute configuration...");

      // DS: Inactive as for 2014-07-15
      //Database.Execute("EXEC [GenerateProductAttributeConfiguration] @0", "Color, Size, Subsize");    //TODO: SP missing in database
    }

    private Boolean ProcessPricatFile(List<string> lines)
    {
      //TODO: use filehelper engine or another CSV processor here....

      var pricatEnvelop = PricatEnvelop.Parse(lines.First(line => line.StartsWith("\"ENV\"")));
      var pricatHeader = PricatHeader.Parse(lines.First(line => line.StartsWith("\"HDR\"")));
      var pricatLines = lines
        .Where(line => line.StartsWith("\"LIN\""))
        .Select(PricatLine.Parse);

      return ProcessPricatLines(DefaultVendor, pricatLines);
    }

    private List<string> ReadRawPricatLines(Stream fileStream)
    {
      var lines = new List<string>();
      using (var fileReader = new StreamReader(fileStream, Encoding.UTF7))
      {
        lines = fileReader
          .ReadToEnd()
          .Split(Environment.NewLine).ToList();
      }
      return lines;
    }

    private Boolean ProcessPricatLines(Vendor vendor, IEnumerable<PricatLine> pricatLines)
    {
      var noneMappableSizes = pricatLines
        .Select(pricatLine => new
          {
            BrandCode = pricatLine.Brand,
            ModelName = pricatLine.Model,
            GroupCode = pricatLine.ArticleGroupCode,
            Size = pricatLine.SizeSupplier
          })
        .Distinct()
        .Where(item => TranslateSize(item.BrandCode, item.ModelName, item.GroupCode, item.Size) == null)
        .ToArray();

      if (noneMappableSizes.Any())
      {
        foreach (var noneMappableSize in noneMappableSizes)
        {
          TraceWarning("The combination Brand: '{0}', Model: '{1}' (or empty), Group: '{2}' (or empty), Size: '{3}' does not exists."
                       , noneMappableSize.BrandCode
                       , noneMappableSize.ModelName
                       , noneMappableSize.GroupCode
                       , noneMappableSize.Size);
        }

        return false;
      }

      var noneMappableColors = pricatLines
        .Select(pricatLine => pricatLine.ColorCode)
        .Distinct()
        .Where(colorCode => !PricatColorMapping.ContainsKey(colorCode))
        .ToArray();

      if (noneMappableColors.Any())
      {
        foreach (var noneMappableColor in noneMappableColors)
        {
          TraceWarning("The color code '{0}' is not mapped to color filter.", noneMappableColor);
        }

        return false;
      }

      var existBarcodes = (
                            from pricat in pricatLines
                            join currentBarcode in Unit.Scope.Repository<ProductBarcode>().GetAll()
                              on pricat.ArticleID equals currentBarcode.Barcode
                            where String.Join(" ", pricat.SupplierCode
                                              , pricat.ColorCode
                                              ,
                                              _pricatLineProcessor.TranslateSize(PricatSizes, pricat.Brand, pricat.Model, pricat.ArticleGroupCode, pricat.SizeSupplier)
                                                                 .Trim()) != currentBarcode.Product.VendorItemNumber
                            select new
                              {
                                pricatBarcode = pricat.ArticleID,
                                pricatSku = String.Join(" ", pricat.SupplierCode
                                                        , pricat.ColorCode
                                                        ,
                                                        _pricatLineProcessor.TranslateSize(PricatSizes, pricat.Brand, pricat.Model, pricat.ArticleGroupCode,
                                                                                          pricat.SizeSupplier).Trim())
                                ,
                                currentSku = currentBarcode.Product.VendorItemNumber
                              }
                          )
        .ToArray();

      if (existBarcodes.Any())
      {
        foreach (var item in existBarcodes)
        {
          TraceWarning(
            "Error during processing Pricat File, Barcode '{0}' mapped to sku '{1}' in file '{2}' already exists within the Concentrator. It is currently mapped to sku '{3}'"
            , item.pricatBarcode
            , item.pricatSku
            , _currentRemoteFileName
            , item.currentSku);
        }

        return false;
      }

      var vendorAssortmentItems = new List<VendorAssortmentBulk.VendorAssortmentItem>();

      foreach (var pricatLineGroup in pricatLines
        .GroupBy(pricatLine => String.Join(" ", pricatLine.SupplierCode, pricatLine.ColorCode))
        .Where(pricatLineGroup => pricatLineGroup.Any()))
      {
        var productGroupCode = pricatLineGroup.Select(pricatLine => pricatLine.ArticleGroupCode).First();

        foreach (
          var vendorAssortmentItem in
            _pricatLineProcessor.ProcessPricatGroupedLines(Unit, TraceSource, vendor, pricatLineGroup.Key, DefaultLanguage,
                                                          pricatLineGroup.OrderBy(line => line.Number),
                                                          ProductAttributes, PricatColorMapping, PricatSizes))
        {
          vendorAssortmentItems.Add(vendorAssortmentItem);
        }
      }

      TraceVerbose("Bulk importing {0} products...", vendorAssortmentItems.Count);

      try
      {
        var bulkConfig = new VendorAssortmentBulkConfiguration { IsPartialAssortment = true };
        using (var vendorAssortmentBulk = new VendorAssortmentBulk(vendorAssortmentItems, vendor.VendorID, DefaultVendor.VendorID, bulkConfig))
        {
          vendorAssortmentBulk.Init(Unit.Context);
          vendorAssortmentBulk.Sync(Unit.Context);
        }
      }
      catch (SqlException exception)
      {
        TraceError("Failed to bulk import the products. {0}", exception.Message);

        return false;
      }
      catch (Exception exception)
      {
        TraceError("Failed to bulk import the products. {0}", exception.Message);

        return false;
      }

      return true;
    }

    //private Boolean MoveRemoteFile(IFtpClient client, String remoteFileName, String destinationDirectory)
    //{
    //  try
    //  {
    //    // TODO: to be replaced with a move of processed data to a local archive, absence of a file in an archive should trigger processing of the remote file
    //    //client.RenameFile(remoteFileName, destinationDirectory + Path.AltDirectorySeparatorChar + remoteFileName);

    //    return true;
    //  }
    //  catch (WebException exception)
    //  {
    //    TraceError("Unable to move the remote file '{0}' from '{1}' to '{2}'. {3}"
    //      , client.BaseUri.AbsolutePath
    //      , client.BaseUri.AbsolutePath + Path.AltDirectorySeparatorChar + destinationDirectory
    //      , remoteFileName
    //      , exception.Message);

    //    return false;
    //  }
    //}
  }
}