using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;

namespace Concentrator.Plugins.Magento.Providers
{
  using Contracts;
  using Objects.Models.Connectors;
  using Concentrator.Plugins.Magento.Helpers;
  using SBSimpleSftp;

  public sealed class SftpMediaExchangeProvider : IMediaExchangeProvider
  {
    public SftpMediaExchangeProvider(Connector connector)
    {
      Client = SftpHelper.GetSFTPClient(SftpHelper.GetFtpInfo(connector));
    }

    protected TElSimpleSFTPClient Client;


    public void Delete(String remotePath)
    {
      try
      {
        Client.RemoveFile(remotePath);
      }
      catch { }
    }

    public void Dispose()
    {

    }

    public void Download(String localPath, String remotePath)
    {

    }

    public void EnsurePathOnRemote(string remotePath)
    {
      SftpHelper.EnsurePath(Client, remotePath);
    }

    public void Upload(Stream mediaStream, string remotePath)
    {
      Client.UploadStream(mediaStream, remotePath, SBUtils.TSBFileTransferMode.ftmOverwrite);
    }
  }
}
