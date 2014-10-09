using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spider.Objects.Concentrator;
using System.Xml.Linq;
using System.Timers;
using System.Diagnostics;

namespace Spider.Connector.LenmarAssortment
{
  public class LenmarImport : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Lenmar assortment import"; }
    }

    protected override void Process()
    {
      log.Info("Lenmar assortment import started");
      var timer = Stopwatch.StartNew();




      log.Info("Lenmar assortment import finished in "+ timer.Elapsed);
      timer.Stop();
    }

    private XDocument getContent()
    {
      XDocument doc = XDocument.Load(@"D:\Projects\Documentation\Concentrator\MacWay-Content.xml");
      return doc;
    }
  }
}
