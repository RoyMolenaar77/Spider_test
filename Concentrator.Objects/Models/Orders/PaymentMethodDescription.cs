using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Orders
{
	public class PaymentMethodDescription : BaseModel<PaymentMethodDescription>
	{
		public int PaymentMethodID { get; set; }
		public int LanguageID { get; set; }
		public string Description { get; set; }

		public virtual Language Language { get; set; }
		public virtual PaymentMethod PaymentMethod { get; set; }

		public override System.Linq.Expressions.Expression<Func<PaymentMethodDescription, bool>> GetFilter()
		{
			return null;
		}
	}
}
