using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Vendors
{
  public class VendorAssortment : BaseModel<VendorAssortment>
  {

    public Int32 VendorAssortmentID { get; set; }

    public Int32 ProductID { get; set; }

    public String CustomItemNumber { get; set; }

    public Int32 VendorID { get; set; }

    public String ShortDescription { get; set; }

    public String LongDescription { get; set; }

    public String LineType { get; set; }

    public String LedgerClass { get; set; }

    public Boolean? ExtendedCatalog { get; set; }

    public String ProductDesk { get; set; }

    public String ActivationKey { get; set; }

    public String ZoneReferenceID { get; set; }

    public String ShipmentRateTableReferenceID { get; set; }

    public Boolean IsActive { get; set; }

    public virtual ICollection<ContentLedger> ContentLedgers { get; set; }

    public virtual Products.Product Product { get; set; }

    public virtual Vendor Vendor { get; set; }

    public virtual ICollection<VendorAccruel> VendorAccruels { get; set; }

    public virtual ICollection<VendorFreeGood> VendorFreeGoods { get; set; }

    public virtual ICollection<VendorPrice> VendorPrices { get; set; }

    public virtual ICollection<ProductGroupVendor> ProductGroupVendors { get; set; }

    public override System.Linq.Expressions.Expression<Func<VendorAssortment, bool>> GetFilter()
    {
      return (v => Client.User.VendorIDs.Contains(v.VendorID));
    }

    public override String ToString()
    {
      return String.Format("{0}: '{1}' - '{2}'", VendorAssortmentID, CustomItemNumber, Vendor.Name);
    }
  }

  public class VendorAssortmentWehkampSent : VendorAssortment
  {
    public String SentToWehkamp { get; set; }
    public String SentToWehkampAsDummy { get; set; }
  }
}