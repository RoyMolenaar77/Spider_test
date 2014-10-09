using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Scan;
using System.Configuration;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.EDI.Order;
using Concentrator.Objects.Models.EDI.Post;
using Concentrator.Objects.Models.Management;
using Concentrator.Objects.Models.MastergroupMapping;

namespace Concentrator.Objects.Models.Connectors
{
  public class Connector : BaseModel<Connector>
  {
    public Int32 ConnectorID { get; set; }

    public String Name { get; set; }

    public Int32 ConnectorType { get; set; }

    public Int32? ConnectorSystemID { get; set; }

    public Boolean ConcatenateBrandName { get; set; }

    public Boolean ObsoleteProducts { get; set; }

    public Boolean ZipCodes { get; set; }

    public Boolean Selectors { get; set; }

    public Boolean OverrideDescriptions { get; set; }

    public Int32? BSKIdentifier { get; set; }

    public String BackendEanIdentifier { get; set; }

    public Boolean UseConcentratorProductID { get; set; }

    public String Connection { get; set; }

    public Boolean ImportCommercialText { get; set; }

    public Boolean IsActive { get; set; }

    public Int32? AdministrativeVendorID { get; set; }

    public String OutboundUrl { get; set; }

    public Int32? ParentConnectorID { get; set; }

    public String DefaultImage { get; set; }

    public Int32? ConnectorSystemType { get; set; }

    public String ConnectorLogoPath { get; set; }

    public bool? IgnoreMissingImage { get; set; }

    public bool? IgnoreMissingConcentratorDescription { get; set; }

    public int OrganizationID { get; set; }

    public virtual Organization  Organization { get; set; }

    public virtual ICollection<AdditionalOrderProduct> AdditionalOrderProducts { get; set; }

    public virtual ICollection<AttributeMatchStore> AttributeMatchStores { get; set; }

    public virtual ICollection<Connector> ChildConnectors { get; set; }

    public virtual ICollection<ProductGroupMapping> ProductGroupMappingsNotActive { get; set; }

    public virtual Connector ParentConnector { get; set; }

    public virtual ConnectorSystem ConnectorSystem { get; set; }

    public virtual Vendor Vendor { get; set; }

    public virtual ICollection<ConnectorLanguage> ConnectorLanguages { get; set; }

    public virtual ICollection<ConnectorPaymentProvider> ConnectorPaymentProviders { get; set; }

    public virtual ICollection<ConnectorPublication> ConnectorPublications { get; set; }

    public virtual ICollection<ConnectorRuleValue> ConnectorRuleValues { get; set; }

    public virtual ICollection<ConnectorSetting> ConnectorSettings { get; set; }

    public virtual ICollection<Contents.Content> Contents { get; set; }

    public virtual ICollection<ContentPrice> ContentPrices { get; set; }

    public virtual ICollection<ContentProduct> ContentProducts { get; set; }

    public virtual ICollection<ContentProductGroup> ContentProductGroups { get; set; }

    public virtual ICollection<ContentVendorSetting> ContentVendorSettings { get; set; }

    public virtual ICollection<CrossLedgerclass> CrossLedgerclasses { get; set; }

    public virtual ICollection<Order> Orders { get; set; }

    public virtual ICollection<Outbound> Outbounds { get; set; }

    public virtual ICollection<PreferredConnectorVendor> PreferredConnectorVendors { get; set; }

    public virtual ICollection<ProductAttributeGroupMetaData> ProductAttributeGroupMetaDatas { get; set; }

    public virtual ICollection<ProductGroupMapping> ProductGroupMappings { get; set; }

    public virtual ICollection<ProductGroupPublish> ProductGroupPublishes { get; set; }

    public virtual ICollection<ScanData> ScanDatas { get; set; }

    public virtual ICollection<ConnectorSchedule> ConnectorSchedules { get; set; }

    public virtual ICollection<User> Users { get; set; }

    public virtual ICollection<ConnectorRelation> ConnectorRelations { get; set; }

    public virtual ICollection<EdiOrder> EdiOrders { get; set; }

    public virtual ICollection<MissingContent> MissingContents { get; set; }

    public virtual ICollection<ProductGroupMappingCustomLabel> ProductGroupMappingCustomLabels { get; set; }

    public virtual ICollection<MasterGroupMappingCustomLabel> MasterGroupMappingCustomLabels { get; set; }

    public virtual ICollection<EdiOrderPost> EdiOrderPosts { get; set; }

    public virtual ICollection<ConnectorProductStatus> ConnectorProductStatuses { get; set; }

    public virtual ICollection<ProductCompare> ProductCompares { get; set; }

    public virtual ICollection<EdiOrderListener> EdiOrderListeners { get; set; }

    public virtual ICollection<ProductGroupConnectorVendor> ProductGroupConnectorVendors { get; set; }

    public virtual ICollection<ConnectorVendorManagementContent> ConnectorVendorManagementContents { get; set; }

    public virtual ICollection<ProductAttributeValueConnectorValueGroup> ProductAttributeValueConnectorValueGroups { get; set; }

    public virtual ICollection<ProductAttributeValueLabel> ProductAttributeValueLabels { get; set; }  

    public virtual ICollection<PriceSet> PriceSets { get; set; }

    public virtual ICollection<ProductAttributeValueGroup> ProductAttributeValueGroups { get; set; }

    public virtual ICollection<ExcludeProduct> ExcludeProducts { get; set; }

    public virtual ICollection<ContentStock> ContentStocks { get; set; }

    public virtual ICollection<ConnectorPublicationRule> ConnectorPublicationRules { get; set; }

    public virtual ICollection<MasterGroupMapping> MasterGroupMappings { get; set; }

    public virtual ICollection<MasterGroupMapping> NotActiveMasterGroupMappings { get; set; }

    public virtual ICollection<MasterGroupMappingMedia> MasterGroupMappingMedias { get; set; }

    public string ConnectionString
    {
      get
      {
        if (ConfigurationManager.ConnectionStrings[Connection] != null)
          return ConfigurationManager.ConnectionStrings[Connection].ConnectionString;
        else
          return Connection;
      }
    }

    public override System.Linq.Expressions.Expression<Func<Connector, bool>> GetFilter()
    {
      return null;
    }

    public override string ToString()
    {
      return String.Format("Connector {0}: '{1}' ({2})", ConnectorID, Name, ConnectorSystem.Name);
    }
  }
}