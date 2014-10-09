using System;
using System.Collections.Generic;
using System.Linq;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.DataAccess.Repository;

namespace Concentrator.Objects.Models.Orders
{
	public class OrderLine : BaseModel<OrderLine>
	{
		public Int32 OrderLineID { get; set; }

		public String Remarks { get; set; }

		public Int32 OrderID { get; set; }

		public String CustomerOrderLineNr { get; set; }

		public String CustomerOrderNr { get; set; }

		public Int32? ProductID { get; set; }

		public Double? Price { get; set; }

		public Int32 Quantity { get; set; }

		public Boolean isDispatched { get; set; }

		public Int32? DispatchedToVendorID { get; set; }

		public Int32? VendorOrderNumber { get; set; }

		public String Response { get; set; }

		public Boolean? CentralDelivery { get; set; }

		public String CustomerItemNumber { get; set; }

		public String WareHouseCode { get; set; }

		public Boolean PriceOverride { get; set; }

		public Double? LineDiscount { get; set; }

		public Double? UnitPrice { get; set; }

		public Double? BasePrice { get; set; }

    public string OriginalLine { get; set; }

    public Decimal? TaxRate
    {
      get;
      set;
    }

	  public virtual Order Order { get; set; }

		public virtual ICollection<OrderLedger> OrderLedgers { get; set; }

		public virtual Products.Product Product { get; set; }

		public virtual Vendor Vendor { get; set; }

		public virtual ICollection<OrderResponseLine> OrderResponseLines { get; set; }

		public virtual ICollection<OrderLineAppliedDiscountRule> OrderLineAppliedDiscountRules { get; set; }

		public int CurrentState()
		{
			var ledger = OrderLedgers.FirstOrDefault(x => x.Status == (int)OrderLineStatus.Cancelled && (x.Quantity.HasValue && x.Quantity.Value == this.Quantity));

			if (ledger == null)
				ledger = OrderLedgers.OrderByDescending(x => x.LedgerDate).FirstOrDefault();

			if (ledger != null)
				return ledger.Status;
			else
				return (int)OrderLineStatus.New;
		}

		public void SetStatus(OrderLineStatus status, IRepository<OrderLedger> repo, int? quantity = null, bool useStatusOnNonAssortmentItems = false)
		{
			OrderLedger ledger = null;
			if (this.Product.IsNonAssortmentItem.HasValue && this.Product.IsNonAssortmentItem.Value && !this.OrderLedgers.Any(x => x.Status == (int)status))
			{
				int statusNA = (int)OrderLineStatus.ReadyToOrder;

				if (useStatusOnNonAssortmentItems)
				{
					statusNA = (int)status;
				}

				ledger = new OrderLedger()
				{
					LedgerDate = DateTime.Now,
					OrderLineID = this.OrderLineID,
					Status = statusNA,

				};
			}

			else if (!this.OrderLedgers.Any(x => x.Status == (int)status))
			{
				ledger = new OrderLedger()
				{
					LedgerDate = DateTime.Now,
					OrderLineID = this.OrderLineID,
					Status = (int)status,
				};
			}



			if (ledger != null)
			{
				if (quantity.HasValue)
					ledger.Quantity = quantity.Value;

				this.OrderLedgers.Add(ledger);
				repo.Add(ledger);
			}
		}

		public override System.Linq.Expressions.Expression<Func<OrderLine, bool>> GetFilter()
		{
			return null;
		}

		public int GetDispatchQuantity()
		{
			int cancelStatus = (int)OrderLineStatus.Cancelled;
			var qty = this.OrderLedgers.Where(x => x.Quantity.HasValue && x.Status == cancelStatus).Sum(x => x.Quantity.Value);

			return this.Quantity - qty;
		}


	}
}