using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Globalization;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using log4net;

namespace Concentrator.Plugins.Slurp.Providers
{
  public class NoOp : ISlurpSite
  {
    private ILog log = LogManager.GetLogger("Slurp");
    private static CultureInfo culture = new CultureInfo("en-US");

    #region ISlurpSite Members

    public SlurpResult Process(string manufacturerId)
    {
      log.InfoFormat("Running {0} for {1}", this.GetType().Name, manufacturerId);
      SlurpResult result = new SlurpResult(manufacturerId);



      return result;
    }

    #endregion



  }
}
