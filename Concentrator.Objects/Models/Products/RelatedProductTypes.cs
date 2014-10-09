using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using Concentrator.Objects.DataAccess.Repository;

namespace Concentrator.Objects.Models.Products
{
  public class RelatedProductTypes
  {
    private IRepository<RelatedProductType> Table;
    private List<RelatedProductType> _relatedProductTypes;
    /// <summary>
    /// Initializes a new instance of product status mapper
    /// </summary>
    /// <param name="statusCollection">Collection of all vendorProductStatuses (context.VendorProductStatus)</param>
    public RelatedProductTypes(IRepository<RelatedProductType> types)
    {
      Table = types;
      _relatedProductTypes = types.GetAll().ToList();
    }

    public RelatedProductType SyncRelatedProductTypes(string type)
    {
      //if (string.IsNullOrEmpty(status)) return defaultStatus;

      var rType = _relatedProductTypes.FirstOrDefault(c => c.Type == type);

      if (rType == null)
      {
        rType = new RelatedProductType
        {
          Type = type
        };
        Table.Add(rType);
        _relatedProductTypes.Add(rType);
      }

      return rType;
    }
  }
}
