using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MagentoImport
{
  public class MagentoUtility
  {
    /// <summary>
    /// Function to download Image from website
    /// </summary>
    /// <param name="url">URL address to download image</param>
    /// <returns>Image</returns>
    public static Image DownloadImage(string url)
    {
      Image _tmpImage = null;

      try
      {
        // Open a connection
        System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);

        request.AllowWriteStreamBuffering = true;

        // You can also specify additional header values like the user agent or the referer: (Optional)
        request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)";
        request.Referer = "http://www.google.com/";

        // set timeout for 20 seconds (Optional)
        request.Timeout = 20000;

        // Request response:
        System.Net.WebResponse response = request.GetResponse();

        // Open data stream:
        using (System.IO.Stream stream = response.GetResponseStream())
        {

          // convert webstream to image
          _tmpImage = Image.FromStream(stream);
        }
        // Cleanup
        response.Close();
        response.Close();
      }
      catch (Exception ex)
      {
        // Error
        Console.WriteLine("Exception caught in process: {0}", ex.ToString());
        return null;
      }

      return _tmpImage;
    }

  }
}
