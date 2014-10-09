#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Concentrator.Objects.Environments;
using Concentrator.Tasks.Euretco.Rso.ProductContentSync.Constants;
using Concentrator.Tasks.Euretco.Rso.ProductContentSync.Models;
using Concentrator.Tasks.Euretco.Rso.ProductContentSync.Repositories;
using PetaPoco;

#endregion

namespace Concentrator.Tasks.Euretco.Rso.ProductContentSync.Synchronizer
{
  [Task("Product Media Synchronizer")]
  public class ProductMediaSynchronizer : ConnectorTaskBase
  {
    [Resource] private static readonly string GetVendorByNameQuery = null;

    [ConnectorSetting("MediaSourceDatabaseName")]
    public string SourceDatabaseName { get; set; }

    [ConnectorSetting("MediaDestinationDatabaseName")]
    public string DestinationDatabaseName { get; set; }

    [ConnectorSetting("MediaSourceLocation")]
    public string MediaSourceLocation { get; set; }

    [ConnectorSetting("MediaLocationBasePath")]
    public string MediaLocationBasePath { get; set; }

    [ConnectorSetting("MediaLocationOrganisationPath")]
    public string MediaLocationOrganisationPath { get; set; }

    public ProductMediaRepository ProductMediaRepository { get; set; }

    public Dictionary<int, int> ProductMediaTypeIDLookup { get; set; }

    public int IntersportContentVendorID { get; set; }

    protected override void ExecuteConnectorTask()
    {
      EmbeddedResourceHelper.Bind(GetType());
      Init();

      var productToProcess = ProductMediaRepository.GetProductsForSync();

      foreach (var productToSync in productToProcess)
      {
        SyncMedia(productToSync);
      }
    }

    private void Init()
    {
      ProductMediaRepository = new ProductMediaRepository(Database, SourceDatabaseName, DestinationDatabaseName);

      ProductMediaTypeIDLookup = new Dictionary<int, int>();

      var mediaTypes = ProductMediaRepository.GetProductMediaTypes();

      foreach (var mediaType in mediaTypes)
      {
        if (!mediaType.RsoTypeID.HasValue)
        {
          mediaType.RsoTypeID = ProductMediaRepository.AddProductMediaType(mediaType.Type);
          mediaType.RsoType = mediaType.Type;
        }

        ProductMediaTypeIDLookup.Add(mediaType.TypeID, mediaType.RsoTypeID.Value);
      }

      IntersportContentVendorID = Database.FirstOrDefault<int>(GetVendorByNameQuery, RsoConstants.Vendor.ContentVendorName);
      if (IntersportContentVendorID <= 0)
      {
        throw new ArgumentNullException("Intersport content vendor not found.");
      }
    }

    private void SyncMedia(ProductMediaDifferential productToSync)
    {
      TraceInformation("Synchronizing product: {0}", productToSync.VendorItemNumber);

      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        db.BeginTransaction();

        try
        {
          SyncProductMediaData(productToSync, db);
          SyncProductMediaFiles(productToSync, db);

          db.CompleteTransaction();
        }
        catch (Exception e)
        {
          db.AbortTransaction();
          TraceError("Failed to synchronize media for product: {0}", productToSync.VendorItemNumber);
          TraceCritical(e);
        }
      }
    }

    private void SyncProductMediaFiles(ProductMediaDifferential productToSync, Database db)
    {
      var productMedias = ProductMediaRepository.GetMediaForProduct(productToSync.ProductID);

      foreach (var productMedia in productMedias)
      {
        var newFileLocation = Path.Combine(MediaLocationBasePath, GetNewMediaPath(productMedia.MediaPath));

        var directoryName = Path.GetDirectoryName(newFileLocation);
        if (!Directory.Exists(directoryName))
          Directory.CreateDirectory(directoryName);

        if (!File.Exists(newFileLocation))
          File.Copy(Path.Combine(MediaSourceLocation, productMedia.MediaPath), newFileLocation);
      }
    }

    private void SyncProductMediaData(ProductMediaDifferential productToSync, Database db)
    {
      var productMedia = ProductMediaRepository.GetMediaForProduct(productToSync.ProductID);

      foreach (var productMediaItem in productMedia)
      {
        var newProductMedia = productMediaItem;
        newProductMedia.ProductID = productToSync.RsoProductID;
        newProductMedia.TypeID = ProductMediaTypeIDLookup[productMediaItem.TypeID];
        newProductMedia.VendorID = IntersportContentVendorID;
        newProductMedia.MediaPath = GetNewMediaPath(productMediaItem.MediaPath);

        ProductMediaRepository.AddNewProductMedia(newProductMedia, db);
      }
    }

    private string GetNewMediaPath(string mediaPath)
    {
      return Path.Combine(MediaLocationOrganisationPath, string.Join("\\", mediaPath.Split('\\').Skip(1)));
    }
  }
}