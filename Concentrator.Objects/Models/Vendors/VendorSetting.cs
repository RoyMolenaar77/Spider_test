using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Vendors
{
	public class VendorSetting : BaseModel<VendorSetting>
	{
		public Int32 VendorID { get; set; }

		public String SettingKey { get; set; }

		public String Value { get; set; }

		public virtual Vendor Vendor { get; set; }


		public override System.Linq.Expressions.Expression<Func<VendorSetting, bool>> GetFilter()
		{
			return null;

		}
	}
}