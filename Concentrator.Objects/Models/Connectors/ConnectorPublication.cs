using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Statuses;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Attributes;

namespace Concentrator.Objects.Models.Connectors
{
  public class ConnectorPublication : AuditObjectBase<ConnectorPublication>
  {
    public Int32 ConnectorPublicationID { get; set; }
          
    public Int32 ConnectorID { get; set; }
          
    public Int32 VendorID { get; set; }
          
    public Int32? ProductGroupID { get; set; }
          
    public Int32? BrandID { get; set; }

    public int? MinimumStock { get; set; }

    public Int32? ProductID { get; set; }
          
    public Boolean? Publish { get; set; }
          
    public Boolean? PublishOnlyStock { get; set; }
          
    public Int32 ProductContentIndex { get; set; }
          
    public Int32? StatusID { get; set; }
          
    public DateTime? FromDate { get; set; }
          
    public DateTime? ToDate { get; set; }
          
    public virtual AssortmentStatus AssortmentStatus { get;set;}
            
    public virtual Brand Brand { get;set;}
            
    public virtual Connector Connector { get;set;}
            
    public virtual Products.Product Product { get;set;}
            
    public virtual ProductGroup ProductGroup { get;set;}
            
    public virtual Vendor Vendor { get;set;}

    public Int32? AttributeID { get; set; }

    public virtual ProductAttributeMetaData ProductAttributeMetaData { get; set; }

    public string AttributeValue { get; set; }

    public override System.Linq.Expressions.Expression<Func<ConnectorPublication, bool>> GetFilter()
    {
      return (c => Client.User.VendorIDs.Contains(c.VendorID));

    }
  }
}