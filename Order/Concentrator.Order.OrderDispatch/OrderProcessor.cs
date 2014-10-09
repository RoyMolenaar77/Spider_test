using System.Linq;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Ordering.Rules;
using Concentrator.Plugins.Monitoring;



namespace Concentrator.Order.OrderDispatch
{
  public class OrderProcessor : ConcentratorPlugin
  {
    private readonly Monitoring _monitoring = new Monitoring();

    private const string vendorSettingType = "DispatcherType";

    public override string Name
    {
      get { return "Order Processor"; }
    }

    protected override void Process()
    {
      _monitoring.Notify(Name, 0);

      using (var unit = GetUnitOfWork())
      {
        var orders = unit
          .Scope
          .Repository<OrderLine>()
          .GetAll(o => (o.Order.OrderType == (int)OrderTypes.SalesOrder || o.Order.OrderType == (int)OrderTypes.PurchaseOrder || o.Order.OrderType == (int)OrderTypes.PickTicketOrder) && !o.isDispatched && !o.Order.HoldOrder && (!o.CentralDelivery.HasValue || (o.CentralDelivery.HasValue && !o.CentralDelivery.Value)))
          .ToList();

        log.AuditInfo(string.Format("Find {0} orders to dispatch", orders.Count));
        var rulePipe = new RulePipeline(orders, unit, log);
        log.AuditInfo("Finished order dispatch logic");
        log.InfoFormat("Found {0} orders to process", orders.Count());
        log.Info("Order logic finished lets dispatch");
        rulePipe.Dispatch(true);
        log.AuditInfo("Orders dispatched to vendor");
        foreach (var e in rulePipe.Exceptions)
        {
          _monitoring.Notify(Name, -1);
          log.AuditError(e.Message, e.InnerException ?? e, "Order Dispatching");
        }

        log.AuditComplete(string.Format("Finished order dispatching process. Dispatched Order Lines: {0} Failed: {1}", rulePipe.DispatchedOrders, rulePipe.FailedOrders), "Order Dispatching");
      }

      _monitoring.Notify(Name, 1);
    }
  }
}
