using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Concentrator.Plugins.Axapta.Models
{
  public class FtpSetting
  {
    public string FtpAddress { get; set; }
    public string FtpUsername { get; set; }
    public string FtpPassword { get; set; }

    public string Path { get; set; }

    public string FtpUri
    {
      get
      {
        return string.Format("ftp://{0}:{1}@{2}/{3}",
        HttpUtility.UrlEncode(FtpUsername),
        HttpUtility.UrlEncode(FtpPassword),
        FtpAddress,
        Path);
      }
    }
  }
}
