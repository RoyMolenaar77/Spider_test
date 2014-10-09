using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concentrator.Plugins.Axapta.Helpers
{
  public class Logger : ILogger
  {
    public void Info(string message)
    {
      Console.WriteLine("{0} - {1}", DateTime.Now, message);
    }
  }
}
