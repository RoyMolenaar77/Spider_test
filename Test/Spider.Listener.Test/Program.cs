using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml;
using Concentrator.Objects.Web;
using Concentrator.Objects.Orders;
using Concentrator.Objects;
using Concentrator.Objects.Vendors;
using Concentrator.Objects.Product;

namespace Concentrator.Listener.Test
{
  class Program
  {
    static void Main(string[] args)
    {
      SpiderPrincipal.Login("SYSTEM", "SYS");

      ConcentratorOrders.OrderInboundSoapClient client = new Concentrator.Listener.Test.ConcentratorOrders.OrderInboundSoapClient();
      XmlDocument dp = new XmlDocument();
      dp.Load(@"D:\Projects\Documentation\Concentrator\Orders\985776\6cc20be8-e478-453b-9fec-b102ad4b4c64.xml");
      client.ImportOrder(dp.DocumentElement, 1);

      Console.ReadLine();

    }
  }
}
