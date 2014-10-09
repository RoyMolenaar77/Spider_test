using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.IO;
using Concentrator.Objects.ZipUtil;
using Ionic.Zip;

namespace Concentrator.Web.Shared.Results
{
  class ZipResult : ActionResult
  {
    private readonly string _filename;
    private readonly string _contentType;
    private readonly Stream _stream;

    public ZipResult(string filename)
    {
      _filename = filename;
      _contentType = "application/octet-stream";
    }

    public ZipResult(Stream stream)
    {
      _stream = stream;
    }

    private string GetContentType(string fileName)
    {
      string contentType = "application/octetstream";
      string ext = System.IO.Path.GetExtension(fileName).ToLower();
      Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
      if (registryKey != null && registryKey.GetValue("Content Type") != null)
        contentType = registryKey.GetValue("Content Type").ToString();
      return contentType;
    }

    public override void ExecuteResult(ControllerContext context)
    {
      if (_filename != null)
      {
        ZipFile zip = new ZipFile(_filename);
        context.HttpContext.Response.ContentType = "application/zip";
        zip.Save(context.HttpContext.Response.OutputStream);
      }

      else
      {
        MemoryStream stream = new MemoryStream();
        byte[] buffer = new byte[256];
        int bytesRead = 0;

        do
        {
          bytesRead = _stream.Read(buffer, 0, 256);
          stream.Write(buffer, 0, bytesRead);
        } while (bytesRead > 0);

        stream.Position = 0;
        var _zipFile = ZipFile.Read(stream);

        context.HttpContext.Response.ContentType = "application/zip";
        _zipFile.Save(context.HttpContext.Response.OutputStream);

      }

      context.HttpContext.Response.End();
    }


  }
}
