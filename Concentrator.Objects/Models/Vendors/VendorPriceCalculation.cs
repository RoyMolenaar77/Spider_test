using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Vendors
{
  public class VendorPriceCalculation : AuditObjectBase<VendorPriceCalculation>
  {
    public int VendorPriceCalculationID { get; set; }

    public string Name { get; set; }

    public string Calculation { get; set; }


    public virtual ICollection<VendorPriceRule> VendorPriceRules { get; set; }

    public override System.Linq.Expressions.Expression<Func<VendorPriceCalculation, bool>> GetFilter()
    {
      return null;
    }
  }
}
