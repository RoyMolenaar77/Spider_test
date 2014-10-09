using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace Concentrator.Plugins.Alatest
{
  public class AlatestWebService : IDisposable
  {
    private StreamReader _streamReader;
    protected StreamReader StreamReader
    {
      get { return _streamReader; }
      set
      {
        if (_streamReader == null)
          this._streamReader = value;
      }
    }
    private string _reviewUrl = null;
    private string _ratingUrl = null;
    private string _reviewPartnerID = null;

    /// <summary>
    /// Standard alatest parameters to add.
    /// <para>Note: Add to the End of the Querystring</para>
    /// </summary>
    protected string ReviewPartnerID
    {
      get
      {
        if (this._reviewPartnerID == null)
        {
          this._reviewPartnerID = ConfigurationManager.AppSettings["partnerid"];
        }
        return _reviewPartnerID;
      }
    }

    protected string ReviewUrl
    {
      get
      {
        if (_reviewUrl == null)
          _reviewUrl = ConfigurationManager.AppSettings["AlatestReviewURL"];
        return _reviewUrl;
      }
    }

    protected string ReviewSnippetUrl
    {
      get
      {
        if (_ratingUrl == null)
          _ratingUrl = ConfigurationManager.AppSettings["AlatestReviewSnippetURL"];
        return _ratingUrl;
      }
    }


    /// <summary>
    /// Get review of a product
    /// </summary>
    /// <param name="productID">product id</param>
    /// <returns></returns>
    public string GetReview(string productID)
    {
      LoadReader(ReviewUrl, productID);
      return _streamReader.ReadToEnd();
    }

    public string GetReviewSnippet(string productID)
    {
      LoadReader(ReviewSnippetUrl, productID);
      return _streamReader.ReadToEnd();
    }

    protected void LoadReader(string virtualDirectory, string productID)
    {
      virtualDirectory += "/" + ReviewPartnerID + "/" + productID + "/" + 4500;
      _streamReader = new StreamReader(WebRequest.Create(virtualDirectory).GetResponse().GetResponseStream());
    }

    #region IDisposable Members

    public void Dispose()
    {
      this._streamReader.Dispose();
    }

    #endregion
  }
}
