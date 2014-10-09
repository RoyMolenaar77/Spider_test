using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concentrator.Plugins.Wehkamp.ExtensionMethods
{
  internal static class StringExtensions
  {
    internal static string ToUnixPath(this string windowsPath)
    {
      return windowsPath.Replace(@"\", @"/");
    }
  }
}
