using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.EDI.Response;
using Concentrator.Objects.Models.EDI.Enumerations;
using Concentrator.Objects.DataAccess.Repository;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.EDI.Order
{
  public class EdiOrderLine : BaseModel<EdiOrderLine>
  {
    public Int32 EdiOrderLineID { get; set; }

    public string Remarks { get; set; }

    public Int32 EdiOrderID { get; set; }

    public string CustomerEdiOrderLineNr { get; set; }

    public string CustomerOrderNr { get; set; }

    public Int32? ProductID { get; set; }

    public Double? Price { get; set; }

    public Int32 Quantity { get; set; }

    public bool isDispatched { get; set; }

    public Int32? DispatchedToVendorID { get; set; }

    public Int32? VendorOrderNumber { get; set; }

    public string Response { get; set; }

    public bool? CentralDelivery { get; set; }

    public string CustomerItemNumber { get; set; }

    public string WareHouseCode { get; set; }

    public bool PriceOverride { get; set; }

    public string EndCustomerOrderNr { get; set; }

    public string Currency { get; set; }

    public string UnitOfMeasure { get; set; }

    public string ProductDescription { get; set; }

    public string EdiProductID { get; set; }

    public string CompanyCode { get; set; }

    public virtual ICollection<EdiOrderLedger> EdiOrderLedgers { get; set; }

    public virtual EdiOrder EdiOrder { get; set; }

    public virtual ICollection<EdiOrderResponseLine> EdiOrderResponseLines { get; set; }

    public int CurrentState()
    {
      var ledger = EdiOrderLedgers.OrderByDescending(x => x.LedgerDate).FirstOrDefault();
      if (ledger != null)
        return ledger.Status;
      else
        return (int)EdiOrderStatus.Received;
    }

    public void SetStatus(EdiOrderStatus status, IUnitOfWork unit)
    {
      if (this.EdiOrderLedgers == null || !this.EdiOrderLedgers.Any(x => x.Status == (int)status))
      {
        EdiOrderLedger ledger = new EdiOrderLedger()
        {
          LedgerDate = DateTime.Now,
          EdiOrderLine = this,
          Status = (int)status
        };

        if (this.EdiOrderLedgers != null)
          this.EdiOrderLedgers.Add(ledger);

        unit.Scope.Repository<EdiOrderLedger>().Add(ledger);
      }
    }

    public override System.Linq.Expressions.Expression<Func<EdiOrderLine, bool>> GetFilter()
    {
      return null;
    }
  }
}
