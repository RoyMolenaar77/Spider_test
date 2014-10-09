using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.MastergroupMapping;
using Concentrator.Objects.Models.Contents;

namespace Concentrator.Objects.Models.Attributes
{
  public class ProductAttributeMetaData : AuditObjectBase<ProductAttributeMetaData>
  {
    public int ProductID { get; set; }
    public Int32 AttributeID { get; set; }

    public String AttributeCode { get; set; }

    public Int32 ProductAttributeGroupID { get; set; }

    public String FormatString { get; set; }

    public String DataType { get; set; }

    public Int32? Index { get; set; }

    public Boolean IsVisible { get; set; }

    public Boolean NeedsUpdate { get; set; }

    public Int32? VendorID { get; set; }

    public Boolean IsSearchable { get; set; }

    public String Sign { get; set; }

    public String AttributePath { get; set; }

    public Boolean Mandatory { get; set; }

    public String DefaultValue { get; set; }

    public Boolean IsConfigurable { get; set; }

    public bool IsSlider { get; set; }

    public Boolean?  HasOption { get; set; }

		public int? ConfigurablePosition { get; set; }

    public string FrontendType { get; set; }

    public virtual ICollection<AttributeMatchStore> AttributeMatchStores { get; set; }

    public virtual ICollection<ProductAttributeDescription> ProductAttributeDescriptions { get; set; }

    public virtual ProductAttributeGroupMetaData ProductAttributeGroupMetaData { get; set; }

    public virtual Vendor Vendor { get; set; }

    public virtual ICollection<ProductAttributeValue> ProductAttributeValues { get; set; }

    public virtual ICollection<ProductAttributeName> ProductAttributeNames { get; set; }

    public virtual ICollection<MasterGroupMapping> MasterGroupMappings { get; set; }

    public virtual ICollection<MasterGroupMappingRelatedAttribute> MasterGroupMappingRelatedAttributes { get; set; }

    public virtual ICollection<ProductAttributeValueConnectorValueGroup> ProductAttributeValueConnectorValueGroups { get; set; }

    public virtual ICollection<Product> Products { get; set; }

    public virtual ICollection<ConnectorPublicationRule> ConnectorPublicationRules { get; set; }

    public virtual ICollection<ProductAttributeValueLabel> ProductAttributeValueLabels { get; set; }

    public virtual ICollection<ConnectorPublication> ConnectorPublications { get; set; }

    public virtual ICollection<ProductAttributeOption> ProductAttributeOptions { get; set; }

    

    public virtual ICollection<ContentPrice> ContentPrices { get; set; }

    public override System.Linq.Expressions.Expression<Func<ProductAttributeMetaData, bool>> GetFilter()
    {
      if (VendorID.HasValue)
        return (p => Client.User.VendorIDs.Contains((int)p.VendorID));
      else
        return null;
    }

    public override string ToString()
    {
      return String.Format("{0}: {1}, VendorID: {2}", AttributeID, AttributeCode, VendorID);
    }
  }
}