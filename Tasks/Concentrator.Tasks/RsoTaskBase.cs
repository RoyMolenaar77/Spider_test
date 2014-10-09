using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using OfficeOpenXml;

using Concentrator.Tasks.Stores;
using Concentrator.Objects.Ftp;
using Concentrator.Tasks.Extensions;

namespace Concentrator.Tasks
{
  using Objects.Ftp;
  using Objects.Models.Localization;
  using Objects.Models.Vendors;
  using Models;

  /// <summary>
  /// A abstract class that contains the logic for reading the Pricat Mapping document.
  /// </summary>
  /// <remarks>
  /// The Pricat Mapping document is an excel document containing a multitude of named worksheet.
  /// The document is used for mapping when importing product data.
  /// Each excel worksheet has a different purpose in the EDI import part.
  /// </remarks>
  [Task("Concentrator Intersport Generic RSO Task")]
  public abstract class RsoTaskBase : TaskBase
  {
    #region Default Language

    protected Language DefaultLanguage
    {
      get;
      private set;
    }

    protected virtual String DefaultLanguageName
    {
      get
      {
        return "Nederlands";
      }
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

    protected Vendor DefaultVendor
    {
      get;
      private set;
    }

    protected virtual String DefaultVendorName
    {
      get
      {
        return "RSO";
      }
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

    #region Pricat Document

    protected ExcelPackage PricatDocument
    {
      get;
      private set;
    }

    private Boolean LoadPricatDocument()
    {
      TraceVerbose("Loading PRICAT document...");

      var client = FtpClientFactory.Create(new Uri(PricatDocumentSettings.Server), PricatDocumentSettings.UserName, PricatDocumentSettings.Password);

      using (var stream = client.DownloadFile(PricatDocumentSettings.FileName))
      {
        try
        {
          PricatDocument = new ExcelPackage(stream);
        }
        catch (Exception exception)
        {
          TraceError("Unable to load the PRICAT document. {0}", exception.Message);

          return false;
        }
      }

      return true;
    }

    #endregion

    #region Pricat Document Settings

    private class PricatDocumentSettingStore : VendorSettingStoreBase
    {
      [VendorSetting("Pricat Document Filename")]
      public String FileName
      {
        get;
        set;
      }

      [VendorSetting("Pricat Document Server")]
      public String Server
      {
        get;
        set;
      }

      [VendorSetting("Pricat Document Username")]
      public String UserName
      {
        get;
        set;
      }

      [VendorSetting("Pricat Document Password")]
      public String Password
      {
        get;
        set;
      }

      public PricatDocumentSettingStore(Vendor vendor, TraceSource traceSource = null)
        : base(vendor, traceSource)
      {
      }
    }

    private PricatDocumentSettingStore PricatDocumentSettings
    {
      get;
      set;
    }

    private Boolean LoadPricatDocumentSettings()
    {
      PricatDocumentSettings = new PricatDocumentSettingStore(DefaultVendor, TraceSource);

      return PricatDocumentSettings.Load();
    }

    #endregion

    #region Pricat Brands

    protected IEnumerable<PricatBrand> PricatBrands
    {
      get;
      private set;
    }

    private const String PricatBrandWorksheetName = "Brands";

    private const Int32 PricatBrandAliasColumnIndex = 2;
    private const Int32 PricatBrandCodeColumnIndex = 3;
    private const Int32 PricatBrandNameColumnIndex = 1;

    protected Boolean LoadPricatBrands()
    {
      var brandWorkSheet = PricatDocument.Workbook.Worksheets.FirstOrDefault(worksheet => worksheet.Name == PricatBrandWorksheetName);

      if (brandWorkSheet == null)
      {
        TraceError("Unable to find the worksheet '{0}' in the PRICAT-document.", PricatBrandWorksheetName);

        return false;
      }

      PricatBrands = Enumerable
        .Range(2, brandWorkSheet.Dimension.End.Row - 1)
        .Select(rowIndex => new PricatBrand
        {
          Alias = brandWorkSheet.GetValue<String>(rowIndex, PricatBrandAliasColumnIndex, String.Empty).Trim(),
          Name = brandWorkSheet.GetValue<String>(rowIndex, PricatBrandNameColumnIndex, String.Empty).Trim(),
          Code = brandWorkSheet.GetValue<String>(rowIndex, PricatBrandCodeColumnIndex, String.Empty).Trim(),
        })
        .Where(item => !item.Alias.IsNullOrEmpty() && !item.Name.IsNullOrEmpty())
        .ToArray();

      return true;
    }

    #endregion

    #region Pricat Colors

    protected IEnumerable<PricatColor> PricatColors
    {
      get;
      private set;
    }

    private const String PricatColorWorksheetName = "Colors";

    private const Int32 PricatColorColorCodeColumnIndex = 1;
    private const Int32 PricatColorDescriptionColumnIndex = 2;
    private const Int32 PricatColorFilterColumnIndex = 3;

    protected Boolean LoadPricatColors()
    {
      var colorWorkSheet = PricatDocument.Workbook.Worksheets.FirstOrDefault(worksheet => worksheet.Name == PricatColorWorksheetName);

      if (colorWorkSheet == null)
      {
        TraceError("Unable to find the worksheet '{0}' in the PRICAT-document.", PricatColorWorksheetName);

        return false;
      }

      PricatColors = Enumerable
        .Range(2, colorWorkSheet.Dimension.End.Row - 1)
        .Select(rowIndex => new PricatColor
        {
          ColorCode = colorWorkSheet.GetValue<String>(rowIndex, PricatColorColorCodeColumnIndex, String.Empty).Trim(),
          Description = colorWorkSheet.GetValue<String>(rowIndex, PricatColorDescriptionColumnIndex, String.Empty).Trim(),
          Filter = colorWorkSheet.GetValue<String>(rowIndex, PricatColorFilterColumnIndex, String.Empty).Trim()
        })
        .Where(item => !item.ColorCode.IsNullOrEmpty()
          && !item.Description.IsNullOrEmpty()
          && !item.Filter.IsNullOrEmpty())
        .ToArray();

      return true;
    }

    #endregion

    #region Pricat Product Groups

    protected IEnumerable<PricatProductGroup> PricatProductGroups
    {
      get;
      private set;
    }

    private const String PricatProductGroupWorksheetName = "Product Groups";
    private const Int32 PricatProductGroupCodeColumnIndex = 1;
    private const Int32 PricatProductGroupDescriptionColumnIndex = 2;

    protected Boolean LoadPricatProductGroups()
    {
      var productGroupWorkSheet = PricatDocument.Workbook.Worksheets.FirstOrDefault(worksheet => worksheet.Name == PricatProductGroupWorksheetName);

      if (productGroupWorkSheet == null)
      {
        TraceError("Unable to find the worksheet '{0}' in the PRICAT-document.", PricatProductGroupWorksheetName);

        return false;
      }

      PricatProductGroups = Enumerable
        .Range(2, productGroupWorkSheet.Dimension.End.Row - 1)
        .Select(rowIndex => new PricatProductGroup
        {
          Code = productGroupWorkSheet.GetValue<String>(rowIndex, PricatProductGroupCodeColumnIndex, String.Empty).Trim(),
          Description = productGroupWorkSheet.GetValue<String>(rowIndex, PricatProductGroupDescriptionColumnIndex, String.Empty).Trim(),
        })
        .Where(pricatProductGroup => !pricatProductGroup.Code.IsNullOrWhiteSpace())
        .ToArray();

      return true;
    }

    #endregion

    #region Pricat Sizes

    protected IEnumerable<PricatSize> PricatSizes
    {
      get;
      private set;
    }

    private const String PricatSizesWorksheetName = "Sizes";
    private const Int32 PricatSizesBrandColumnIndex = 1;
    private const Int32 PricatSizesModelColumnIndex = 2;
    private const Int32 PricatSizesGroupColumnIndex = 3;
    private const Int32 PricatSizesFromColumnIndex = 4;
    private const Int32 PricatSizesToColumnIndex = 5;

    protected Boolean LoadPricatSizes()
    {
      var sizeWorkSheet = PricatDocument.Workbook.Worksheets.FirstOrDefault(worksheet => worksheet.Name == PricatSizesWorksheetName);

      if (sizeWorkSheet == null)
      {
        TraceError("Unable to find the worksheet '{0}' in the PRICAT-document.", PricatSizesWorksheetName);

        return false;
      }

      PricatSizes = Enumerable
        .Range(2, sizeWorkSheet.Dimension.End.Row - 1)
        .Select(rowIndex => new PricatSize
        {
          BrandName = sizeWorkSheet.GetValue<String>(rowIndex, PricatSizesBrandColumnIndex, String.Empty).Trim(),
          ModelName = sizeWorkSheet.GetValue<String>(rowIndex, PricatSizesModelColumnIndex, String.Empty).Trim().TrimStart('0'),
          GroupCode = sizeWorkSheet.GetValue<String>(rowIndex, PricatSizesGroupColumnIndex, String.Empty).Trim().TrimStart('0'),
          From = sizeWorkSheet.GetValue<String>(rowIndex, PricatSizesFromColumnIndex, String.Empty).Trim(),
          To = sizeWorkSheet.GetValue<String>(rowIndex, PricatSizesToColumnIndex, String.Empty).Trim()
        })
        .Where(item => !item.BrandName.IsNullOrEmpty()
          && !item.From.IsNullOrEmpty()
          && !item.To.IsNullOrEmpty())
        .ToArray();

      return true;
    }

    #endregion

    #region Pricat Vendors

    protected IEnumerable<PricatVendor> PricatVendors
    {
      get;
      private set;
    }

    private const String PricatVendorsWorksheetName = "Vendors";

    private const Int32 PricatVendorsAliasColumnIndex = 3;
    private const Int32 PricatVendorsBackendIDColumnIndex = 1;
    private const Int32 PricatVendorsBarcodeColumnIndex = 4;
    private const Int32 PricatVendorsNameColumnIndex = 2;

    protected Boolean LoadPricatVendors()
    {
      var vendorWorkSheet = PricatDocument.Workbook.Worksheets.FirstOrDefault(worksheet => worksheet.Name == PricatVendorsWorksheetName);

      if (vendorWorkSheet == null)
      {
        TraceError("Unable to find the worksheet '{0}' in the PRICAT-document.", PricatVendorsWorksheetName);

        return false;
      }

      PricatVendors = Enumerable
        .Range(2, vendorWorkSheet.Dimension.End.Row - 1)
        .Select(rowIndex => new PricatVendor
        {
          Alias = vendorWorkSheet.GetValue<String>(rowIndex, PricatVendorsAliasColumnIndex, String.Empty).Trim(),
          BackendID = vendorWorkSheet.GetValue<String>(rowIndex, PricatVendorsBackendIDColumnIndex, String.Empty).Trim(),
          Barcode = vendorWorkSheet.GetValue<String>(rowIndex, PricatVendorsBarcodeColumnIndex, String.Empty).Trim(),
          Name = vendorWorkSheet.GetValue<String>(rowIndex, PricatVendorsNameColumnIndex, String.Empty).Trim()
        })
        .Where(item => !item.Alias.IsNullOrEmpty()
          && !item.BackendID.IsNullOrEmpty()
          && !item.Barcode.IsNullOrEmpty()
          && !item.Name.IsNullOrEmpty())
        .ToArray();

      return true;
    }

    #endregion

    protected override void ExecuteTask()
    {
      if (LoadDefaultVendor() && LoadDefaultLanguage() && LoadPricatDocumentSettings() && LoadPricatDocument())
      {
        using (PricatDocument)
        {
          if (LoadPricatBrands() && LoadPricatColors() && LoadPricatSizes() && LoadPricatVendors() && LoadPricatProductGroups())
          {
            ExecutePricatTask();
          }
        }
      }
    }

    protected abstract void ExecutePricatTask();
  }
}