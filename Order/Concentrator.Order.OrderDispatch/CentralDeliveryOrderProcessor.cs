using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using System.Xml.Linq;
using System.Reflection;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Ordering.Rules;
using Concentrator.Objects.Ordering.Dispatch;

namespace Concentrator.Order.OrderDispatch
{
  public class CentralDeliveryOrderProcessor : ConcentratorPlugin
  {
    private const string vendorSettingType = "DispatcherType";

    public override string Name
    {
      get { return "Central Delivery Order Processor"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        var orders = unit.Scope.Repository<OrderLine>().GetAll(o => !o.isDispatched && (o.CentralDelivery.HasValue && o.CentralDelivery.Value)).ToList();

        var rulePipe = new RulePipeline(orders, unit);
        rulePipe.Dispatch(false);
        foreach (var e in rulePipe.Exceptions)
        {
          log.AuditError(e.Message, e.InnerException ?? e, "Order Dispatching");
        }
        log.AuditComplete(string.Format("Finished order dispatching process. Dispatched Order Lines: {0} Failed: {1}", rulePipe.DispatchedOrders, rulePipe.FailedOrders), "Order Dispatching");
      }

      //check if any dispatch advices are available

      try
      {
        CheckDispatchAdvice();
        log.AuditSuccess("Available dispatch advices processed");
      }
      catch (Exception e)
      {
        log.AuditError("Sending dispatch advice to EDI failed", e, "Order Dispatching");
      }

    }

    private void CheckDispatchAdvice()
    {
      using (var unit = GetUnitOfWork())
      {
        var dispatchers = unit.Scope.Repository<Vendor>().GetAll(c => c.ParentVendorID == null
                                 && (c.OrderDispatcherType != null && c.OrderDispatcherType != string.Empty)
                                 && c.IsActive);

        string logPath = GetConfiguration().AppSettings.Settings["XMLlogReceive"].Value;

        foreach (var disp in dispatchers.Where(x => ((VendorType)x.VendorType).Has(VendorType.Assortment)))
        {
          var dispatcher =
            (IDispatchable)
            Activator.CreateInstance(Assembly.GetAssembly(typeof(IDispatchable)).GetType(disp.OrderDispatcherType));
          dispatcher.GetAvailableDispatchAdvices(disp, log, logPath, unit);
        }

      }
    }
  }
}
