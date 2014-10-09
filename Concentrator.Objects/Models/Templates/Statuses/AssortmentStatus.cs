using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Contents;

namespace Concentrator.Objects.Models.Statuses
{
  public class AssortmentStatus : BaseModel<AssortmentStatus>
  {
    public Int32 StatusID { get; set; }

    public String Status { get; set; }

    public virtual ICollection<ConnectorProductStatus> ConnectorProductStatus { get; set; }

    public virtual ICollection<ConnectorPublication> ConnectorPublications { get; set; }

    public virtual ICollection<VendorPrice> VendorPrices { get; set; }

    public virtual ICollection<VendorProductStatus> VendorProductStatus { get; set; }

    public virtual ICollection<VendorStock> VendorStocks { get; set; }

    public virtual ICollection<ContentPublicationRule> ContentPublicationRules { get; set; }

    public virtual ICollection<ConnectorPublicationRule> ConnectorPublicationRules { get; set; }

    public override System.Linq.Expressions.Expression<Func<AssortmentStatus, bool>> GetFilter()
    {
      return null;
    }
  }
}