using System;
using System.Linq;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Statuses;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Vendors 
{
  public class VendorStock : BaseModel<VendorStock>
  {
    public Int32 ProductID { get; set; }
          
    public Int32 VendorID { get; set; }
          
    public Int32 QuantityOnHand { get; set; }
          
    public DateTime? PromisedDeliveryDate { get; set; }
          
    public Int32? QuantityToReceive { get; set; }
          
    public Int32 VendorStockTypeID { get; set; }
          
    public String StockStatus { get; set; }
          
    public Decimal? UnitCost { get; set; }
          
    public Int32? ConcentratorStatusID { get; set; }
          
    public String VendorStatus { get; set; }
          
    public virtual AssortmentStatus AssortmentStatus { get;set;}
            
    public virtual Product Product { get;set;}
            
    public virtual Vendor Vendor { get;set;}
            
    public virtual VendorStockType VendorStockType { get;set;}

    public override System.Linq.Expressions.Expression<Func<VendorStock, bool>> GetFilter()
    {
      return (v => Client.User.VendorIDs.Contains(v.VendorID));

    }
  }
}