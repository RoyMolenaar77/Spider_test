using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Order.Objects.Dispatch
{
    public class CentraalBoekhuisDispatcher : IDispatchable
    {
        public int DispatchOrders(Dictionary<Concentrator.Objects.Models.Orders.Order, List<Concentrator.Objects.Models.Orders.OrderLine>> orderLines, Concentrator.Objects.Models.Vendors.Vendor vendor, AuditLog4Net.Adapter.IAuditLogAdapter log, Concentrator.Objects.DataAccess.UnitOfWork.IUnitOfWork uni)
        {
            throw new NotImplementedException();
        }

        public void GetAvailableDispatchAdvices(Concentrator.Objects.Models.Vendors.Vendor vendor, AuditLog4Net.Adapter.IAuditLogAdapter log, string logPath, Concentrator.Objects.DataAccess.UnitOfWork.IUnitOfWork unit)
        {
            throw new NotImplementedException();
        }

        public void CancelOrder(Concentrator.Objects.Models.Vendors.Vendor vendor, AuditLog4Net.Adapter.IAuditLogAdapter log, string logPath)
        {
            throw new NotImplementedException();
        }
    }
}
