using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Magento;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Users;
using System;
using System.Collections.Generic;

namespace Concentrator.Objects.Models.MastergroupMapping
{
  public class MasterGroupMapping : AuditObjectBase<MasterGroupMapping>, ILedgerObject
  {
    public Int32 ComputeDepth()
    {
      return ParentMasterGroupMappingID.HasValue
        ? MasterGroupMappingParent.ComputeDepth() + 1
        : 0;
    }

    public Int32 MasterGroupMappingID { get; set; }

    public Int32? ParentMasterGroupMappingID { get; set; }

    public Int32 ProductGroupID { get; set; }

    public Int32? Score { get; set; }

    public Int32? ConnectorID { get; set; }

    public Int32? ExportID { get; set; }

    public Int32? SourceMasterGroupMappingID { get; set; }

    public Int32? MagentoPageLayoutID { get; set; }

    public Boolean FlattenHierarchy { get; set; }

    public Boolean FilterByParentGroup { get; set; }

    public string BackendMatchingLabel { get; set; }

    public string CustomProductGroupLabel { get; set; }

    public virtual ICollection<ContentProductGroup> ContentProductGroups { get; set; }

    public Int32? SourceProductGroupMappingID { get; set; }

    public virtual ICollection<MasterGroupMapping> MasterGroupMappingChildren { get; set; }

    public virtual MasterGroupMapping MasterGroupMappingParent { get; set; }

    public virtual ProductGroup ProductGroup { get; set; }

    public virtual ICollection<MasterGroupMappingLanguage> MasterGroupMappingLanguages { get; set; }

    public virtual ICollection<MasterGroupMappingProduct> MasterGroupMappingProducts { get; set; }

    public virtual ICollection<ProductAttributeMetaData> ProductAttributeMetaDatas { get; set; }

    public virtual ICollection<MasterGroupMappingMedia> MasterGroupMappingMedias { get; set; }

    public virtual ICollection<ProductGroupVendor> ProductGroupVendors { get; set; }

    public virtual ICollection<User> Users { get; set; }

    public virtual ICollection<ProductGroupMapping> ProductGroupMappings { get; set; }

    public virtual ICollection<MasterGroupMapping> MasterGroupMappings { get; set; }

    public virtual ICollection<MasterGroupMapping> MasterGroupMappingCrossReferences { get; set; }

    public virtual ICollection<MasterGroupMappingRelatedAttribute> MasterGroupMappingRelatedAttributes { get; set; }

    public virtual Connector Connector { get; set; }

    public virtual ICollection<MasterGroupMapping> ConnectorMasterGroupMappings { get; set; }

    public virtual MasterGroupMapping SourceMasterGroupMapping { get; set; }

    public virtual ICollection<ConnectorPublicationRule> ConnectorPublicationRules { get; set; }

    public virtual ICollection<MagentoProductGroupSetting> MagentoProductGroupSettings { get; set; }

    public virtual MasterGroupMapping SourceProductGroupMapping { get; set; }

    public virtual ICollection<MasterGroupMapping> ChildProductGroupMappings { get; set; }

    public virtual ICollection<Connector> Connectors { get; set; }

    public virtual MagentoPageLayout MagentoPageLayout { get; set; }

    public virtual ICollection<MasterGroupMappingDescription> MasterGroupMappingDescriptions { get; set; }

    public virtual ICollection<MasterGroupMappingCustomLabel> MasterGroupMappingCustomLabels { get; set; }

    public virtual ICollection<MasterGroupMappingSettingValue> MasterGroupMappingSettingValues { get; set; }

    public virtual ICollection<MasterGroupMappingLabel> MasterGroupMappingLabels { get; set; }

    

    public override System.Linq.Expressions.Expression<Func<MasterGroupMapping, bool>> GetFilter()
    {
      return null;
    }

    public string ReturnPrimaryKeyHash()
    {
      return String.Format("{0}-{1}-{2}-{3}-{4}-{5}", MasterGroupMappingID, ParentMasterGroupMappingID, ProductGroupID, ConnectorID, SourceMasterGroupMappingID, SourceMasterGroupMappingID);
    }
  }
}
