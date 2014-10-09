using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Products
{
  public class ProductGroupConnectorVendor: BaseModel<ProductGroupConnectorVendor>
  {
    public Int32 ProductGroupID { get; set; }
          
    public Int32 ConnectorID { get; set; }
          
    public Boolean isPreferredAssortmentVendor { get; set; }
          
    public Boolean isPreferredContentVendor { get; set; }
          
    public Int32 VendorID { get; set; }
          
    public virtual ProductGroup ProductGroup { get;set;}
            
    public virtual Connector Connector { get;set;}
            
    public virtual Vendor Vendor { get;set;}


    public override System.Linq.Expressions.Expression<Func<ProductGroupConnectorVendor, bool>> GetFilter()
    {
      return (p => Client.User.VendorIDs.Contains(p.VendorID));
    }
  }
}