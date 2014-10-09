using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Statuses;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.MastergroupMapping;

namespace Concentrator.Objects.Models.Connectors
{

  public enum ConnectorPublicationRuleType
  {
    Include = 1,
    Exclude = 0
  }

  public class ConnectorPublicationRule : AuditObjectBase<ConnectorPublicationRule>
  {
    public Int32 ConnectorPublicationRuleID { get; set; }
          
    public Int32 ConnectorID { get; set; }
          
    public Int32 VendorID { get; set; }
          
    public Int32? ProductGroupID { get; set; }
          
    public Int32? BrandID { get; set; }
          
    public Int32? ProductID { get; set; }
          
    public Boolean? PublishOnlyStock { get; set; }

    public Int32 PublicationIndex { get; set; }
          
    public Int32? StatusID { get; set; }

    public Int32? ConnectorRelationID { get; set; }
          
    public DateTime? FromDate { get; set; }
          
    public DateTime? ToDate { get; set; }

    public decimal? FromPrice { get; set; }
    public decimal? ToPrice { get; set; }

    public Int32? AttributeID { get; set; }

    public int PublicationType { get; set; }

    public string AttributeValue { get; set; }

    public bool IsActive { get; set; }

    public Int32? MasterGroupMappingID { get; set; }

    public bool OnlyApprovedProducts { get; set; }
    
    public virtual AssortmentStatus AssortmentStatus { get;set;}
            
    public virtual Brand Brand { get;set;}
            
    public virtual Connector Connector { get;set;}
            
    public virtual Product Product { get;set;}
            
    public virtual ProductGroup ProductGroup { get;set;}
            
    public virtual Vendor Vendor { get;set;}

    public virtual ProductAttributeMetaData ProductAttributeMetaData { get; set; }

    public virtual ConnectorRelation ConnectorRelation { get; set; }

    public virtual ICollection<Content> Contents { get; set; }

    public virtual MasterGroupMapping MasterGroupMapping { get; set; }

    public virtual ICollection<MasterGroupMappingProduct> MasterGroupMappingProducts { get; set; }

    public bool EnabledByDefault { get; set; }

    public override System.Linq.Expressions.Expression<Func<ConnectorPublicationRule, bool>> GetFilter()
    {
      return (c => Client.User.VendorIDs.Contains(c.VendorID));
    }
  }
}