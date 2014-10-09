using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using log4net;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.Objects.Vendors.Base
{
  public interface IProcessVendorContent
  {
    /// <summary>
    /// Processes the content of the dataset according to the type of vendor and connector
    /// </summary>
    /// <param name="content">Content to process</param>
    void Process(DataSet content, Vendor vendor, ILog log);
  }
}
