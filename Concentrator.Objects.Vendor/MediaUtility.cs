﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;


namespace Concentrator.Objects.Vendors
{
  public class MediaUtility
  {
    public static int? getSize(string imagePath)
    {
      if (File.Exists(imagePath))
      {
        return (int)Math.Round(new FileInfo(imagePath).Length / 1024d, 0);
      }
      return null;
    }

    public static string getRes(string imagePath)
    {
      if (File.Exists(imagePath))
      {
        FileStream fs = null;
        try
        {
          using (fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
          {
            using (var image = Image.FromStream(fs))
            {
              string res = string.Format("{0}x{1}", image.Width, image.Height);
              return res;
            }
          }
        }
        catch
        {
        }
      }

      return null;
    }
  }
}
