using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Vendors;
using System.Data.Objects;
using Concentrator.Objects.DataAccess.Repository;

namespace Concentrator.Objects.Utility
{
  public class ProductStatusVendorMapper
  {
    private IRepository<VendorProductStatus> Table;
    private List<VendorProductStatus> Statuses;
    private int VendorID;
    /// <summary>
    /// Initializes a new instance of product status mapper
    /// </summary>
    /// <param name="statusCollection">Collection of all vendorProductStatuses (context.VendorProductStatus)</param>
    public ProductStatusVendorMapper(IRepository<VendorProductStatus> statusCollection, int vendorid)
    {
      Table = statusCollection;
      VendorID = vendorid;
      Statuses = statusCollection.GetAll(s => s.VendorID == vendorid).ToList();
    }

    public int SyncVendorStatus(string status, int defaultStatus)
    {
      //if (string.IsNullOrEmpty(status)) return defaultStatus;

      var prStatus = Statuses.FirstOrDefault(c => c.VendorStatus == status);

      if (prStatus == null)
      {
        prStatus = new VendorProductStatus
        {
          VendorID = VendorID,
          VendorStatus = status,
          //ProductStatus = ProductStatus.NeedsMatch,
          ConcentratorStatusID = -1
          //,
          //VendorStatusID = vendorStatusID
        };
        Table.Add(prStatus);
        Statuses.Add(prStatus);
      }

      return prStatus.ConcentratorStatusID;
    }
  }
}
