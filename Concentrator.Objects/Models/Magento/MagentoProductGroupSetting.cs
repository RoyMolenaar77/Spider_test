using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.MastergroupMapping;

namespace Concentrator.Objects.Models.Magento
{
  public class MagentoProductGroupSetting : AuditObjectBase<MagentoProductGroupSetting>
  {
    public Int32 MagentoProductGroupSettingID { get; set; }

    public Int32? ProductGroupmappingID { get; set; }

    public Int32? MasterGroupMappingID { get; set; }

    public Boolean? ShowInMenu { get; set; }

    public Boolean? DisabledMenu { get; set; }

    public Boolean? IsAnchor { get; set; }

    public virtual ProductGroupMapping ProductGroupMapping { get; set; }

    public virtual MasterGroupMapping MasterGroupMapping { get; set; }

    public override System.Linq.Expressions.Expression<Func<MagentoProductGroupSetting, bool>> GetFilter()
    {
      return null;
    }
  }
}