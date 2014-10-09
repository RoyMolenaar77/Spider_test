using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestConsole
{
  public class Program
  {
    static void Main(string[] args)
    {
      ServiceReference1.SpiderServiceSoapClient client = new TestConsole.ServiceReference1.SpiderServiceSoapClient();
      string s = client.GetAttributesAssortment(1, new int[] { 2326175,666,347533 }, null);
    }
  }
}
