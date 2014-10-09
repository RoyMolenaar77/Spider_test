using System.IO.Compression;
using System.Web;
using System.Globalization;

namespace Concentrator.Web.Services
{
  public class XmlCompressionModule : IHttpModule
  {
    #region IHttpModule Members

    public void Dispose()
    {

    }

    public void Init(HttpApplication context)
    {
      context.PreRequestHandlerExecute += new System.EventHandler(context_PreRequestHandlerExecute);
    }

    void context_PreRequestHandlerExecute(object sender, System.EventArgs e)
    {
      HttpApplication app = (HttpApplication)sender;
      HttpRequest request = app.Request;
      HttpResponse response = app.Response;

      //Ajax Web Service request is always starts with application/json
      if (request.ContentType.ToLower(CultureInfo.InvariantCulture).Contains("text/xml"))
      {
        //User may be using an older version of IE which does not support compression, so skip those
        if (!((request.Browser.IsBrowser("IE")) && (request.Browser.MajorVersion <= 6)))
        {
          string acceptEncoding = request.Headers["Accept-Encoding"];

          if (!string.IsNullOrEmpty(acceptEncoding))
          {
            acceptEncoding = acceptEncoding.ToLower(CultureInfo.InvariantCulture);

            if (acceptEncoding.Contains("gzip"))
            {
              response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
              response.AddHeader("Content-encoding", "gzip");
            }
            else if (acceptEncoding.Contains("deflate"))
            {
              response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress);
              response.AddHeader("Content-encoding", "deflate");
            }
          }
        }

      }

    #endregion
    }
  }
}