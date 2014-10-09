using Concentrator.Objects.Ftp;
using Concentrator.Objects.Utility.TransferServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Utility.TransferServices.Implementations
{
  public class FtpTransfer : ITransferAdapter
  {
    protected IFtpClient ftpClient;

    private TraceSource TraceSource { set; get; }
    public FtpTransfer(TraceSource traceSource)
    {
      TraceSource = traceSource;
    }

    public bool Init(Uri uri)
    {
      return true;
    }

    public bool Upload(Stream file, string fileName)
    {
      try
      {
        ftpClient.UploadFile(fileName, file);
        return true;
      }
      catch (Exception e)
      {
        TraceSource.TraceWarning("File cannot be uploaded to ftp server, the transfer will be skipped. local name {0}. Error: {1}", fileName, e.Message);
      }
      return false;
    }

    public Stream Download(string fileName)
    {
      try
      {
        return ftpClient.DownloadFile(fileName);
      }
      catch (Exception e)
      {
        TraceSource.TraceWarning("File cannot be found on ftp server, the transfer will be skipped. local name {0}. Error: {1}", fileName, e.Message);
        return null;
      }
      
    }
  }
}
