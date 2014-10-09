using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Compression;
using System.IO;

namespace Concentrator.Plugins.IceCat
{
  public static class BasicUnzip
  {
    private const int BufSize = 1024 * 50;

    public static void Unzip(string sourceFile, string destination)
    {
      using(var inFile = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, BufSize, FileOptions.SequentialScan))
      {
        using (var zip = new GZipStream(inFile, CompressionMode.Decompress))
        {
          var buffer = new byte[BufSize];
          int read = 0;

          using (var outStream = File.OpenWrite(destination))
          {
            while ((read = zip.Read(buffer, 0, BufSize)) > 0)
            {
              outStream.Write(buffer, 0, read);
            }
          }
        }
      }
    }
  }
}
