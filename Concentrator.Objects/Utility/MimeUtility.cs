using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Utility
{
  public static class MimeUtility
  {
    public static string MimeType(string filename)
    {
      string mime = "application/octetstream";
      string ext = System.IO.Path.GetExtension(filename).ToLower();
      Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
      if (rk != null && rk.GetValue("Content Type") != null)
        mime = rk.GetValue("Content Type").ToString();
      return mime;
    }
  }
}
