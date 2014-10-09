using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Concentrator.Objects.Utility.FingerPrinting
{
  public static class FingerPrintHelper
  {
    public static FingerPrintModel ExtractFingerPrintInfo(string fileName, string productId, string sequence)
    {              
      if (File.Exists(fileName))
      {
        byte[] content;
        FileInfo fileInfo;
        var info = new FingerPrintModel();               

        try
        {
          fileInfo = new FileInfo(fileName);
          content = File.ReadAllBytes(fileName);
        }
        catch (Exception e)
        {
          return null;
        }
        
        info.PartialFilePath = Path.GetFileName(fileName);
        info.LastWriteMoment = fileInfo.LastWriteTimeUtc;
        info.ProductId = productId;
        info.Sequence = sequence;
        info.Length = fileInfo.Length;
        DateTime fileTime = fileInfo.LastWriteTimeUtc;
      
        using (var cryptoProvider = new SHA1CryptoServiceProvider())
        {
          info.SHA1 = BitConverter.ToString(cryptoProvider.ComputeHash(content)).Replace("-", "");
        }
        using (var cryptoProvider = new MD5CryptoServiceProvider())
        {
          info.MD5 = BitConverter.ToString(cryptoProvider.ComputeHash(content)).Replace("-", "");
        }
        return info;
      }

      return null;
    }
  }
}
