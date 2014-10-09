using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Products
{
  public class AssortmentContentView : BaseModel<AssortmentContentView>
  {
    public Int32 ConnectorID { get; set; }

    public Int32 ProductID { get; set; }

    public Int32? ParentProductID { get; set; }

    public String VendorItemNumber { get; set; }

    public Int32 BrandID { get; set; }

    public String BrandName { get; set; }

    public String VendorBrandCode { get; set; }

    public String ShortDescription { get; set; }

    public String LongDescription { get; set; }

    public String LineType { get; set; }

    public String LedgerClass { get; set; }

    public String ProductDesk { get; set; }

    public Boolean? ExtendedCatalog { get; set; }

    public Int32? ProductContentID { get; set; }

    public Int32 VendorID { get; set; }

    public DateTime? CutOffTime { get; set; }

    public Int32? DeliveryHours { get; set; }

    public String CustomItemNumber { get; set; }

    public String ConnectorStatus { get; set; }

    public DateTime? PromisedDeliveryDate { get; set; }

    public Int32? QuantityToReceive { get; set; }

    public Int32 QuantityOnHand { get; set; }

    public Int32 ProductContentIndex { get; set; }

    public Int32? ProductName { get; set; }

    public bool IsConfigurable { get; set; }

    public bool? IsNonAssortmentItem { get; set; }

    public bool Visible { get; set; }

    public override System.Linq.Expressions.Expression<Func<AssortmentContentView, bool>> GetFilter()
    {
      return (a => Client.User.VendorIDs.Contains(a.VendorID));      
    }
  }
}