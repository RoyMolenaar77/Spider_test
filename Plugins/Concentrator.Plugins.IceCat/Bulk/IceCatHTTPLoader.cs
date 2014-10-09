using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Data.Objects;
using Concentrator.Objects.Vendors.Base;

namespace Concentrator.Plugins.IceCat.Bulk
{
  public abstract class IceCatHTTPLoader<TContext> : BulkLoader<TContext>
    where TContext : ObjectContext, new()
  {
    private const int InMemBufSize = 1024;

    protected Uri BaseURI { get; private set; }
    protected NetworkCredential Credentials;

    public IceCatHTTPLoader(string baseURI, NetworkCredential credentials)
    {
      BaseURI = new Uri(baseURI);
      Credentials = credentials;
    }

    protected void DownloadFile(string relativeUrl, Stream destStream)
    {
      HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(BaseURI, relativeUrl));
      request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

      if (Credentials != null)
        request.Credentials = Credentials;

      using (var resp = request.GetResponse())
      {
        using (var respStream = resp.GetResponseStream())
        {
          var buf = new byte[InMemBufSize];
          int read = 0;
          while ((read = respStream.Read(buf, 0, buf.Length)) > 0)
          {
            destStream.Write(buf, 0, read);
          }
        }
      }
    }

    protected void DownloadFile(string relativeUrl, string localDestination)
    {
      if (!Directory.Exists(Path.GetDirectoryName(localDestination)))
        Directory.CreateDirectory(Path.GetDirectoryName(localDestination));

      using (var outFile = File.OpenWrite(localDestination))
      {
        DownloadFile(relativeUrl, outFile);
      }
    }
  }
}
