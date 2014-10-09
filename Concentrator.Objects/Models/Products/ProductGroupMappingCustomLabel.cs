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
using Concentrator.Objects.Models.Localization;

namespace Concentrator.Objects.Models.Products
{
  public class ProductGroupMappingCustomLabel : BaseModel<ProductGroupMappingCustomLabel>
  {
    public int ProductGroupMappingCustomLabelID { get; set; }

    public int ProductGroupMappingID { get; set; }

    public int LanguageID { get; set; }

    public string CustomLabel { get; set; }

    public int ? ConnectorID { get; set; }

    public virtual ProductGroupMapping ProductGroupMapping { get; set; }

    public virtual Language Language { get; set; }

    public virtual Connector Connector { get; set; }

    public override System.Linq.Expressions.Expression<Func<ProductGroupMappingCustomLabel, bool>> GetFilter()
    {
      return null;
    }
  }
}