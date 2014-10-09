using Concentrator.Objects.Utility.TransferServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Utility.TransferServices.Implementations
{
  public class FileSystemTransfer : ITransferAdapter
  {
    private TraceSource TraceSource { set; get; }
    public FileSystemTransfer(TraceSource traceSource)
    {
      TraceSource = traceSource;
    }

    public Stream Download(string fileName)
    {
      if (File.Exists(fileName))
        return File.OpenRead(fileName);      
      else
        TraceSource.TraceWarning("File cannot be found on filesystem, the transfer will be skipped. local {0}.", fileName);

      return null;
    }

    public bool Upload(Stream file, string fileName)
    {
      using (var fileStream = File.Create(fileName))
      {
        try
        {
          file.CopyTo(fileStream);
        }
        catch (Exception e)
        { 
          TraceSource.TraceError("File cannot be uploaded on filesystem, the transfer will be skipped. remote name {0}. Error: {1}", fileName, e.Message);
          return false;
        }
      }
      return true;
    }

    public bool Init(Uri uri)
    {
      return true;
    }
  }
}
