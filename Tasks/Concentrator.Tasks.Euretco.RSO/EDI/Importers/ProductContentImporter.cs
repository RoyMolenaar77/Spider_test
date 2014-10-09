#region Usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Concentrator.Objects.Ftp;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Vendors.Bulk;
using Concentrator.Tasks.Euretco.Rso;
using Concentrator.Tasks.Euretco.Rso.EDI;
using Concentrator.Tasks.Stores;
using OfficeOpenXml;

#endregion

namespace Concentrator.Tasks.Euretco.RSO.EDI.Importers
{
  [Task("EDI Product Content Importer")]
  public sealed partial class ProductContentImporter : RsoTaskBase
  {
    #region Product Content Constants & Types

    private const String HeaderName = "Name";
    private const String HeaderColor = "Kleur";

    private readonly String[] _requiredHeaders = new[]
      {
        HeaderName,
        HeaderColor
      };

    private class ProductContentRecord : Dictionary<String, String>
    {
      public ProductContentRecord(IDictionary<String, String> dictionary)
        : base(dictionary, StringComparer.CurrentCultureIgnoreCase)
      {
      }

      public String this[String key, String defaultValue = null]
      {
        get
        {
          string value;

          return TryGetValue(key, out value)
                   ? value
                   : defaultValue;
        }
      }
    }

    /// <summary>
    ///   Derive this class to support one or more columns in the product content documents.
    /// </summary>
    private abstract class ProductContentRecordProcessor
    {
      public String CustomItemNumber { get; set; }

      public Int32 LanguageID { get; set; }

      public Int32 VendorID { get; set; }

      public VendorAssortmentBulk.VendorAssortmentItem VendorAssortment { get; set; }

      public abstract void Process(ProductContentRecord record);
    }

    #endregion

    #region Product Content Settings

    private ProductContentSettingStore ProductContentSettings { get; set; }

    private Boolean LoadProductContentSettings()
    {
      ProductContentSettings = new ProductContentSettingStore(DefaultVendor, TraceSource);

      return ProductContentSettings.Load();
    }

    private class ProductContentSettingStore : VendorSettingStoreBase
    {
      public ProductContentSettingStore(Vendor vendor, TraceSource traceSource = null)
        : base(vendor, traceSource)
      {
      }

      private string _server;

      [VendorSetting("Product Content Server")]
      public String Server {
        get
        {
#if DEBUG
          _server = _server.Replace("Staging", "Test");
#endif
          return _server;
        }
        set { _server = value; }
      }

      [VendorSetting("Product Content Username")]
      public String UserName { get; set; }

      [VendorSetting("Product Content Password")]
      public String Password { get; set; }
    }

    #endregion

    private const String ErrorDirectoryName = "Error";
    private const String SuccessDirectoryName = "Success";

    private void EnsureFtpDirectories(FtpClient client)
    {
      client.Update();

      if (!client.Directories.Contains(ErrorDirectoryName))
      {
        client.CreateDirectory(ErrorDirectoryName);
      }

      if (!client.Directories.Contains(SuccessDirectoryName))
      {
        client.CreateDirectory(SuccessDirectoryName);
      }
    }

    protected override void ExecutePricatTask()
    {
      if (LoadProductContentSettings())
      {
        var client = new FtpClient(new Uri(ProductContentSettings.Server), ProductContentSettings.UserName, ProductContentSettings.Password);

        EnsureFtpDirectories(client);
        ProcessFiles(client);
      }
    }

    private IEnumerable<ProductContentRecord> GetProductContentRecords(ExcelWorksheet worksheet)
    {
      var headers = Enumerable
        .Range(1, worksheet.Dimension.End.Column + 1)
        .Select(columnIndex => (worksheet.GetValue<String>(1, columnIndex) ?? String.Empty).Trim())
        .Where(header => !header.IsNullOrWhiteSpace())
        .Distinct()
        .ToArray();

      for (var rowIndex = 2; rowIndex <= worksheet.Dimension.End.Row; rowIndex++)
      {
        var dictionary = Enumerable
          .Range(1, headers.Length)
          .ToDictionary
          (
            columnIndex => headers[columnIndex - 1],
            columnIndex => (worksheet.GetValue<String>(rowIndex, columnIndex) ?? String.Empty).Trim()
          );

        string sku;
        string colorCode;

        if (dictionary.TryGetValue(HeaderName, out sku)
            && !sku.IsNullOrWhiteSpace()
            && dictionary.TryGetValue(HeaderColor, out colorCode)
            && !colorCode.IsNullOrWhiteSpace())
        {
          yield return new ProductContentRecord(dictionary);
        }
      }
    }

    private Boolean ProcessFile(Stream fileStream)
    {
      using (var document = new ExcelPackage(fileStream))
      {
        var worksheet = document.Workbook.Worksheets.First();

        if (VerifyWorksheet(worksheet))
        {
          TraceVerbose("Loading existing product vendor assortment data...");

          var vendorBySkuLookup = Unit.Scope.Repository<VendorAssortment>()
                                      .Include(vendorAssortment => vendorAssortment.Product)
                                      .GetAll(vendorAssortment => vendorAssortment.Product.IsConfigurable)
                                      .ToLookup(vendorAssortment => vendorAssortment.CustomItemNumber, vendorAssortment => vendorAssortment.VendorID);

          TraceVerbose("Loading product content processors...");

          var productContentRecordProcessors = GetType()
            .GetNestedTypes(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(type => type.BaseType == typeof (ProductContentRecordProcessor))
            .Select(processorType => Activator.CreateInstance(processorType, DefaultVendor) as ProductContentRecordProcessor)
            .ToArray();

          TraceVerbose("Loading product content records...");

          var productContentRecords = GetProductContentRecords(worksheet).ToArray();
          var vendorAssortmentDictionary = new Dictionary<Int32, IList<VendorAssortmentBulk.VendorAssortmentItem>>();
          var progressStopwatch = Stopwatch.StartNew();

          for (var index = 0; index < productContentRecords.Length; index++)
          {
            var productContentRecord = productContentRecords[index];
            var customItemNumber = String.Format("{0} {1}", productContentRecord[HeaderName].Substring(3), productContentRecord[HeaderColor]);

            foreach (var vendorID in vendorBySkuLookup[customItemNumber])
            {
              var vendorAssortmentItem = new VendorAssortmentBulk.VendorAssortmentItem();

              foreach (var productContentProcessor in productContentRecordProcessors)
              {
                productContentProcessor.CustomItemNumber = customItemNumber;
                productContentProcessor.LanguageID = DefaultLanguage.LanguageID;
                productContentProcessor.VendorAssortment = vendorAssortmentItem;
                productContentProcessor.VendorID = vendorID;
                productContentProcessor.Process(productContentRecord);
              }

              if (!vendorAssortmentDictionary.ContainsKey(vendorID))
              {
                vendorAssortmentDictionary[vendorID] = new List<VendorAssortmentBulk.VendorAssortmentItem>();
              }

              vendorAssortmentDictionary[vendorID].Add(vendorAssortmentItem);
            }

            if (progressStopwatch.ElapsedMilliseconds > 1000L || productContentRecords.Length - 1 == index)
            {
              TraceVerbose("Progress: {0:P2}", (index + 1D)/productContentRecords.Length);
              progressStopwatch.Restart();
            }
          }

          foreach (var vendorID in vendorAssortmentDictionary.Keys)
          {
            TraceVerbose("Bulk importing vendor '{0}'...", Unit.Scope.Repository<Vendor>().GetSingle(vendor => vendor.VendorID == vendorID).Name);

            var bulkConfig = new VendorAssortmentBulkConfiguration {IsPartialAssortment = true};

            using (var vendorAssortmentBulk = new VendorAssortmentBulk(vendorAssortmentDictionary[vendorID], vendorID, vendorID, bulkConfig))
            {
              vendorAssortmentBulk.Init(Unit.Context);
              vendorAssortmentBulk.Sync(Unit.Context);
            }
          }

          return true;
        }
      }

      return false;
    }

    private void ProcessFiles(FtpClient client)
    {
      foreach (var remoteFile in client.Files.Where(file => file.FileName.EndsWith(".xlsx", StringComparison.CurrentCultureIgnoreCase)))
      {
        var remoteFileName = remoteFile.FileName;

        using (var fileStream = client.DownloadFile(remoteFileName))
        {
          TraceVerbose("Processing file '{0}'...", remoteFileName);

          if (ProcessFile(fileStream))
          {
            if (client.TryMoveFile(remoteFileName, SuccessDirectoryName))
            {
              TraceVerbose("The file '{0}' has been moved to '{1}'-folder.", remoteFileName, SuccessDirectoryName);
            }
          }
          else
          {
            TraceError("The import of the document '{0}' failed.", remoteFileName);

            if (client.TryMoveFile(remoteFileName, ErrorDirectoryName))
            {
              TraceInformation("The file '{0}' has been moved to '{1}'-folder.", remoteFileName, ErrorDirectoryName);
            }
          }
        }
      }
    }

    private Boolean VerifyWorksheet(ExcelWorksheet worksheet)
    {
      if (worksheet.Dimension.End.Row <= 1)
      {
        TraceWarning("Worksheet '{0}' is invalid, atleast the one row is expected with column headers.", worksheet.Name);

        return false;
      }

      var headers = Enumerable
        .Range(1, worksheet.Dimension.End.Column + 1)
        .Select(columnIndex => (worksheet.GetValue<String>(1, columnIndex) ?? String.Empty).Trim())
        .Where(header => !header.IsNullOrWhiteSpace())
        .ToArray();

      var missingHeaders = _requiredHeaders.Except(headers, StringComparer.OrdinalIgnoreCase).ToArray();

      if (missingHeaders.Any())
      {
        TraceError("The import document does not contain all required headers. The missing headers are: {0}.", String.Join(", ", missingHeaders));

        return false;
      }

      return true;
    }
  }
}