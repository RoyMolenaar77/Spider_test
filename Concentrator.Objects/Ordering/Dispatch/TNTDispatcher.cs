using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Ordering.Dispatch
{
	public class TNTDispatcher : IDispatchable
	{
		public int DispatchOrders(Dictionary<Models.Orders.Order, List<Models.Orders.OrderLine>> orderLines, Models.Vendors.Vendor vendor, AuditLog4Net.Adapter.IAuditLogAdapter log, DataAccess.UnitOfWork.IUnitOfWork uni)
		{
			foreach (var ord in orderLines)
			{
				Console.WriteLine(string.Format("{0} : {1}", ord.Key, string.Join(", ", ord.Value.Select(c => c.CustomerItemNumber).ToArray())));
			}
			return 1;
		}

		public void GetAvailableDispatchAdvices(Models.Vendors.Vendor vendor, AuditLog4Net.Adapter.IAuditLogAdapter log, string logPath, DataAccess.UnitOfWork.IUnitOfWork unit)
		{
			throw new NotImplementedException();
		}

		public void CancelOrder(Models.Vendors.Vendor vendor, AuditLog4Net.Adapter.IAuditLogAdapter log, string logPath)
		{
			throw new NotImplementedException();
		}

		public void LogOrder(object orderInformation, int vendorID, string fileName, AuditLog4Net.Adapter.IAuditLogAdapter log)
		{
			throw new NotImplementedException();
		}
	}
}
