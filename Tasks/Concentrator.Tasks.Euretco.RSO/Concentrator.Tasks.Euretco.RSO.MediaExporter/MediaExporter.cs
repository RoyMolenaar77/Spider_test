#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Media;
using Concentrator.Objects.Utility;
using Concentrator.Objects.Utility.TransferServices.Models;
using Concentrator.Tasks.Euretco.RSO.MediaExporter.Models;

#endregion

namespace Concentrator.Tasks.Euretco.RSO.MediaExporter
{
  [Task("Product Media exporter")]
  public sealed class MediaExporter : ConnectorTaskBase
  {
    #region Variables

    private const String ImageMediaTypeName = "Image";
    private const String Image360MediaTypeName = "Image_360";
    [Resource]
    private static readonly String SelectProductMediaForConnector = null;

    private MediaType ImageMediaType { get; set; }
    private MediaType Image360MediaType { get; set; }

    #endregion

    #region Additional commandline parameters

    /// <summary>
    /// 0 = partial export, 1 = full export 
    /// </summary>
    [CommandLineParameter("/FullExport", "/FE")]
    internal static Boolean EnableFullExport { get; set; }

    /// <summary>
    /// Gets a collection of product ID's that where explicitily supplied via the command-line.
    /// </summary>
    [CommandLineParameter("/Products", "/Product", "/P")]
    internal static Int32[] ProductIDs { get; set; }

    #endregion

    #region Connector settings

    [ConnectorSetting("Fingerprint archive file")]
    private String FingerprintArchiveFile { get; set; }

    [ConnectorSetting("MediaLocationBasePath")]
    private String MediaLocationBasePath { get; set; }

    [ConnectorSetting("ISM Export Server")]
    private String ExportServer { get; set; }

    [ConnectorSetting("ISM Export Username")]
    private String ExportUser { get; set; }

    [ConnectorSetting("ISM Export CertPath")]
    private String ExportCertPath { get; set; }

    [ConnectorSetting("ISM Export Path")]
    private String ExportPath { get; set; }

    #endregion

    /// <summary>
    ///   Loading supported media typed: Next types are supported: (2)Image and (3)Image_360.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    ///   Overriding the Filename for RSO to support 360 sequences and the requested format of filename:
    ///   Configured: [SKU_SEQ_jpg]
    ///   Simple: [Barcode_SEQ_jpg]
    /// </summary>
    /// <param name="media"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    private string GenerateFileName(ProductMediaModel media, string path = "")
    {
      if (media.TypeID != ImageMediaType.TypeID && media.TypeID != Image360MediaType.TypeID)
      {
        TraceWarning("Image media type is not supported: {0} for product {1}", media.TypeID, media.ProductID);
        return string.Empty;
      }

      var sequence = media.Sequence;
      if (media.TypeID == Image360MediaType.TypeID)
        sequence = sequence + 200;

      if (media.IsConfigurable)
        return string.Format(@"{0}/{1}_{2}.jpg", ExportPath, media.VendorItemNumber.Replace(" ", "_"), sequence);

      return string.Format(@"{0}/{1}_{2}.jpg", ExportPath, media.Barcode, sequence);
    }

    private IEnumerable<ProductMediaModel> FilterProductImages(List<ProductMediaModel> productMediaModels)
    {
      var result = new List<ProductMediaModel>();

      var contentVendorSettings = Unit
        .Scope
        .Repository<ContentVendorSetting>()
        .GetAll(x => x.ConnectorID == Context.ConnectorID)
        .OrderBy(v => v.ContentVendorIndex)
        .ToList();

      var prefferedVendor = contentVendorSettings.FirstOrDefault();

      if (prefferedVendor != null)
      {
        result = productMediaModels
          .Where(p => p.VendorID == prefferedVendor.VendorID)
          .ToList();

        for (var i = 1; i < contentVendorSettings.Count(); i++)
        {
          var listOfProductMedia = productMediaModels
            .Where(p => p.VendorID == contentVendorSettings[i].VendorID && result.FindIndex(r => r.ProductID == p.ProductID) == -1)
            .ToArray();

          result.AddRange(listOfProductMedia);
        }

      }
      return result;
    }

    private IEnumerable<ProductMediaModel> GetAllProductImages()
    {
      return Database.Query<ProductMediaModel>(SelectProductMediaForConnector, Context.ConnectorID).ToArray();
    }

    private Boolean GenerateTransferFileModel(ProductMediaModel productMediaModel, out TransferResourceModel transferResourceModel)
    {
      transferResourceModel = new TransferResourceModel();

      if (productMediaModel == null)
      {
        TraceError("Unable to setup TranferResourceModel. ProductMediaModel is NUll or empty.");
        return false;
      }

      var fileName = GenerateFileName(productMediaModel);
      if (fileName.IsNullOrWhiteSpace())
      {
        TraceError("Unable to setup TranferResourceModel. Remote fileName cannot be setup.");
        return false;
      }

      try
      {
        var source =
          new Uri(
            string.Format(@"file:///{0}?productId={1}&sequence={2}",
              Path.Combine(MediaLocationBasePath, productMediaModel.MediaPath),
              productMediaModel.ProductID,
              productMediaModel.Sequence),
            true);

        var destination = new Uri(
          string.Format(@"sftp://{0}@{1}/{2}?privateKeyFile={3}", ExportUser, ExportServer.Replace("sftp://", ""), fileName, ExportCertPath), true);

        transferResourceModel.Source = source;
        transferResourceModel.Destination = destination;
      }
      catch (Exception e)
      {
        TraceError("Error setting up TranferResourceModel for product # {1}. Error: {0}", e.Message, productMediaModel.ProductID);
        return false;
      }

      return true;
    }

    private IEnumerable<TransferResourceModel> GetTransferResources()
    {
      var mediaFiles = GetFilteredMedia();

      if (mediaFiles == null)
      {
        TraceError("No media found for export on connector {0}", Context.ConnectorID);
        return null;
      }

      var listOfProductMediaFiles = mediaFiles as ProductMediaModel[] ?? mediaFiles.ToArray();

      TraceInformation("Found {0} Media Files.", listOfProductMediaFiles.Length);

      if (ProductIDs.Any())
      {
        listOfProductMediaFiles = listOfProductMediaFiles
          .Where(x => ProductIDs.Contains(x.ProductID))
          .ToArray();
      }

      var transferFiles = new List<TransferResourceModel>();

      foreach (var media in listOfProductMediaFiles)
      {
        TransferResourceModel transferResourceModel;

        if (GenerateTransferFileModel(media, out transferResourceModel))
        {
          transferFiles.Add(transferResourceModel);
        }
      }

      if (transferFiles.Count == 0)
      {
        TraceWarning("There are no transfer files to send for connector {0}", Context.ConnectorID);
        return null;
      }

      return transferFiles;
    }

    private IEnumerable<ProductMediaModel> GetFilteredMedia()
    {
      var mediaSelection = GetAllProductImages();

      var filteredMediaselection = FilterProductImages(mediaSelection.ToList());

      return filteredMediaselection;
    }

    protected override void ExecuteConnectorTask()
    {
      TraceVerbose("Processing export media files");

      EmbeddedResourceHelper.Bind(this);

      if (LoadMediaTypes())
      {
        var transferResources = GetTransferResources();

        if (transferResources != null)
        {
          if (EnableFullExport)
          {
            TraceInformation("Start full export for {0} products.", transferResources.Count());

            TransferService
              .Default
              .Process(
                transferResources,
                TraceSource,
                new
                {
                  FingerprintArchiveFile
                });
          }
          else
          {
            TraceInformation("Start partial export for {0} products.", transferResources.Count());

            TransferService
              .Partial
              .Process(
                transferResources,
                TraceSource,
                new
                {
                  FingerprintArchiveFile
                });
          }
        }
      }

      TraceVerbose("Ended export media files");
    }
  }
}