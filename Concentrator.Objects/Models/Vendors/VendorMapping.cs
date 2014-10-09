using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Vendors
{
  public class VendorMapping : AuditObjectBase<VendorMapping>
  {
    public Int32 VendorMappingID { get; set; }
          
    public Int32 VendorID { get; set; }
          
    public Int32 MapVendorID { get; set; }
          
    public virtual Vendor Vendor { get;set;}
            
    public virtual Vendor Vendor1 { get;set;}


    public override System.Linq.Expressions.Expression<Func<VendorMapping, bool>> GetFilter()
    {
      return (v => Client.User.VendorIDs.Contains(v.VendorID));
      
    }
  }
}