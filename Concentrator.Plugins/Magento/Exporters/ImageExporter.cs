using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects;
using AuditLog4Net.Adapter;
using System.Configuration;
using System.Xml.Linq;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using SBSimpleSftp;
using SBSSHCommon;
using Concentrator.Plugins.Magento.Models;
using Concentrator.Plugins.Magento.Helpers;
using System.Collections.Concurrent;
using System.Threading;
using SBSftpCommon;
using Concentrator.Web.Services;

namespace Concentrator.Plugins.Magento.Exporters
{
  using Contracts;
  using Providers;
  using System.Web;

  public class ImageExporter : BaseExporter
  {
    public Configuration Configuration
    {
      get;
      private set;
    }

    private string _serializationPath;

    public ImageExporter(Connector connector, IAuditLogAdapter logger, Configuration configuration, string serializationPath = "")
      : base(connector, logger)
    {
      Connector = connector;
      Logger = logger;

      _serializationPath = serializationPath;
    }

#if CACHE
    private static string ImagesCacheFile = @"e:\magento\images-{0}-{1}.xml";
#endif

    private XDocument ImagesXml
    {
      get;
      set;
    }

    private IMediaExchangeProvider GetMediaExchangeProvider()
    {
      var mediaExchangeProviderType = Type.GetType(Connector.ConnectorSettings.GetValueByKey<String>("MediaExchangeProviderType", typeof(SftpMediaExchangeProvider).FullName));

      return (IMediaExchangeProvider)Activator.CreateInstance(mediaExchangeProviderType, Connector);
    }

    protected override void Process()
    {
      try
      {
        CurrentLanguage = PrimaryLanguage;

        var assortmentService = new AssortmentService();

#if CACHE
      if (File.Exists(String.Format(ImagesCacheFile, Connector.ConnectorID, PrimaryLanguage.LanguageID)))
      {
        ImagesXml = XDocument.Load(String.Format(ImagesCacheFile, Connector.ConnectorID, PrimaryLanguage.LanguageID));
      }
      else
      {
        ImagesXml = XDocument.Parse(assortmentService.GetAssortmentImages(Connector.ConnectorID));
        ImagesXml.Save(String.Format(ImagesCacheFile, Connector.ConnectorID, PrimaryLanguage.LanguageID));
      }
#else
        ImagesXml = XDocument.Parse(assortmentService.GetAssortmentImages(Connector.ConnectorID));
#endif

        if (ImagesXml.Root.Element("Error") != null)
        {
          Logger.Debug(ImagesXml);
          throw new Exception("Web service failed call failed.");
        }

        //for debugging purposes
        var debugDirectoryPath = Path.Combine(_serializationPath, "Archive");
        var debugFilePath = Path.Combine(debugDirectoryPath, string.Format(@"image_{0}_{1}.xml", Connector.ConnectorID, DateTime.Now.ToString("dd-MM-yyyy_hhmm")));
        try
        {
          if (!Directory.Exists(debugDirectoryPath))
            Directory.CreateDirectory(debugDirectoryPath);

          ImagesXml.Save(debugFilePath);
        }
        catch (Exception e)
        {
          Logger.Error("Archiving the xml failed in path " + debugFilePath, e);
        }

        var serializationPath = Path.Combine(_serializationPath, Connector.ConnectorID.ToString());
        if (!Directory.Exists(serializationPath))
          Directory.CreateDirectory(serializationPath);

        ImageDiffComparisonHelper comparison = new ImageDiffComparisonHelper(ImagesXml, serializationPath);

        Logger.DebugFormat("Exporting images for language '{0}'", CurrentLanguage.Language.Name);

        var currentProducts = default(SortedDictionary<String, catalog_product_entity>);
        var attributeList = default(SortedDictionary<String, eav_attribute>);

        using (var helper = new MagentoMySqlHelper(Connector.Connection))
        {
          attributeList = helper.GetAttributeList(PRODUCT_ENTITY_TYPE_ID);
          currentProducts = helper.GetSkuList();
        }

        var products = comparison.ProductsToProcess
          .Where(element => element.Attribute("ManufacturerID") != null)
          .OrderByDescending(element => element.AttributeValue("ProductID", 0))
          .GroupBy(element => element.AttributeValue("ManufacturerID"))
          .ToArray();

        var directoryStructureRecords = (
          from i in ImagesXml.Root.Elements("Products").Elements("ProductMedia")
          let urlAttribute = i.Attribute("Url")
          let uri = (urlAttribute != null)
            ? new Uri(urlAttribute.Value)
            : new Uri(i.Value)
          select new
          {
            FirstLevel = uri.Segments.Last().Substring(0, 1),
            SecondLevel = uri.Segments.Last().Substring(1, 1)
          })
          .GroupBy(x => x.FirstLevel)
          .ToDictionary(x => x.Key, y => y.Select(z => z.SecondLevel).Distinct());

        var mediaExchangeProvider = GetMediaExchangeProvider();

        int totalImages = products.Select(element => element.Count()).Sum();
        int totalRecords = products.Length;
        int totalProcessed = 0;

        if (totalRecords == 0)
        {
          Logger.DebugFormat("Finished exporting images for language '{0}'", CurrentLanguage.Language.Name);
          return;
        }

        var options = new ParallelOptions()
        {
          MaxDegreeOfParallelism = 8
        };

        var basePath = Connector.ConnectorSettings.GetValueByKey<string>("FtpPath", string.Empty);
        if (basePath.EndsWith("/"))
          basePath = basePath.TrimEnd('/');


        Parallel.For(0, totalRecords, options, index =>
        {

          using (var helper = new AssortmentHelper(Connector.Connection, Version))
          {
            var product = products[index];
            var sku = product.Key.Trim();
            var entity = default(catalog_product_entity);

            try
            {
              if (currentProducts.TryGetValue(sku, out entity))
              {
                var currentPathList = new List<String>();

                var sequencedImages = product.OrderBy(x => x.AttributeValue("Sequence", 0)).ToList();
                var thumbnailImage = sequencedImages.FirstOrDefault(c => c.AttributeValue("IsThumbnailImage", false));

                int? thumbImageIndex = null;

                if (thumbnailImage != null)
                {
                  thumbImageIndex = sequencedImages.IndexOf(thumbnailImage);
                }

                for (var imageIndex = 0; imageIndex < sequencedImages.Count; imageIndex++)
                {
                  var storeID = 0;
                  var image = sequencedImages[imageIndex];
                  var position = image.AttributeValue("Sequence", 0) + 1;
                  var sourceUri = new Uri(image.AttributeValue("Url", image.Value));
                  var label = image.AttributeValue("Description", String.Empty);

                  var fileName = HttpUtility.UrlDecode(sourceUri.Segments.Last());

                  fileName = fileName.Replace(" ", "");

                  var remotePath = String.Format(@"/{0}/{1}/{2}"
                    , fileName.Substring(0, 1)
                    , fileName.Substring(1, 1)
                    , fileName.Replace(" ", String.Empty));


                  var path = basePath + remotePath;

                  //#if DEBUG
                  //                  //sourceUri = new Uri(sourceUri.AbsoluteUri.Replace("localhost", "10.172.26.1"));
                  //                  sourceUri = new Uri(sourceUri.AbsoluteUri.Replace("localhost", "54.72.108.185"));
                  //#endif

                  using (WebClient dlClient = new WebClient())
                  {
                    using (var stream = new MemoryStream(dlClient.DownloadData(sourceUri)))
                    {
                      mediaExchangeProvider.Upload(stream, path);
                    }
                  }

                  if (remotePath != null)
                  {
                    helper.AddImageToGallery(entity.entity_id, storeID, remotePath, position, label, Logger);

                    if (imageIndex == 0)
                    {
                      helper.SyncAttributeValue(attributeList["image"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, remotePath);
                      helper.SyncAttributeValue(attributeList["small_image"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, remotePath);
                      helper.SyncAttributeValue(attributeList["image_label"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, label);
                    }

                    if (thumbImageIndex.HasValue && thumbImageIndex.Value == imageIndex)
                    {
                      helper.SyncAttributeValue(attributeList["thumbnail"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, remotePath);
                    }

                    currentPathList.Add(remotePath);
                  }
                }

                var mediaGalleryItems = helper
                  .GetGalleryItems(entity)
                  .Where(mediaGalleryItem => !currentPathList.Contains(mediaGalleryItem.value))
                  .ToArray();

                foreach (var file in mediaGalleryItems.Select(mediaGalleryItem => basePath + mediaGalleryItem.value))
                {
                  try
                  {
                    mediaExchangeProvider.Delete(file);
                  }
                  catch
                  {
                  }
                }

                helper.DeleteGalleryItems(mediaGalleryItems);

                Interlocked.Increment(ref totalProcessed);

                if (totalProcessed % 100 == 0)
                {
                  Logger.DebugFormat(String.Format("Processed {0} of {1} products", totalProcessed, totalRecords));
                }
                helper.SetProductLastModificationTime(entity.entity_id);
              }
              else
              {
                comparison.MarkProductAsNotProcessed(product.ToList());
              }
            }
            catch (Exception e)
            {
              Logger.Error(string.Format("Image uploader error for sku {0}", sku), e);

              comparison.MarkProductAsNotProcessed(product.ToList());
            }
          }
        });

        Logger.DebugFormat("Finished exporting images for language '{0}'", CurrentLanguage.Language.Name);
        comparison.ArchiveImages();

        mediaExchangeProvider.Dispose();
      }
      catch (Exception e)
      {
        Logger.Error("Image exporter failed", e);
        throw e;
      }
    }
  }
}
