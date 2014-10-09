using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.Objects.Models.Contents
{
  public class ContentLedger : AuditObjectBase<ContentLedger>
  {
    public Int32 LedgerID { get; set; }
          
    public Int32 ProductID { get; set; }
          
    public DateTime LedgerDate { get; set; }
          
    public Decimal? UnitPrice { get; set; }
          
    public Decimal? CostPrice { get; set; }
          
    public Decimal? TaxRate { get; set; }
          
    public Int32? ConcentratorStatusID { get; set; }
          
    public Int32? MinimumQuantity { get; set; }
          
    public Int32? VendorAssortmentID { get; set; }
          
    public String Remark { get; set; }
          
    public String LedgerObject { get; set; }
          
    public Decimal? Margin { get; set; }

    public Decimal? BasePrice { get; set; }

    public Decimal? BaseCostPrice { get; set; }
          
    public virtual Products.Product Product { get;set;}
            
    public virtual VendorAssortment VendorAssortment { get;set;}


    public override System.Linq.Expressions.Expression<Func<ContentLedger, bool>> GetFilter()
    {
      return null;
    }
  }
}