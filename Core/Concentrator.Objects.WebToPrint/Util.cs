using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Net;

namespace Concentrator.Objects.WebToPrint
{
  public static class Util
  {
    

    public static double MillimeterToPoint(double position)
    {
      return (position / 25.4) * 72.0;
    }

    public static double PointToMillimeter(double position)
    {
      return (position / 72.0) * 25.4;
    }

    private static Dictionary<string, Image> imageCache;
    private static object imageCacheLock = new object();

    public static Image LoadImageFromUrl(string url)
    {
      lock (imageCacheLock)
      {
        if (imageCache == null)
          imageCache = new Dictionary<string, Image>();

        if (imageCache.ContainsKey(url))
        {
          return imageCache[url];
        }
        HttpWebRequest request = (HttpWebRequest)System.Net.HttpWebRequest.Create(url);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        Image image = Image.FromStream(response.GetResponseStream());
        imageCache.Add(url, image);
        return image;
      }
    }

    public static Image LoadImageFromPath(string path)
    {
      lock (imageCacheLock)
      {
        if (imageCache == null)
          imageCache = new Dictionary<string, Image>();

        if (imageCache.ContainsKey(path))
          return imageCache[path];

        Image image = Image.FromFile(path);
        imageCache.Add(path, image);
        return image;
      }
    }
  }
}
