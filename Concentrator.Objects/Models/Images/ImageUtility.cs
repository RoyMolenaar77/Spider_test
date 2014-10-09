using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Drawing;
using Image = System.Drawing.Image;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Net;
using System.IO;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Data;
using log4net;
using System.Reflection;
using log4net.Config;
using System.Security.Principal;
using System.Net.Mail;

using System.Text.RegularExpressions;

namespace Concentrator.Objects.Images
{
  public class ImageUtility
  {
    public static string LoadToBase64(string path, int width, int height)
    {
      if (File.Exists(path))
      {
        Image imageFile = Image.FromFile(path);

        var image = ImageUtility.GetFixedSizeImage(imageFile, width, height, true);

        using (MemoryStream ms = new MemoryStream())
        {

          // Convert Image to byte[]
          image.Save(ms, ImageFormat.Gif);
          byte[] imageBytes = ms.ToArray();



          // Convert byte[] to Base64 String
          string base64String = Convert.ToBase64String(imageBytes);
          return base64String;
        }
      }

      return string.Empty;
    }

    public static string ResolveExtension(Image image)
    {
      if (image.RawFormat.Equals(ImageFormat.Png)) return "png";
      if (image.RawFormat.Equals(ImageFormat.Tiff)) return "tiff";
      if (image.RawFormat.Equals(ImageFormat.Bmp)) return "bmp";
      if (image.RawFormat.Equals(ImageFormat.Emf)) return "emf";
      if (image.RawFormat.Equals(ImageFormat.Gif)) return "gif";
      if (image.RawFormat.Equals(ImageFormat.Icon)) return "ico";
      if (image.RawFormat.Equals(ImageFormat.Jpeg)) return "jpg";
      if (image.RawFormat.Equals(ImageFormat.Wmf)) return "wmf";

      return string.Empty;
    }

    /// <summary>
    /// Resizes an image
    /// </summary>
    /// <param name="img">The Image object</param>
    /// <param name="height"></param>
    /// <param name="width"></param>
    /// <returns></returns>
    public static Bitmap ResizeImage(Image img, decimal? height, decimal? width)
    {
      ImageFormat _format = img.RawFormat;
      Color _backgroundColor = _format == ImageFormat.Jpeg ? Color.White : Color.Transparent;

      int _width = width.HasValue ? (int)width : img.Width;
      int _height = height.HasValue ? (int)height : img.Height;
      return ImageUtility.GetFixedSizeImage(img, _width, _height, true, _backgroundColor);
    }

    public enum LifeStyleColors
    {
      Blue,
      LightBlue,
      Orange,
      Gray,
      LightGray
    }

    public static Image CreateProductImage(Image productImage, Size imageSize, bool transparent, Color backgroundColor, bool antiAlias)
    {
      Bitmap result = new Bitmap(imageSize.Width, imageSize.Height, PixelFormat.Format32bppArgb);

      using (Graphics g = Graphics.FromImage(result))
      {
        if (antiAlias)
          g.SmoothingMode = SmoothingMode.HighQuality;
        else
          g.SmoothingMode = SmoothingMode.HighSpeed;

        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

        double maxWidth = (double)imageSize.Width;
        double maxHeight = (double)imageSize.Height;

        #region Product

        int avX = (int)maxWidth; //  - (int)(2 * (_padding * imageSize.Width));
        int avY = (int)maxHeight; // -(int)(2 * (_padding * imageSize.Height));

        Rectangle resizedSize = GetFixedSize(productImage, avX, avY, true);

        int x = (imageSize.Width - resizedSize.Width) / 2;
        int y = (imageSize.Height - resizedSize.Height) / 2;


        g.DrawImage(productImage, new Rectangle(x, y, resizedSize.Width, resizedSize.Height));
        g.Flush();
        #endregion

      }

      return result;
    }

    public static Image CreateLifeStyleProductImage(LifeStyleColors color, Image productImage, Size imageSize, bool transparent, Color backgroundColor, bool antiAlias)
    {

      Bitmap result = new Bitmap(imageSize.Width, imageSize.Height);

      using (Graphics g = Graphics.FromImage(result))
      {
        g.Clear(backgroundColor);

        if (antiAlias)
          g.SmoothingMode = SmoothingMode.HighQuality;
        else
          g.SmoothingMode = SmoothingMode.HighSpeed;

        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

        // background

        // circle


        #region Draw Circle
        Color _colorInner, _colorOuter;

        WebColorConverter cc = new WebColorConverter();
        switch (color)
        {

          case LifeStyleColors.LightBlue:
            _colorOuter = (Color)cc.ConvertFromString("#A8BADE");
            _colorInner = (Color)cc.ConvertFromString("#89A5D5");
            break;
          case LifeStyleColors.Orange:
            _colorOuter = (Color)cc.ConvertFromString("#F7C272");
            _colorInner = (Color)cc.ConvertFromString("#F2B248");
            break;
          case LifeStyleColors.Blue:
            _colorOuter = (Color)cc.ConvertFromString("#425790");
            _colorInner = (Color)cc.ConvertFromString("#2C4985");
            break;
          case LifeStyleColors.Gray:
          default:
            _colorOuter = (Color)cc.ConvertFromString("#425790");
            _colorInner = (Color)cc.ConvertFromString("#2C4985");
            break;
        }

        Rectangle outerRect = new Rectangle(1, 1, imageSize.Width - 2, imageSize.Height - 2);


        float borderWidth = 0.1f;

        g.FillEllipse(new SolidBrush(_colorOuter), outerRect);
        outerRect.Inflate((int)(-borderWidth * outerRect.Width), (int)(-borderWidth * outerRect.Height));
        g.FillEllipse(new SolidBrush(_colorInner), outerRect);

        #endregion

        double maxWidth = Math.Sin(0.25 * Math.PI) * (double)imageSize.Width;
        double maxHeight = Math.Cos(0.25 * Math.PI) * (double)imageSize.Height;

        #region Product




        int avX = (int)maxWidth; //  - (int)(2 * (_padding * imageSize.Width));
        int avY = (int)maxHeight; // -(int)(2 * (_padding * imageSize.Height));

        Rectangle resizedSize = GetFixedSize(productImage, avX, avY, true);


        int x = (imageSize.Width - resizedSize.Width) / 2;
        int y = (imageSize.Height - resizedSize.Height) / 2;


        g.DrawImage(productImage, new Rectangle(x, y, resizedSize.Width, resizedSize.Height));

        #endregion

      }

      result.MakeTransparent(backgroundColor);
      return result;
    }

    public static Image CreateWaterMark(Image imgPhoto, string watermarkFile)
    {
      int size = 0;

      using (Graphics g = Graphics.FromImage(imgPhoto))
      {
        g.SmoothingMode = SmoothingMode.HighQuality;

        if (imgPhoto.Height >= imgPhoto.Width)
        {
          size = Convert.ToInt32(imgPhoto.Height * (2f / 3f));
        }
        else
        {
          size = Convert.ToInt32(imgPhoto.Width * (2f / 3f));
        }

        using (Image waterMark = Bitmap.FromFile(watermarkFile))
        {

          float[][] ptsArray ={ new float[] {1, 0, 0, 0, 0},
                      new float[] {0, 1, 0, 0, 0},
              new float[] {0, 0, 1, 0, 0},
              new float[] {0, 0, 0, 0.2f, 0}, 
              new float[] {0, 0, 0, 0, 1}};
          ColorMatrix clrMatrix = new ColorMatrix(ptsArray);
          ImageAttributes imgAttributes = new ImageAttributes();
          imgAttributes.SetColorMatrix(clrMatrix,
             ColorMatrixFlag.Default, ColorAdjustType.Bitmap);


          Rectangle rect = GetFixedSize(waterMark, size, size, true);
          g.DrawImage(
            waterMark,
            new Rectangle(
             (imgPhoto.Width - rect.Width) / 2,
             (imgPhoto.Height - rect.Height) / 2,
             rect.Width,
             rect.Height),
            0,
            0,
            waterMark.Width,
            waterMark.Height,
            GraphicsUnit.Pixel,
            imgAttributes);
        }
      }
      return imgPhoto;
    }

    public static Image CreateKitProductImage(LifeStyleColors color, Image productImage, Size imageSize, bool transparent, Color backgroundColor, bool antiAlias)
    {

      Bitmap result = new Bitmap(imageSize.Width, imageSize.Height);

      using (Graphics g = Graphics.FromImage(result))
      {
        g.Clear(backgroundColor);

        if (antiAlias)
          g.SmoothingMode = SmoothingMode.HighQuality;
        else
          g.SmoothingMode = SmoothingMode.HighSpeed;

        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

        // background

        // circle

        #region Draw Circle
        Color _colorOuter;
        Color _colorInner;

        WebColorConverter cc = new WebColorConverter();
        switch (color)
        {
          case LifeStyleColors.LightBlue:
            _colorOuter = (Color)cc.ConvertFromString("#A8BADE");
            _colorInner = (Color)cc.ConvertFromString("#89A5D5");
            break;
          case LifeStyleColors.Orange:
            _colorOuter = (Color)cc.ConvertFromString("#F7C272");
            _colorInner = (Color)cc.ConvertFromString("#F2B248");
            break;
          case LifeStyleColors.Gray:
            _colorInner = (Color)cc.ConvertFromString("#616d8d");
            _colorOuter = (Color)cc.ConvertFromString("#3d5076");
            break;
          case LifeStyleColors.Blue:
          default:
            _colorOuter = (Color)cc.ConvertFromString("#425790");
            _colorInner = (Color)cc.ConvertFromString("#2C4985");
            break;
        }

        Rectangle outerRect = new Rectangle(1, 1, imageSize.Width - 2, imageSize.Height - 2);

        float borderWidth = 0.1f;

        g.FillEllipse(new SolidBrush(_colorOuter), outerRect);
        outerRect.Inflate((int)(-borderWidth * outerRect.Width), (int)(-borderWidth * outerRect.Height));
        g.FillEllipse(new SolidBrush(_colorInner), outerRect);


        #endregion


        double maxWidth = Math.Sin(0.25 * Math.PI) * (double)imageSize.Width;
        double maxHeight = Math.Cos(0.25 * Math.PI) * (double)imageSize.Height;

        #region Product




        int avX = (int)maxWidth; //  - (int)(2 * (_padding * imageSize.Width));
        int avY = (int)maxHeight; // -(int)(2 * (_padding * imageSize.Height));

        Rectangle resizedSize = GetFixedSize(productImage, avX, avY, true);


        int x = (imageSize.Width - resizedSize.Width) / 2;
        int y = (imageSize.Height - resizedSize.Height) / 2;


        g.DrawImage(productImage, new Rectangle(x, y, resizedSize.Width, resizedSize.Height));

        #endregion




        // product

      }

      result.MakeTransparent(backgroundColor);
      return result;
    }

    public static ImageCodecInfo GetEncoderInfo(String mimeType)
    {
      int j;
      ImageCodecInfo[] encoders;
      encoders = ImageCodecInfo.GetImageEncoders();
      for (j = 0; j < encoders.Length; ++j)
      {
        if (encoders[j].MimeType == mimeType)
          return encoders[j];
      }
      return null;
    }

    public static Bitmap GetFixedSizeImage(Image imgPhoto, int Width, int Height, bool KeepRatio)
    {
      return GetFixedSizeImage(imgPhoto, Width, Height, KeepRatio, Color.Transparent);
    }

    public static Bitmap GetFixedSizeImage(Image imgPhoto, int Width, int Height, bool KeepRatio, Color backgroundColor)
    {
      return GetFixedSizeImage(imgPhoto, Width, Height, KeepRatio, backgroundColor, 0);
    }

    public static Bitmap GetFixedSizeImage(Image imgPhoto, int maxWidth, int maxHeight, bool KeepRatio, Color backgroundColor, int padding)
    {
      int sourceWidth = imgPhoto.Width;
      int sourceHeight = imgPhoto.Height;
      int sourceX = 0;
      int sourceY = 0;
      int destX = 0;
      int destY = 0;
      int destWidth = -1;
      int destHeight = -1;


      if (KeepRatio)
      {
        float aspectRatio = (float)sourceWidth / (float)sourceHeight;
        if (aspectRatio > 1f)
        {
          destWidth = maxWidth;
          destHeight = (int)((float)maxWidth / aspectRatio);
        }
        else
        {
          destHeight = maxHeight;
          destWidth = (int)((float)maxHeight * aspectRatio);
        }

        if (destWidth == -1)
        {
          destWidth = sourceWidth;
          destHeight = (int)((float)sourceWidth / aspectRatio);
        }

        if (destHeight == -1)
        {
          destHeight = sourceHeight;
          destWidth = (int)((float)sourceHeight * aspectRatio);
        }
        destX = (int)((float)(maxWidth - destWidth) / 2f);
        destY = (int)((float)(maxHeight - destHeight) / 2f);
      }
      else
      {
        destWidth = maxWidth;
        destHeight = maxHeight;

        if (destWidth == -1)
          destWidth = sourceWidth;

        if (destHeight == -1)
          destHeight = sourceHeight;
      }


      Bitmap bmPhoto = new Bitmap(maxWidth > 0 ? maxWidth : destWidth, maxHeight > 0 ? maxHeight : destHeight, PixelFormat.Format24bppRgb);
      //bmPhoto.SetResolution(imgPhoto.Width, imgPhoto.Height);

      if (backgroundColor == Color.Transparent)
        bmPhoto.MakeTransparent();

      using (Graphics grPhoto = Graphics.FromImage(bmPhoto))
      {

        if (backgroundColor != Color.Transparent)
          grPhoto.Clear(backgroundColor);


        grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
        grPhoto.CompositingQuality = CompositingQuality.HighQuality;
        grPhoto.SmoothingMode = SmoothingMode.AntiAlias;

        var rec = grPhoto.ClipBounds;
        var region = grPhoto.Clip;

        grPhoto.DrawImage(imgPhoto, new Rectangle(destX, destY, destWidth, destHeight), new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight), GraphicsUnit.Pixel);
      }
      return bmPhoto;
    }

    public static Rectangle GetFixedSize(Image imgPhoto, int Width, int Height, bool KeepRatio)
    {
      int sourceWidth = imgPhoto.Width;
      int sourceHeight = imgPhoto.Height;
      int destX = 0;
      int destY = 0;
      int destWidth = -1;
      int destHeight = -1;
      float nPercent = 0;
      float nPercentW = 0;
      float nPercentH = 0;

      nPercentW = ((float)Width / (float)sourceWidth);
      nPercentH = ((float)Height / (float)sourceHeight);

      if (KeepRatio)
      {
        if (nPercentH < nPercentW)
        {
          nPercent = nPercentH;
          destX = System.Convert.ToInt16((Width - (sourceWidth * nPercent)) / 2);
          destX = -1;
        }
        else
        {
          nPercent = nPercentW;
          destY = -1;
        }
        destWidth = (int)(sourceWidth * nPercent);
        destHeight = (int)(sourceHeight * nPercent);
      }
      else
      {
        destX = -1;
        destY = -1;
        destWidth = Width;
        destHeight = Height;
      }

      return new Rectangle(destX, destY, destWidth, destHeight);
    }

    public static ColorPalette SetTransparentColor(ColorPalette palette, int nColors, Color color)
    {
      Color c;
      Color[] colors = palette.Entries;

      for (int idx = 0; idx <= nColors; idx++)
      {
        c = colors[idx];
        if (c.R == color.R && c.G == color.G && c.B == color.B)
          palette.Entries[idx] = Color.FromArgb(0, c.R, c.G, c.B);
        else
          palette.Entries[idx] = Color.FromArgb(255, c.R, c.G, c.B);
      }

      return palette;

    }

  }
}

