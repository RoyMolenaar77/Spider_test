using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Concentrator.MiddleWare.OrderImportTestConsole
{
  public class Program
  {
    static void Main(string[] args)
    {

      OrderInbound.OrderInboundSoapClient client = new Concentrator.MiddleWare.OrderImportTestConsole.OrderInbound.OrderInboundSoapClient();

      XDocument doc4 = XDocument.Load(@"D:\Projects\Documentation\Concentrator\Orders\985785\465a38ef-071e-4486-9b76-c46b3952366b.xml");
      XDocument doc5 = XDocument.Load(@"D:\Projects\Documentation\Concentrator\Orders\985784\c21f5cf6-068e-44f2-b8f1-33fdbed1944d.xml");
      XDocument doc6 = XDocument.Load(@"D:\Projects\Documentation\Concentrator\Orders\985783\80b9a428-ac74-40eb-8513-1de478b91c98.xml");
      XDocument doc7 = XDocument.Load(@"D:\Projects\Documentation\Concentrator\Orders\985779\a66ddcf7-b15a-4d34-a31e-8fb31b2cacf0.xml");
      client.ImportOrder(doc4.ToString(), 1);
      client.ImportOrder(doc5.ToString(), 1);
      client.ImportOrder(doc6.ToString(), 1);
      client.ImportOrder(doc7.ToString(), 1);
      
    }
  }
}
