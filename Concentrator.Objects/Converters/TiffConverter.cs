using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using BitMiracle.LibTiff.Classic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Drawing.Imaging;

namespace Concentrator.Objects.Converters
{
  public class TiffConverter : IDisposable
  {
    public string InputPath { get; private set; }
    public TiffConverter(string inputPath)
    {
      if (String.IsNullOrEmpty(inputPath))
        throw new ArgumentNullException("inputPath");

      InputPath = inputPath;
    }

    private Bitmap sourceData = null;

    #region Public Methods

    //public void WriteTo(Stream outputStream)
    //{
    //  WriteTo(outputStream, null, null);
    //}

    public void WriteTo(Stream outputStream, int width, int height, bool forceOutputSize = true)
    {
      if (sourceData == null)
      {
        Convert();
      }

      OutputResult(outputStream, width, height,forceOutputSize);
    }

    //public void WriteTo(string fileName)
    //{
    // WriteTo(fileName, null, null);
    //}

    public void WriteTo(string fileName, int width, int height, bool forceOutputSize = true)
    {

      if (sourceData == null)
      {
        Convert();
      }

      if (File.Exists(fileName))
        File.Delete(fileName);

      using (FileStream ms = new FileStream(fileName, FileMode.OpenOrCreate))
      {
        OutputResult(ms, width, height,forceOutputSize);
      }


    }

    public Image GetImage(int width, int height, bool forceOutputSize = true)
    {
      if (sourceData == null)
      {
        Convert();
      }
      Stream s = new MemoryStream();
      OutputResult(s, width, height, forceOutputSize);
      return Image.FromStream(s);
    }

    #endregion

    private void OutputResult(Stream outputStream, int width, int height, bool forceOutputSize)
    {
      if (width > 0 || height > 0)
      {

        int targetWidth = -1, targetHeight = -1;

        float aspectRatio = (float)sourceData.Width / (float)sourceData.Height;

        if (aspectRatio >= (float)width / (float)height)
        {
          targetWidth = width;
          targetHeight = (int)((float)width / aspectRatio);
        }
        else
        {
          targetHeight = height;
          targetWidth = (int)((float)height * aspectRatio);
        }

        if (forceOutputSize)
        {
          using (Bitmap resized = new Bitmap(width, height))
          {
            using (Graphics rG = Graphics.FromImage(resized))
            {
              rG.DrawImage(sourceData, ((float)width - (float)targetWidth) / 2f, ((float)height - (float)targetHeight) / 2f, targetWidth, targetHeight);
            }
            resized.Save(outputStream, ImageFormat.Png);

          }
        } else
        {
          using (Bitmap resized = new Bitmap(targetWidth, targetHeight))
          {
            using (Graphics rG = Graphics.FromImage(resized))
            {
              rG.DrawImage(sourceData, 0f,0f, targetWidth, targetHeight);
            }
            resized.Save(outputStream, ImageFormat.Png);

          }
        }
      }
      else
      {
        sourceData.Save(outputStream, ImageFormat.Png);
      }
    }



    private struct Bezier
    {
      public PointF Start;
      public PointF Center;
      public PointF End;
      public PathRecordType Type;
    }

    private void Convert()
    {


      if (!File.Exists(InputPath))
        throw new ArgumentException("Input file cannot be found", "inputPath");

      List<List<Bezier>> beziers = new List<List<Bezier>>();

      using (MemoryStream ms = new MemoryStream())
      {
        using (FileStream fs = new FileStream(InputPath, FileMode.Open,FileAccess.Read,FileShare.Read))
        {
          fs.CopyTo(ms);
        }
        ms.Position = 0;

        using (Tiff image = Tiff.ClientOpen("name", "r", ms, new TiffStream()))
        {
          if (image == null)
            throw new IOException("Could not open image file");

          // Find the width and height of the image
          FieldValue[] value = image.GetField(TiffTag.IMAGEWIDTH);
          int sourceWidth = value[0].ToInt();

          value = image.GetField(TiffTag.IMAGELENGTH);
          int sourceHeight = value[0].ToInt();

          // Read the image into the memory buffer
          /*int[] raster = new int[sourceHeight * sourceWidth];
          if (!image.ReadRGBAImage(sourceWidth, sourceHeight, raster))
          {
            throw new IOException("Could not read image file");
          }*/


          FieldValue[] tag = image.GetField(TiffTag.PHOTOSHOP);

          byte[] contents = tag[1].Value as byte[];

          List<Bezier> bezierList = new List<Bezier>();

          int idx = 0;
          while (idx < contents.Length - 7)
          {
            string header = Encoding.ASCII.GetString(contents, idx, 4);
            idx += 4;
            if (header != "8BIM")
              continue;

            int resourceId = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt16(contents, idx));
            idx += 2;

            int lengthIndication = (int)contents[idx];
            lengthIndication++;


            bool even = ((lengthIndication % 2) == 0);

            if (!even)
              lengthIndication++;

            string pathName = "";
            if (resourceId == 2001)
            {
              pathName = System.Text.Encoding.Default.GetString(contents.Skip(idx + 1).Take(lengthIndication - 2).ToArray());
            }
            idx += lengthIndication;

            int contentSize = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt32(contents, idx));
            idx += 4;

            if (idx >= contents.Length - contentSize)
              break;



            if (resourceId >= 2000 && resourceId < 2999
                  && pathName != "etiket") // jumbo fix to filter out cutouts who don't add to the alpha channel
            {

              byte[] content2 = contents.Skip(idx).Take(contentSize).ToArray();

              int recordSize = 26;
              int subPathLength = 0;
              for (int pIndex = 0; pIndex < content2.Length; pIndex += recordSize)
              {
                byte[] pointRecord = content2.Skip(pIndex).Take(recordSize).ToArray();
                int recordSelector = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt16(pointRecord, 0));

                switch ((PathRecordType)recordSelector)
                {
                  case PathRecordType.ClosedSubPath:
                    subPathLength = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt16(pointRecord, 2));
                    break;

                  case PathRecordType.ClosedSubPathBezierLinked:
                  case PathRecordType.ClosedSubPathBezierUnlinked:

                    var previousKnot = ParsePoint(pointRecord.Skip(2).Take(8).ToArray(), sourceWidth, sourceHeight);

                    var anchorPoint = ParsePoint(pointRecord.Skip(10).Take(8).ToArray(), sourceWidth, sourceHeight);

                    var nextKnot = ParsePoint(pointRecord.Skip(18).Take(8).ToArray(), sourceWidth, sourceHeight);

                    bezierList.Add(new Bezier()
                    {
                      Start = previousKnot,
                      Center = anchorPoint,
                      End = nextKnot,
                      Type = (PathRecordType)recordSelector
                    });

                    subPathLength--;
                    if (subPathLength == 0)
                    {
                      beziers.Add(bezierList);
                      bezierList = new List<Bezier>();
                    }
                    break;


                  case PathRecordType.InitialFill:
                    // Console.WriteLine("Initial Fill : {0}", pointRecord[3]);
                    break;

                  case PathRecordType.PathFill:
                    {

                    } break;

                  default:
                    {

                    } break;
                }





              }


            }

            byte[] content = new byte[contentSize];
            Array.Copy(contents, idx, content, 0, contentSize);

            even = ((contentSize % 2) == 0);
            if (!even)
              contentSize++;

            idx += contentSize;


          }

            #region Read Original TIFF bitmap

            /*Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

          BitmapData bmpdata = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
          byte[] bits = new byte[bmpdata.Stride * bmpdata.Height];

          for (int y = 0; y < bmp.Height; y++)
          {
            int rasterOffset = y * bmp.Width;
            int bitsOffset = (bmp.Height - y - 1) * bmpdata.Stride;

            for (int x = 0; x < bmp.Width; x++)
            {
              int rgba = raster[rasterOffset++];

              bits[bitsOffset++] = (byte)Tiff.GetB(rgba);
              bits[bitsOffset++] = (byte)Tiff.GetG(rgba);
              bits[bitsOffset++] = (byte)Tiff.GetR(rgba);
              bits[bitsOffset++] = (byte)Tiff.GetA(rgba);

              //(byte)((rgba >> 16) & 0xff);
            }
          }

          System.Runtime.InteropServices.Marshal.Copy(bits, 0, bmpdata.Scan0, bits.Length);
          bmp.UnlockBits(bmpdata);*/

            #endregion


            #region Create Clipping Path
            List<GraphicsPath> graphicPaths = new List<GraphicsPath>();
            GraphicsPath gp = new GraphicsPath();
            gp.AddRectangle(new Rectangle(0, 0, sourceWidth, sourceHeight));
            graphicPaths.Add(gp);

            for (int bezieridx = 0; bezieridx < beziers.Count; bezieridx++)
            {
              gp = new GraphicsPath();
              for (idx = 0; idx < beziers[bezieridx].Count; idx++)
              {
                Bezier b = beziers[bezieridx][idx];
                Bezier nextB = beziers[bezieridx][(idx + 1) % beziers[bezieridx].Count];

                gp.AddBezier(b.Center, b.End, nextB.Start, nextB.Center);
              }
              graphicPaths.Add(gp);
            }

            #endregion

            #region Write out clipped image

            ms.Position = 0;
            using (Image tiffimgdata = Image.FromStream(ms, true))
            {
              //Image tiffimgdata = Image.FromStream(image.GetStream());
              if (sourceData != null)
                sourceData.Dispose();

              sourceData = null;

              sourceData = new Bitmap(sourceWidth, sourceHeight);

              using (Graphics g = Graphics.FromImage(sourceData))
              {
                if (graphicPaths.Count > 1)
                {
                  foreach (GraphicsPath gpath in graphicPaths)
                    g.SetClip(gpath, CombineMode.Xor);
                }
                g.DrawImage(tiffimgdata, 0, 0, sourceWidth, sourceHeight);

#if DEBUG
                /*Pen pen = new Pen(Brushes.Black);
              pen.Width = 5;

              foreach (GraphicsPath gpath in graphicPaths)
                g.DrawPath(pen, gpath);*/
#endif
              }
            }
            #endregion
        }
      }
    }

    private PointF ParsePoint(byte[] pathPoint, int width, int height)
    {
      long input = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt32(pathPoint, 0));
      long input2 = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt32(pathPoint, 4));

      float f1 = ((float)input) / 0x1000000;
      float f2 = ((float)input2) / 0x1000000;
      return new PointF(f2 * (float)width, f1 * (float)height);
    }




    private enum PathRecordType
    {
      ClosedSubPath = 0,
      ClosedSubPathBezierLinked = 1,
      ClosedSubPathBezierUnlinked = 2,
      OpenSubPath = 3,
      OpenSubPathBezierLinked = 4,
      OpenSubPathBezierUnlinked = 5,
      PathFill = 6,
      Clipboard = 7,
      InitialFill = 8
    }


    #region IDisposable Members

    protected void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (sourceData != null)
          sourceData.Dispose();

        sourceData = null;
      }
    }
    public void Dispose()
    {
      Dispose(true); //i am calling you from Dispose, it's safe
      GC.SuppressFinalize(this); //Hey, GC: don't bother calling finalize later
    }

    #endregion
  }
}