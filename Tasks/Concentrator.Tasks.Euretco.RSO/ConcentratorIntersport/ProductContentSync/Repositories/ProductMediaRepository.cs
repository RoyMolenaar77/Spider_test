using Concentrator.Objects.Models.Products;
using Concentrator.Tasks.Euretco.Rso.ProductContentSync.Models;
using PetaPoco;
using System;
using System.Collections.Generic;

namespace Concentrator.Tasks.Euretco.Rso.ProductContentSync.Repositories
{
  public class ProductMediaRepository
  {
    [Resource]
    private static readonly String GetProductsForProcessingQuery = null;

    [Resource]
    private static readonly String GetProductMediaByProductIDQuery = null;

    [Resource]
    private static readonly String GetProductMediaTypesQuery = null;

    public string SourceDatabaseName { get; set; }
    public string DestinationDatabaseName { get; set; }

    private readonly Database _database;

    public ProductMediaRepository(Database database, string sourceDatabaseName, string destinationDatabaseName)
    {
      _database = database;
      SourceDatabaseName = sourceDatabaseName;
      DestinationDatabaseName = destinationDatabaseName;

      EmbeddedResourceHelper.Bind(GetType());
    }

    public IEnumerable<ProductMediaDifferential> GetProductsForSync()
    {
      var query = string.Format(GetProductsForProcessingQuery, SourceDatabaseName, DestinationDatabaseName);

      return _database.Fetch<ProductMediaDifferential>(query);
    }

    public IEnumerable<ProductMediaType> GetProductMediaTypes()
    {
      var query = string.Format(GetProductMediaTypesQuery, SourceDatabaseName, DestinationDatabaseName);

      return _database.Query<ProductMediaType>(query);
    }

    public int AddProductMediaType(string mediaType)
    {
      var newMediaTypeID = _database.Insert("MediaType", "TypeID", true, new { Type = mediaType });
      return Convert.ToInt32(newMediaTypeID);
    }

    public IEnumerable<ProductMedia> GetMediaForProduct(int productID)
    {
      return _database.Fetch<ProductMedia>(string.Format(GetProductMediaByProductIDQuery, SourceDatabaseName, productID));
    }

    public void AddNewProductMedia(ProductMedia newProductMedia, Database db = null)
    {
      if (db == null)
        db = _database;

      db.Insert("ProductMedia", "MediaID", true, new
        {
          newProductMedia.ProductID,
          newProductMedia.Sequence,
          newProductMedia.VendorID,
          newProductMedia.TypeID,
          newProductMedia.MediaUrl,
          newProductMedia.MediaPath,
          newProductMedia.Resolution,
          newProductMedia.Size,
          newProductMedia.Description,
          newProductMedia.FileName,
          newProductMedia.IsThumbNailImage,
          LastChanged = DateTime.Now
        });
    }
  }
}
