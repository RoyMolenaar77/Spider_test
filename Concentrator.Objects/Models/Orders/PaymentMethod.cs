using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Orders
{
	public class PaymentMethod : BaseModel<PaymentMethod>
	{
		public int ID { get; set; }

		public string Code { get; set; }

		public virtual IList<PaymentMethodDescription> PaymentMethodDescriptions { get; set; }

		public override System.Linq.Expressions.Expression<Func<PaymentMethod, bool>> GetFilter()
		{
			return null;
		}
	}
}
