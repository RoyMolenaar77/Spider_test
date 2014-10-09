using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Concentrator.Objects;
using AuditLog4Net.Adapter;
using AuditLog4Net.AuditLog;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Utility;
using Concentrator.Objects.Ordering.Vendors;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Ordering.Dispatch;
using Concentrator.Objects.Ordering.Purchase;

namespace Concentrator.Objects.Ordering.Rules
{
  public class RulePipeline
  {
    private List<OrderLine> orderLines;
    private IUnitOfWork unit;
    private List<int> vendorIDs;
    private List<RuleBase> _orderRules;
    protected IAuditLogAdapter log;
    private List<Vendor> vendors;

    /// <summary>
    /// Gets the number of successfully dispatched orders
    /// </summary>
    public int DispatchedOrders { get; private set; }

    /// <summary>
    /// Gets the number of failed orders
    /// </summary>
    public int FailedOrders { get; set; }


    /// <summary>
    /// Gets any exceptions encountered during the dispatching process
    /// </summary>
    public List<Exception> Exceptions { get; private set; }

    /// <summary>
    ///Dictionary containing order lines by vendor to be dispatched to
    /// </summary>
    public Dictionary<int, List<OrderLine>> VendorOrderLines { get; private set; }

    /// <summary>
    /// Gets all order rules from the rule assembly
    /// </summary>
    protected List<RuleBase> OrderRules
    {
      get
      {
        if (_orderRules == null)
        {
          var type = typeof(RuleBase);
          _orderRules = (from t in Assembly.GetAssembly(typeof(RuleBase)).GetTypes()
                         where !t.IsAbstract &&
                                type.IsAssignableFrom(t)
                         select (RuleBase)Activator.CreateInstance(t)).ToList();
        }
        return _orderRules;
      }
    }

    /// <summary>
    /// Initializes a new pipeline and appies all the rules to the order lines.
    /// The VendorOrderLines Dict is populated
    /// </summary>
    /// <param name="OrderLines">A list of order lines to be dispatched</param>
    /// <param name="context">A data context instance to use for lookup and marking of the dispatched orderlines</param>
    public RulePipeline(List<OrderLine> OrderLines, IUnitOfWork unit)
      : this(OrderLines, unit, null, new AuditLogAdapter(log4net.LogManager.GetLogger("OrderProcessor"), new AuditLog(new ConcentratorAuditLogProvider())))
    {

    }

    /// <summary>
    /// Initializes a new pipeline and appies all the rules to the order lines.
    /// The VendorOrderLines Dict is populated
    /// </summary>
    /// <param name="OrderLines">A list of order lines to be dispatched</param>
    /// <param name="context">A data context instance to use for lookup and marking of the dispatched orderlines</param>
    public RulePipeline(List<OrderLine> OrderLines, IUnitOfWork unit, IAuditLogAdapter log)
      : this(OrderLines, unit, null, log)
    {

    }

    /// <summary>
    /// Initializes a new pipeline and appies all the rules to the order lines.
    /// The VendorOrderLines Dict is populated
    /// </summary>
    /// <param name="OrderLines">A list of order lines to be dispatched</param>
    /// <param name="context">A data context instance to use for lookup and marking of the dispatched orderlines</param>
    /// <param name="VendorIDs">All vendor ids to include when applying the rules.Defaults to all assortment vendors</param>
    public RulePipeline(List<OrderLine> OrderLines, IUnitOfWork unit, List<int> VendorIDs, IAuditLogAdapter log)
    {
      orderLines = OrderLines;
      this.unit = unit;
      this.log = log;

      if (VendorIDs == null)
      {

        var v = this.unit.Scope.Repository<Vendor>().GetAll().ToList();

        vendors = v;
        vendorIDs = v.Where(x => ((VendorType)x.VendorType).Has(VendorType.Assortment) && x.IsActive && x.OrderDispatcherType != null).Select(x => x.VendorID).ToList();

      }
      else
        vendorIDs = VendorIDs;


      VendorOrderLines = new Dictionary<int, List<OrderLine>>();
      Exceptions = new List<Exception>();
      LoadVendorOrderRules();
    }

    public void Dispatch(bool directShipment)
    {
      if (orderLines.Count > 0)
        DispatchOrders(directShipment);

      orderLines.ForEach(c =>
      {
        if (c.Order.OrderLines.All(o => o.isDispatched))
        {
          c.Order.IsDispatched = true;
          c.Order.DispatchToVendorDate = DateTime.Now.ToUniversalTime();
        }
      });
      unit.Save();
    }


    /// <summary>
    /// Retrieves a grouped collection of orderlines per order
    /// </summary>
    /// <param name="orderLines"></param>
    /// <returns></returns>
    protected Dictionary<Concentrator.Objects.Models.Orders.Order, List<OrderLine>> GetGroupedOrders(List<OrderLine> orderLines, OrderLineStatus status)
    {
      return (from m in orderLines
#if !DEBUG
              where m.CurrentState() == (int)status
#endif
              group m by m.OrderID
                into g
                select new
                {
                  Order = g.Key,
                  OrderLines = g.ToList()
                }).ToDictionary(c => orderLines.Select(d => d.Order).Where(d => d.OrderID == c.Order).FirstOrDefault(), c => c.OrderLines);
    }

    private void LoadVendorOrderRules()
    {
      var rules = unit.Scope.Repository<OrderRule>().GetAll().ToList();

      foreach (var line in orderLines)
      {
        try
        {
          if (line.DispatchedToVendorID.HasValue)
          {
            if (!VendorOrderLines.ContainsKey(line.DispatchedToVendorID.Value))
              VendorOrderLines.Add(line.DispatchedToVendorID.Value, new List<OrderLine>());

            VendorOrderLines[line.DispatchedToVendorID.Value].Add(line);
            continue;
          }

          if (line.Product == null || (line.Product.IsNonAssortmentItem.HasValue && line.Product.IsNonAssortmentItem.Value))
          {
            continue;
          }

          OrderLineVendor olv = new OrderLineVendor()
                                  {
                                    OrderLine = line,
                                    VendorValues = (from v in vendorIDs
                                                    let va = VendorUtility.GetMatchedVendorAssortment(unit.Scope.Repository<VendorAssortment>(), v, line.ProductID.Value)
                                                    where va != null && va.VendorPrices.Any(y => y.Price.HasValue)
                                                    select new VendorRuleValue
                                                    {
                                                      Value = 0,
                                                      VendorID = v,
                                                      VendorassortmentID = va.VendorAssortmentID
                                                    }).ToList()
                                  };

          if (olv.VendorValues.Count < 1)
          {
            if (line.Order.Connector.AdministrativeVendorID.HasValue)
            {
              int administrativeVendorID = line.Order.Connector.AdministrativeVendorID.Value;


              var administrativeVendors = (from v in unit.Scope.Repository<Vendor>().GetAllAsQueryable()
                                           where (v.VendorID == administrativeVendorID || (v.ParentVendorID.HasValue && v.ParentVendorID.Value == administrativeVendorID))
                                           && v.IsActive //&& v.OrderDispatcherType != null
                                           select v).ToList();

              administrativeVendors = administrativeVendors.Where(x => ((VendorType)x.VendorType).Has(VendorType.Assortment)).ToList();

              olv = new OrderLineVendor()
              {
                OrderLine = line,
                VendorValues = (from v in administrativeVendors
                                let va = VendorUtility.GetMatchedVendorAssortment(unit.Scope.Repository<VendorAssortment>(), v.VendorID, line.ProductID.Value, false)
                                where va != null
                                && va.VendorPrices.Any(y => y.Price.HasValue)
                                select new VendorRuleValue
                                {
                                  Value = 0,
                                  VendorID = v.VendorID,
                                  VendorassortmentID = va.VendorAssortmentID
                                }).ToList()
              };
            }

            if (olv.VendorValues.Count < 1)
            {
              line.Remarks = "No Vendor to process product, Line will be cancelled";

              try
              {
                CancelLine(line, unit);
                line.isDispatched = true;
              }
              catch (Exception)
              {
              }

              log.DebugFormat("No Vendor to process product line {0}, line cancelled", line.OrderLineID);
              continue;
            }
          }

          foreach (var rule in OrderRules)
          {
            var dbRule = rules.Where(x => x.Name == rule.Name).FirstOrDefault();

            if (dbRule != null)
            {
              rule.Apply(olv, unit, dbRule.Score);
            }
          }

          var vendorValuesList = olv.VendorValues.Where(c => c.Value == olv.VendorValues.Max(v => v.Value));
          var vendorID = vendorValuesList.OrderByDescending(x => x.Score).FirstOrDefault().VendorID;



          var connector = line.Order.Connector;

          var preferredVendorOverride = connector.PreferredConnectorVendors.FirstOrDefault(c => c.isPreferred);
          if (preferredVendorOverride != null)
          {
            vendorID = preferredVendorOverride.VendorID;
          }

          var preferredVendor = connector.PreferredConnectorVendors.Where(x => x.VendorID == vendorID).FirstOrDefault();

          if (preferredVendor != null && preferredVendor.CentralDelivery)
          {
            line.isDispatched = false;
            line.DispatchedToVendorID = vendorID;
            line.CentralDelivery = true;
            continue;
          }

          var parentVendor = vendors.FirstOrDefault(x => x.VendorID == vendorID && x.ParentVendorID.HasValue);

          //TODO: check impact
          //if (parentVendor != null)
          //  vendorID = parentVendor.ParentVendorID.Value;

          if (!VendorOrderLines.ContainsKey(vendorID))
            VendorOrderLines.Add(vendorID, new List<OrderLine>());

          VendorOrderLines[vendorID].Add(olv.OrderLine);
        }
        catch (Exception e)
        {
          log.Debug("Error process orderlineID " + line.OrderLineID, e);
        }
      }
    }

    private void CancelLine(OrderLine line, IUnitOfWork unit)
    {
      OrderResponse orderResponse = new OrderResponse()
      {
        OrderID = line.OrderID,
        Currency = "EUR",
        ReceiveDate = DateTime.Now.ToUniversalTime(),
        ReqDeliveryDate = DateTime.Now.ToUniversalTime(),
        ResponseType = OrderResponseTypes.Acknowledgement.ToString(),
        VendorID = 1,
        VendorDocumentNumber = "Concentrator",
        VendorDocumentDate = DateTime.Now.ToUniversalTime(),
        VendorDocument = "Concentrator Cancel",
        DocumentName = string.Empty
      };
      unit.Scope.Repository<OrderResponse>().Add(orderResponse);

      OrderResponseLine orderResponseLine = new OrderResponseLine()
      {
        Backordered = 0,
        Cancelled = line.Quantity,
        Ordered = line.Quantity,
        Shipped = 0,
        Delivered = 0,
        VendorItemNumber = line.Product.VendorItemNumber,
        VendorLineNumber = "0",
        Price = line.Price.HasValue ? decimal.Parse(line.Price.Value.ToString()) : 0,
        Processed = false,
        OrderLineID = line.OrderLineID,
        OrderResponse = orderResponse
      };

      unit.Scope.Repository<OrderResponseLine>().Add(orderResponseLine);

      unit.Save();
    }

    private void DispatchOrders(bool directShipment)
    {
      FailedOrders = 0;
      DispatchedOrders = 0;
      foreach (var vendorOrderLine in VendorOrderLines)
      {
        var vendor = this.unit.Scope.Repository<Vendor>().GetSingle(vs => vs.VendorID == vendorOrderLine.Key);
        try
        {

          var vendorDispatcherType = vendor.OrderDispatcherType;

          var typeDispatcher = AppDomain.CurrentDomain
              .GetAssemblies()
              .SelectMany(assembly => assembly.GetTypes())
              .Single(type => type.FullName == vendorDispatcherType && type.GetInterfaces().Contains(typeof(IDispatchable)));


          var dispatcher =
            (IDispatchable)
            Activator.CreateInstance(typeDispatcher);

          if (dispatcher != null)
          {
            var ord = GetGroupedOrders(vendorOrderLine.Value, OrderLineStatus.New);
            var OrderAdditionalOrderLines = new List<OrderLine>();

            foreach (var order in ord.Keys)
            {
              var additionalOrderLines = unit.Scope.Repository<OrderLine>().GetAllAsQueryable(x => x.OrderID == order.OrderID && (x.Product == null || (x.Product.IsNonAssortmentItem.HasValue && x.Product.IsNonAssortmentItem.Value)) && !x.isDispatched);

              int parentVendorID = vendor.ParentVendorID.HasValue ? vendor.ParentVendorID.Value : 0;

              if (order.Connector.Vendor != null && (((VendorType)order.Connector.Vendor.VendorType).Has(VendorType.HasFinancialProcess) || (order.Connector.Vendor != vendor && order.Connector.AdministrativeVendorID != parentVendorID))
                && !string.IsNullOrEmpty(order.Connector.Vendor.PurchaseOrderType))
              {
                foreach (var additionalOrderLine in additionalOrderLines)
                {
                  var additionalOrderProduct = (from aop in unit.Scope.Repository<AdditionalOrderProduct>().GetAllAsQueryable()
                                                where aop.ConnectorID == additionalOrderLine.Order.ConnectorID
                                                && aop.ConnectorProductID == additionalOrderLine.CustomerItemNumber
                                                && aop.VendorID == order.Connector.AdministrativeVendorID.Value
                                                select aop).FirstOrDefault();

                  if (additionalOrderProduct != null)
                  {
                    if (additionalOrderLine.ProductID.HasValue)
                      additionalOrderLine.SetStatus(OrderLineStatus.ReadyToOrder, unit.Scope.Repository<OrderLedger>());

                    OrderAdditionalOrderLines.Add(additionalOrderLine);
                    ord[order].Add(additionalOrderLine);
                  }
                  else
                  {
                    log.AuditInfo(string.Format("Skip orderline item {0}, cannot find settings for connectorID {1} and customItemNumber {2}", additionalOrderLine.OrderLineID, additionalOrderLine.Order.ConnectorID, additionalOrderLine.CustomerItemNumber));
                  }
                }

                var purchaseOrderVendor = (IPurchase)Activator.CreateInstance(Assembly.GetAssembly(typeof(IPurchase)).GetType(order.Connector.Vendor.PurchaseOrderType));

                bool waitForConfirmation = false;


                waitForConfirmation = purchaseOrderVendor.PurchaseOrders(order, ord[order], order.Connector.Vendor, vendor, directShipment, unit, log);

                foreach (var line in ord[order])
                {
                  if (waitForConfirmation)
                    line.SetStatus(OrderLineStatus.WaitingForPurchaseConfirmation, unit.Scope.Repository<OrderLedger>());

                  line.DispatchedToVendorID = vendorOrderLine.Key;

                  if (!line.ProductID.HasValue)
                  {
                    line.SetStatus(OrderLineStatus.Processed, unit.Scope.Repository<OrderLedger>());
                    line.isDispatched = true;
                    line.DispatchedToVendorID = order.Connector.AdministrativeVendorID;
                  }
                }
              }
              else
              {
                foreach (var additionalOrderLine in additionalOrderLines)
                {
                  int addVendorID = 0;
                  if (vendor.ParentVendorID.HasValue)
                    addVendorID = vendor.ParentVendorID.Value;

                  var additionalOrderProduct = (from aop in unit.Scope.Repository<AdditionalOrderProduct>().GetAllAsQueryable()
                                                where aop.ConnectorID == additionalOrderLine.Order.ConnectorID
                                                && aop.ConnectorProductID == additionalOrderLine.CustomerItemNumber
                                                && (aop.VendorID == vendor.VendorID || aop.VendorID == addVendorID)
                                                select aop).FirstOrDefault();

                  if (additionalOrderProduct != null)
                  {
                    additionalOrderLine.SetStatus(OrderLineStatus.ReadyToOrder, unit.Scope.Repository<OrderLedger>());
                    OrderAdditionalOrderLines.Add(additionalOrderLine);
                  }
                  else
                  {
                    log.AuditInfo(string.Format("Skip orderline item {0}, cannot find settings for connectorID {1} and customItemNumber {2}", additionalOrderLine.OrderLineID, additionalOrderLine.Order.ConnectorID, additionalOrderLine.CustomerItemNumber));
                  }
                }
                foreach (var line in ord[order])
                  line.SetStatus(OrderLineStatus.ReadyToOrder, unit.Scope.Repository<OrderLedger>());
              }
            }

            vendorOrderLine.Value.AddRange(OrderAdditionalOrderLines);
            ord = GetGroupedOrders(vendorOrderLine.Value, OrderLineStatus.ReadyToOrder);

            if (ord.Count > 0)
            {
              int vendorOrderID = dispatcher.DispatchOrders(ord, vendor, log, unit);
              DispatchedOrders += (from v in ord select v.Value.Count).ToList().Sum();
              log.AuditInfo(string.Format("Dispatched {0} orders to {1}", ord.Count, vendor.Name));
            }

            foreach (var lines in ord.Values)
            {
              lines.ForEach(c =>
                                              {
                                                c.SetStatus(OrderLineStatus.WaitingForAcknowledgement, unit.Scope.Repository<OrderLedger>());
                                                c.isDispatched = true;
                                                c.DispatchedToVendorID = vendorOrderLine.Key;
                                              });
            }
          }
        }
        catch (Exception e)
        {
          FailedOrders++;
          Exceptions.Add(new Exception("Failed: Order #" + vendorOrderLine.Key + " , OrderLines: " + string.Join(", ", vendorOrderLine.Value.Select(c => c.OrderLineID.ToString()).ToArray()) + " to " + vendor.Name, e));
        }
        finally
        {
          unit.Save();
        }
      }
    }
  }
}
