using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.IO;

namespace Concentrator.Web.Shared.Results
{
  public class FileResult : ActionResult
  {
    private readonly string _filename;
    private readonly string _message;
    private readonly string _contentType;

    public FileResult(string filename, string message)
    {
      _filename = filename;
      _message = message;
      _contentType = "application/octet-stream";
    }

    public FileResult(string filename, string message, string contentType)
    {
      _filename = filename;
      _message = message;
      _contentType = contentType;
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


      if (File.Exists(_filename))
      {
        context.HttpContext.Response.Clear();
        context.HttpContext.Response.ClearContent();
        context.HttpContext.Response.ClearHeaders();
        context.HttpContext.Response.ContentType = _contentType;
        context.HttpContext.Response.AddHeader("content-disposition", "attachment; filename=" + _filename);
        context.HttpContext.Response.Buffer = false;
        context.HttpContext.Response.BufferOutput = false;
        context.HttpContext.Response.WriteFile(_filename);
      }
      else
        context.HttpContext.Response.Write("No image found");
      //using (var str = File.Open(_filename, FileMode.Open))
      //{
      //  using (var writer = new StreamWriter(context.HttpContext.Response.OutputStream))
      //  {

      //    writer.WriteLine(_message);
      //    writer.Flush();
      //  }
      //}
      context.HttpContext.Response.End();
    }
  }
}
