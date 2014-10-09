using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.Config;

namespace Spider.MiddleWare.WebImport
{
  class Program
  {
    static void Main(string[] args)
    {
      XmlConfigurator.Configure();

      //Spider.WebsiteImport.Importer.Start();
      //Console.ReadLine();
      //Spider.WebsiteImport.Importer.Stop();

      //Spider.VeilingImport.Importer.Start();
      //Console.ReadLine();
      //Spider.VeilingImport.Importer.Stop();
    }
  }
}
