using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Vendors.PFA.FileFormats
{
  interface IFileFormat
  {
    /// <summary>
    /// Path for fileExport
    /// </summary>
    /// <param name="fileLocation">file location and name for saving</param>
    void WritFile(string fileLocation);
  }
}
