using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Concentrator.Tasks.Euretco.Rso.BizTalk.ProductExport
{
  internal class Program
  {
    [STAThread]
    public static void Main(String[] arguments)
    {
      TaskBase.Execute<ProductExporter>();
    }
  }
}