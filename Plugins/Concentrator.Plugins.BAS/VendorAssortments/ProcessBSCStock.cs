using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Concentrator.Objects;
using Concentrator.Plugins.Base;
using log4net;

namespace Concentrator.Plugins.BAS
{
  public class ProcessBSCStock : IProcessVendorContent
  {
    #region IProcessVendorContent Members

    public void Process(DataSet content, int ImportVendorID, ILog log)
    {
      if (content != null)
      {
        int totalProducts = content.Tables[0].AsEnumerable().Count();
        log.DebugFormat("Start ProcessBSCStock for {0}, {1} productlines", ImportVendorID, totalProducts);
        using (ConcentratorDataContext context = new ConcentratorDataContext())
        {
          VendorDataParser util = new VendorDataParser(context, ImportVendorID, log);

          foreach (DataRow p in content.Tables[0].AsEnumerable())
          {
            util.LoadProduct(p,log);
            util.UpdateBSCStockLevels(p,log);
          }
        }
        log.DebugFormat("Finish ProcessBSCStock for {0}", ImportVendorID);
      }
      else
        log.DebugFormat("Empty dataset for vendor {0} processing assorment failed", ImportVendorID);
    }

    #endregion
  }
}
