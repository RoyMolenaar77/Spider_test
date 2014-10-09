using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Contents
{
  public class MissingContent : BaseModel<MissingContent>
  {
    public int ConcentratorProductID { get; set; }

    public int ConnectorID { get; set; }

    public bool isActive { get; set; }

    public bool HasFrDescription { get; set; }

    public string VendorItemNumber { get; set; }

    public string CustomItemNumber { get; set; }

    public string BrandName { get; set; }

    public string ShortDescription { get; set; }

    public int ProductGroupID { get; set; }

    public int BrandID { get; set; }

    public bool Image { get; set; }

    public bool YouTube { get; set; }

    public bool Specifications { get; set; }

    public string ContentVendor { get; set; }

    public int? ContentVendorID { get; set; }

    public string Barcode { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? LastModificationTime { get; set; }

    public Int32 QuantityOnHand { get; set; }

    public virtual Product Product { get; set; }

    public virtual Connector Connector { get; set; }

    public virtual ProductGroup ProductGroup { get; set; }

    public virtual Vendor Vendor { get; set; }

    public virtual Brand Brand { get; set; }

    public bool IsConfigurable { get; set; }

    public bool HasDescription { get; set; }    

    public override System.Linq.Expressions.Expression<Func<MissingContent, bool>> GetFilter()
    {
      return null;
    }
  }
}
