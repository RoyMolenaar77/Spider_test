using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Orders
{
	public class OrderLineAppliedDiscountRule : BaseModel<OrderLineAppliedDiscountRule>
	{
		public int AppliedRuleID { get; set; }

		public string Code { get; set; }

		public int RuleID { get; set; }

		public decimal DiscountAmount { get; set; }

		public bool Percentage { get; set; }

		public int OrderLineID { get; set; }

		public bool IsSet { get; set; }

		public virtual OrderLine OrderLine { get; set; }

		public override System.Linq.Expressions.Expression<Func<OrderLineAppliedDiscountRule, bool>> GetFilter()
		{
			return null;
		}
	}
}
