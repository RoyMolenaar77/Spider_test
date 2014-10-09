using System;
using System.Collections.Generic;
using System.IO;

namespace Concentrator.Plugins.Magento.Contracts
{
  public class MediaExchangeItem
  {
    public String Name
    {
      get;
      set;
    }

    public Int64 Size
    {
      get;
      set;
    }

    public DateTime Time
    {
      get;
      set;
    }
  }

  public interface IMediaExchangeProvider : IDisposable
  {
    void Delete(String remoteFile);

    void Upload(Stream mediaStream, String remotePath);
  }
}
