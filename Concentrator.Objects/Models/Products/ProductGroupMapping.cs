using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Scan;
using Concentrator.Objects.Models.Slurp;
using Concentrator.Objects.Models.Magento;

namespace Concentrator.Objects.Models.Products
{
  public class ProductGroupMapping : BaseModel<ProductGroupMapping>
  {
    public override bool Equals(object obj)
    {
      return this.ProductGroupMappingID == ((ProductGroupMapping)obj).ProductGroupMappingID;
    }

    public override int GetHashCode()
    {
      return ProductGroupMappingID;
    }

    public string BackendMatchingLabel { get; set; }

    public bool IncludeAllMappingProducts { get; set; }

    public Int32 ProductGroupMappingID { get; set; }

    public Int32 MasterGroupMappingID { get; set; }

    public Int32? ParentProductGroupMappingID { get; set; }

    public Int32 ConnectorID { get; set; }

    public Int32 ProductGroupID { get; set; }

    public Boolean FlattenHierarchy { get; set; }

    public Boolean FilterByParentGroup { get; set; }

    public Int32 Depth { get; set; }

    public String Lineage { get; set; }

    public Int32? Score { get; set; }

    public String CustomProductGroupLabel { get; set; }

    public String ProductGroupMappingLabel { get; set; }

    public String ProductGroupMappingPath { get; set; }

    public String Relation { get; set; }

    public Int32? MagentoPageLayoutID { get; set; }

    public string MappingThumbnailImagePath { get; set; }

    public virtual Connector Connector { get; set; }

    public virtual ICollection<ContentProductGroup> ContentProductGroups { get; set; }

    public virtual ProductGroup ProductGroup { get; set; }

    public virtual ICollection<ProductGroupMapping> ChildMappings { get; set; }

    public virtual ProductGroupMapping ParentMapping { get; set; }

    public virtual ICollection<ScanData> ScanDatas { get; set; }

    public virtual ICollection<Connector> Connectors { get; set; }

    public virtual ICollection<SlurpSchedule> SlurpSchedules { get; set; }

    public virtual ICollection<MagentoProductGroupSetting> MagentoProductGroupSettings { get; set; }

    public virtual ICollection<ProductGroupMappingDescription> ProductGroupMappingDescriptions { get; set; }

    public virtual ICollection<ProductGroupMappingCustomLabel> ProductGroupMappingCustomLabels { get; set; }

    public virtual MagentoPageLayout MagentoPageLayout { get; set; }

    public override System.Linq.Expressions.Expression<Func<ProductGroupMapping, bool>> GetFilter()
    {
      return null;
    }
  }
}