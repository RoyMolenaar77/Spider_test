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
  public class  ImageExporter_Old : BaseExporter
  {

    public Configuration Configuration { get; private set; }
    public ImageExporter_Old(Connector connector, IAuditLogAdapter logger, Configuration configuration)
      : base(connector, logger)
    {
      Connector = connector;
      Logger = logger;
    }
    private static string ImagesCacheFile = @"e:\magento\images-{0}-{1}.xml";
    private XDocument ImagesXml;

    protected override void Process()
    {
      CurrentLanguage = PrimaryLanguage;
      var Soap = new AssortmentService();
#if CACHE

      if (!File.Exists(String.Format(ImagesCacheFile, Connector.ConnectorID, PrimaryLanguage.LanguageID)))
      {
        ImagesXml = XDocument.Parse(Soap.GetAssortmentImages(Connector.ConnectorID));
        ImagesXml.Save(String.Format(ImagesCacheFile, Connector.ConnectorID, PrimaryLanguage.LanguageID));
      }
      else
      {
        ImagesXml = XDocument.Load(String.Format(ImagesCacheFile, Connector.ConnectorID, PrimaryLanguage.LanguageID));
      }

#else
      ImagesXml = XDocument.Parse(Soap.GetAssortmentImages(Connector.ConnectorID));
#endif

      SBLicenseManager.TElSBLicenseManager m = new SBLicenseManager.TElSBLicenseManager();
      m.LicenseKey = "8F1317CD48AC4E68BABA5E339D8B365414D7ADA0289CA037E9074D29AD95FF3EC5D796BEFF0FBADB3BD82F48644C9EB810D9B5A305E0D2A1885C874D8BF974B9608CE918113FBE2AA5EEF8264C93B25ABEA98715DB4AD265F47CE02FC9952D69F2C3530B6ABAAA4C43B45E7EF6A8A0646DA038E34FBFB629C2BF0E83C6B348726E622EBD52CA05CF74C68F1279849CCD0C13EA673916BA42684015D658B8E7626F15BD826A4340EDB36CE55791A051FDBCF9FA1456C3B5008AD9990A0185C0EA3B19F9938CB7DA1FE82736ED4C7A566D4BFD53411E8380F4B020CB50E762520EFAE190836FD253B00DB18D4A485C7DC918AA4DCEC856331DD231CC8DC9C741C3";

      FtpInfo ftpInfo = SftpHelper.GetFtpInfo(Connector);


      Logger.DebugFormat("Exporting images for language '{0}'", CurrentLanguage.Language.Name);

      SortedDictionary<string, catalog_product_entity> currentProducts;
      SortedDictionary<string, eav_attribute> attributeList;

      using (var helper = new MagentoMySqlHelper(Connector.Connection))
      {

        attributeList = helper.GetAttributeList(PRODUCT_ENTITY_TYPE_ID);
        currentProducts = helper.GetSkuList();
      }



      var products = (from i in ImagesXml.Root.Elements("Products").Elements("ProductMedia")
                      where i.Attribute("ManufacturerID") != null

                      orderby Convert.ToInt32(i.Attribute("ProductID").Value) descending
                      group i by i.Attribute("ManufacturerID").Value into grouped
                      select grouped).ToList();


      var directoryStructureRecords = (from i in ImagesXml.Root.Elements("Products").Elements("ProductMedia")
                                       // this catches the changes in webservice structure
                                       let urlAttribute = i.Attribute("Url")
                                       let uri = (urlAttribute != null) ? new Uri(urlAttribute.Value) : new Uri(i.Value)
                                       select new
                                       {
                                         FirstLevel = uri.Segments.Last().Substring(0, 1),
                                         SecondLevel = uri.Segments.Last().Substring(1, 1)
                                       }).GroupBy(x => x.FirstLevel).ToDictionary(x => x.Key, y => y.Select(z => z.SecondLevel).Distinct());


      var dirClient = SftpHelper.GetSFTPClient(ftpInfo);


      foreach (var kv in directoryStructureRecords)
      {

        SftpHelper.EnsurePath(dirClient, ftpInfo.FtpPath + kv.Key);
        //if (!dirClient.Exists(kv.Key))
        //  dirClient.MakeDirectory(kv.Key);
        foreach (var sl in kv.Value)
        {
          //if (!dirClient.Exists(kv.Key + "/" + sl))
          //  dirClient.MakeDirectory(kv.Key + "/" + sl);
          SftpHelper.EnsurePath(dirClient, ftpInfo.FtpPath + kv.Key + "/" + sl);

        }
      }

      dirClient.Close(true);

      int totalImages = products.Select(x => x.Count()).Sum();
      int totalRecords = products.Count;
      int totalProcessed = 0;

      if (totalRecords == 0)
      {
        Logger.DebugFormat("Finished exporting images for language '{0}'", CurrentLanguage.Language.Name);
        return;
      }
      ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = 1};

      Parallel.ForEach(Partitioner.Create(0, totalRecords), options, (range, loopState) =>
      {
        TElSimpleSFTPClient client = SftpHelper.GetSFTPClient(ftpInfo);
        try
        {
          using (var helper = new AssortmentHelper(Connector.Connection, Version))
          {

            for (int index = range.Item1; index < range.Item2; index++)
            {
              var product = products[index];

              var sku = product.Key.Trim();
              catalog_product_entity entity;
              if (!currentProducts.TryGetValue(sku, out entity))
                continue;


              List<string> currentPaths = new List<string>();

              //helper.ResetGallery(entity, 0);

              var sequencedImages = product.OrderBy(x => Convert.ToInt32(x.Attribute("Sequence").Value)).ToList();
              var thumbnailImage = sequencedImages.FirstOrDefault(c => c.Attribute("IsThumbnailImage") != null && Convert.ToBoolean(c.Attribute("IsThumbnailImage").Value));

              int? thumbImageIdx = null;
              if (thumbnailImage != null)
              {
                thumbImageIdx = sequencedImages.IndexOf(thumbnailImage);
              }

              for (int idx = 0; idx < sequencedImages.Count; idx++)
              {
                var image = sequencedImages[idx];
                int position = Convert.ToInt32(image.Attribute("Sequence").Value) + 1;

                int storeid = 0;

                // this catches the changes in webservice structure
                var sourceUri = (image.Attribute("Url") != null) ? new Uri(image.Attribute("Url").Value) : new Uri(image.Value);
                var label = image.Attribute("Description") != null ? image.Attribute("Description").Value : string.Empty;

                string fileName = sourceUri.Segments.Last();
                string linuxPath = String.Format(@"/{0}/{1}/{2}", fileName.Substring(0, 1),
                                       fileName.Substring(1, 1),
                                       fileName.Replace(" ", ""));

#if DEBUG
                //sourceUri = new Uri(sourceUri.AbsoluteUri.Replace("localhost", "10.172.26.1"));
                sourceUri = new Uri(sourceUri.AbsoluteUri.Replace("localhost", "172.16.250.94"));
#endif

                string path = DownloadImage(sourceUri, client, ftpInfo.FtpPath, Logger);

                if (path != null)
                {
                  helper.AddImageToGallery(entity.entity_id, storeid, path, position, label, Logger);

                  if (idx == 0)
                  {
                    helper.SyncAttributeValue(attributeList["image"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, path);
                    helper.SyncAttributeValue(attributeList["small_image"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, path);
                    helper.SyncAttributeValue(attributeList["image_label"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, label);
                  }
                  if (thumbImageIdx.HasValue && thumbImageIdx.Value == idx)
                  {
                    helper.SyncAttributeValue(attributeList["thumbnail"].attribute_id, PRODUCT_ENTITY_TYPE_ID, 0, entity.entity_id, path);
                  }
                  currentPaths.Add(path);
                }
              }

              var currentImages = helper.GetGalleryItems(entity);

              CleanupGallery(helper, entity, currentImages, currentPaths, client, ftpInfo.FtpPath);


              Interlocked.Increment(ref totalProcessed);
              //Logger.DebugFormat("Connections : {0}", ftp.ConnectionOpen);
              //Logger.DebugFormat("Processed {0}", totalProcessed);
              if (totalProcessed % 100 == 0)
                Logger.DebugFormat(String.Format("Processed {0} of {1} products", totalProcessed, totalRecords));

            }
          }
        }
        catch (Exception e)
        {
          Logger.Error("Image uploader error ", e);
        }

      });




      Logger.DebugFormat("Finished exporting images for language '{0}'", CurrentLanguage.Language.Name);

    }

    private void SyncImages()
    {



    }

    private void CleanupGallery(AssortmentHelper helper, catalog_product_entity entity, IEnumerable<catalog_product_entity_media_gallery> currentImages, List<string> currentPaths, TElSimpleSFTPClient client, string basePath)
    {


      var toDelete = (from r in currentImages
                      where !currentPaths.Contains(r.value)
                      select basePath + r.value);


      foreach (var file in toDelete)
      {
        try
        {
          client.RemoveFile(file);
        }
        catch { }
      }
      helper.DeleteGalleryItems(
        (from r in currentImages
         where !currentPaths.Contains(r.value)
         select r).ToList()
        );



    }



    string DownloadImage(Uri source, TElSimpleSFTPClient client, string basePath, IAuditLogAdapter Logger)
    {

      string fileName = System.Web.HttpUtility.UrlDecode(source.Segments.Last()).Replace(" ", "");

      string exportFilePath = String.Format(@"{0}\{1}\{2}", fileName.Substring(0, 1),
                                      fileName.Substring(1, 1),
                                      fileName);
      string serverPath = String.Format(@"/{0}/{1}/{2}", fileName.Substring(0, 1),
                                         fileName.Substring(1, 1),
                                         fileName);

      string magentoPath = String.Format(@"\{0}\{1}", fileName.Substring(0, 1),
                                       fileName.Substring(1, 1));

      try
      {
        using (WebClient dlClient = new WebClient())
        {
          using (var stream = new MemoryStream(dlClient.DownloadData(source)))
          {
            client.UploadStream(stream, basePath + serverPath, SBUtils.TSBFileTransferMode.ftmOverwrite);
            //client.SetAttributes(basePath + serverPath, SftpHelper.DefaultAttributes);
          }
        }
      }
      catch (Exception e)
      {
        return null;
      }
      return serverPath;


    }

  }



}
