using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.MastergroupMapping;

namespace Concentrator.Objects.Models.Magento
{
  public class MagentoPageLayout : AuditObjectBase<MagentoPageLayout>
  {
    public Int32 LayoutID { get; set; }

    public String LayoutName { get; set; }

    public String LayoutCode { get; set; }

    public virtual ICollection<ProductGroupMapping> ProductGroupMappings { get; set; }

    public virtual ICollection<MasterGroupMapping> MasterGroupMappings { get; set; }

    public override System.Linq.Expressions.Expression<Func<MagentoPageLayout, bool>> GetFilter()
    {
      return null;
    }
  }
}