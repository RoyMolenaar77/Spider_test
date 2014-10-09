using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Connectors;
using SBSimpleSftp;
using System.IO;

namespace Concentrator.Plugins.Magento.Helpers
{
  public class IndexerHelper
  {
    private TElSimpleSFTPClient _client;
    private string _pathOnServer;
    private string triggerAllFile = "index-all.trg";
    private string triggerImageFile = "index-image.trg";
    private string triggerStockFile = "index-stock.trg";


    public IndexerHelper(FtpInfo info, string pathOnServer)
    {
      _client = SftpHelper.GetSFTPClient(info);
      _pathOnServer = info.FtpPath + pathOnServer;

      //SftpHelper.EnsurePath(_client, _pathOnServer);

      if (!_pathOnServer.EndsWith("/")) _pathOnServer += "/";
    }



    /// <summary>
    /// Creates assortment trigger
    /// index-all.trg
    /// </summary>
    public void CreateAssortmentTrigger()
    {
      var path = _pathOnServer + triggerAllFile;

      UploadTriggerFile(path);
    }

    /// <summary>
    /// Creates image triger
    /// index-image.trg
    /// </summary>
    public void CreateImageTrigger()
    {
      var path = _pathOnServer + triggerImageFile;

      UploadTriggerFile(path);
    }

    /// <summary>
    /// Creates stock trigger
    /// index-stock.trg
    /// </summary>
    public void CreateStockTrigger()
    {
      var path = _pathOnServer + triggerStockFile;

      UploadTriggerFile(path);
    }


    private void UploadTriggerFile(string path)
    {
      using (var str = new MemoryStream())
      {
        _client.UploadStream(str, path, SBUtils.TSBFileTransferMode.ftmOverwrite);
        _client.SetAttributes(path, SftpHelper.DefaultAttributes);
      }
    }
  }
}
