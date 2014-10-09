using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Environments;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Plugins.PFA.Objects.Helper;
using Concentrator.Plugins.PFA.Objects.Model;
using Concentrator.Plugins.PFA.Transfer.Model.ReceivedTransfer;
using Concentrator.Plugins.PfaCommunicator.Objects.Models;
using Concentrator.Plugins.PfaCommunicator.Objects.Services;
using FileHelpers;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Concentrator.Plugins.PFA.Transfer
{
  public class ReceivedTransferProcessor : ConcentratorPlugin
  {
    public ReceivedTransferProcessor()
    {

    }

    public override string Name
    {
      get { return "Received transfer orders"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        var vendors = unit.Scope.Repository<Vendor>().GetAll().ToList().Where(c => ((VendorType)c.VendorType).Has(VendorType.SupportsPFATransferOrders)).ToList();
        int transferOrderType = (int)OrderTypes.TransferOrder;
        var ledgerRepo = unit.Scope.Repository<OrderLedger>();


        vendors.ForEach((vendor, indexer) =>
        {
          var mutationEngine = new FileHelperEngine(typeof(ReceivedTransferMutation));

          var fileName = String.Format("{0}{1}", "mancosurplus", DateTime.Now.ToString("yyyyMMddHHmmss"));

          var mutations = new List<ReceivedTransferMutation>();

          var rule = vendor.ContentProducts.FirstOrDefault(c => c.IsAssortment);

          rule.ThrowIfNull("Publication rule with IsAssortment is missing for vendor " + vendor.Name);

          List<OrderLine> lines = unit
            .Scope
            .Repository<OrderLine>()
            .GetAll(x => x.Order.OrderType == transferOrderType 
              && x.Order.ConnectorID == rule.ConnectorID 
              && x.OrderLedgers.Any(y => y.Status == (int)OrderLineStatus.ReceivedTransfer) 
              && !x.OrderLedgers.Any(y => y.Status == (int)OrderLineStatus.ProcessedReceivedTransfer)).ToList();

          var storeID = ConnectorHelper.GetStoreNumber(rule.ConnectorID);
          var employeeNumber = VendorHelper.GetEmployeeNumber(vendor.VendorID);

          var salesSlipNumber = ReceiptHelper.GetSlipNumber(vendor.VendorID);
          //ReceiptHelper.IncrementSalesSlipNumber(ref salesSlipNumber);

          var toProcess = lines.GroupBy(k => k.Order).ToList().Select(c => new
          {
            Order = c.Key,
            OrderLines = c.ToList(),
            ExtraOrderLines = c.Key.OrderLines.Except(c.ToList())
          }).ToList();

          foreach (var order in toProcess)
          {
            PfaOrderHelper helper = new PfaOrderHelper(vendor.VendorID);

            if (order.OrderLines.Count == 0) continue;

            order.OrderLines.AddRange(order.ExtraOrderLines);

            var receivedByWehkampDate = order.OrderLines.FirstOrDefault().OrderLedgers.FirstOrDefault(c => c.Status == (int)OrderLineStatus.ReceivedTransfer).LedgerDate.ToLocalTime();

            List<TransferOrderModel> list = helper.GetShippedQuantitiesForOrder(order.Order.WebSiteOrderNumber, order.Order.ConnectorID);
            if (list.Count > 0)
            {
              List<TransferOrderLine> orderLinesToBeProcessed = GetOrderLineStatus(order.OrderLines, list, order.Order.OrderID, vendor.VendorID, unit);

              if (list.Count > order.OrderLines.Count())
              {
                //more lines are in PFA than in the Concentrator
                log.AuditInfo(string.Format("For order {0} there are {1} order lines in Concentrator and {2} in PFA. Difference found of {3} lines."
                                                , order.Order.WebSiteOrderNumber, order.OrderLines.Count(), list.Count, order.ExtraOrderLines.Count()));
              }

              if (orderLinesToBeProcessed.Any(c => c.ShippedFromPFA != c.ReceivedFromWehkamp))
              {
                var differences = orderLinesToBeProcessed.Where(c => c.ShippedFromPFA != c.ReceivedFromWehkamp);

                //add each quantity unmatched product
                differences.ForEach((difference, index) =>
                  {
                    var unmatchedQuantityLine = GetDifferenceRuleForIncompleteShipping(difference, orderLinesToBeProcessed.Sum(x => x.ReceivedFromWehkamp), orderLinesToBeProcessed.Sum(x => x.ShippedFromPFA), storeID, employeeNumber, vendor.VendorID, receivedByWehkampDate, salesSlipNumber);

                    mutations.Add(unmatchedQuantityLine);
                  });

                //add the total rule
                mutations.AddRange(GetTotalRuleForIncompleteShipping(order.Order, orderLinesToBeProcessed.Sum(x => x.ReceivedFromWehkamp), orderLinesToBeProcessed.Sum(x => x.ShippedFromPFA), storeID, employeeNumber, vendor.VendorID, receivedByWehkampDate, salesSlipNumber));
              }
              else
              {
                //add quantity matched list
                mutations.AddRange(GetCompletelyShippedRule(order.Order, orderLinesToBeProcessed.Sum(x => x.ReceivedFromWehkamp), storeID, employeeNumber, vendor.VendorID, receivedByWehkampDate, salesSlipNumber));
              }
            }
            else
            {
              log.AuditError("Ignoring order " + order.Order.WebSiteOrderNumber + ": No lines found in PFA");
            }
            foreach (var line in order.OrderLines) line.SetStatus(OrderLineStatus.ProcessedReceivedTransfer, ledgerRepo);
            ReceiptHelper.IncrementSalesSlipNumber(ref salesSlipNumber, vendor.VendorID);


          }

          if (mutations.Count > 0)
          {
            var file = mutationEngine.WriteString(mutations);

            var path = CommunicatorService.GetMessagePath(vendor.VendorID, MessageTypes.TransferOrderConfirmation);

            File.WriteAllText(Path.Combine(path.MessagePath, fileName), file);
          }
          unit.Save();
        });
      }
    }

    private List<ReceivedTransferMutation> GetCompletelyShippedRule(Order order, int ReceivedFromWehkamp, int storeID, string employeeNumber, int vendorID, DateTime receivedByWehkamp, int salesSlipNumber)
    {
      List<ReceivedTransferMutation> _list = new List<ReceivedTransferMutation>();

      var datcolLine = new ReceivedTransferMutation
      {
        StoreNumber = String.Format("{0} {1}", storeID, "01"),
        EmployeeNumber = employeeNumber,
        ReceiptNumber = salesSlipNumber,
        TransactionType = "21",
        DateNotified = receivedByWehkamp.ToString("yyyyMMddhhmm"),
        RecordType = "99",
        SubType = "02",
        NumberOfDifferences = "0+",
        FixedField1 = "000000001+",
        FixedField2 = 0,
        NumberOfSkus = String.Format("{0}+", ReceivedFromWehkamp),
        FixedField3 = "000",
        FixedField4 = "000000000+",
        FixedField5 = "000",
        FixedField6 = 0,
        FixedField7 = "000",
        FixedField8 = 0,
        FixedField9 = "00",
        FixedField10 = "00000000000000000000",
        FixedField11 = "00000000000000000000",
        TransferNumber = order.WebSiteOrderNumber,
        EmployeeNumber2 = employeeNumber,
        ScannedIndication = 0
      };

      _list.Add(datcolLine);

      return _list;
    }

    private ReceivedTransferMutation GetDifferenceRuleForIncompleteShipping(TransferOrderLine difference, int ReceivedFromWehkamp, int ShippedFromPFA, int storeID, string employeeNumber, int vendorID, DateTime receivedByWehkamp, int salesSlipNumber)
    {
      OrderLedger ledger = difference.OrderLine.OrderLedgers.FirstOrDefault(x => x.Status == (int)OrderLineStatus.ReceivedTransfer);

      var product = difference.OrderLine.Product;
      var priceAssortment = product.VendorAssortments.FirstOrDefault(c => c.VendorID == vendorID).VendorPrices.FirstOrDefault();
      decimal markDown = 0;
      if (priceAssortment.SpecialPrice.HasValue && priceAssortment.Price > priceAssortment.SpecialPrice.Value)
      {
        markDown = priceAssortment.Price.Value - priceAssortment.SpecialPrice.Value;
      }

      var productNumberParts = product.VendorItemNumber.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
      var artikelCode = productNumberParts[0];
      var colorCode = productNumberParts[1].PadLeft(3, '0');

      ReceivedTransferMutation datcolLine = new ReceivedTransferMutation
      {
        StoreNumber = String.Format("{0} {1}", storeID, "01"),
        EmployeeNumber = employeeNumber,
        ReceiptNumber = salesSlipNumber,
        TransactionType = "21",
        DateNotified = receivedByWehkamp.ToString("yyyyMMddhhmm"),
        RecordType = "01",
        SubType = "02",
        NumberOfDifferences = QuantityHelper.GetNumberOfDifferences(difference.ReceivedFromWehkamp, difference.ShippedFromPFA),
        FixedField1 = "000000001+",
        FixedField2 = (int)Math.Round(markDown * 100),
        NumberOfSkus = String.Format("{0}+", difference.ShippedFromPFA),
        FixedField3 = "000",
        FixedField4 = "000000000+",
        FixedField5 = "000",
        FixedField6 = 0,
        FixedField7 = "000",
        FixedField8 = (int)Math.Round(priceAssortment.Price.Value * 100),
        FixedField9 = "00",
        FixedField10 = "000" + ProductHelper.GetPFAItemNumber(artikelCode, colorCode, product.ProductID).ToUpper(),
        FixedField11 = "0000000" + BarcodeHelper.GetBarcode(product.ProductID),
        TransferNumber = difference.OrderLine.Order.WebSiteOrderNumber,
        EmployeeNumber2 = employeeNumber,
        ScannedIndication = 0
      };

      return datcolLine;
    }

    private List<ReceivedTransferMutation> GetTotalRuleForIncompleteShipping(Order order, int ReceivedFromWehkamp, int ShippedFromPFA, int storeID, string employeeNumber, int vendorID, DateTime receivedByWehkamp, int salesSlipNumber)
    {
      var _list = new List<ReceivedTransferMutation>();

      var mutation = new ReceivedTransferMutation
      {
        StoreNumber = String.Format("{0} {1}", storeID, "01"),
        EmployeeNumber = employeeNumber,
        ReceiptNumber = salesSlipNumber,
        TransactionType = "21",
        DateNotified = receivedByWehkamp.ToString("yyyyMMddhhMM"),
        RecordType = "99",
        SubType = "02",
        NumberOfDifferences = QuantityHelper.GetNumberOfDifferences(ReceivedFromWehkamp, ShippedFromPFA),
        FixedField1 = "1+",
        FixedField2 = 0,
        NumberOfSkus = String.Format("{0}+", ReceivedFromWehkamp),
        FixedField3 = "000",
        FixedField4 = "0+",
        FixedField5 = "000",
        FixedField6 = 0,
        FixedField7 = "000",
        FixedField8 = 0,
        FixedField9 = "00",
        FixedField10 = "00000000000000000000",
        FixedField11 = "00000000000000000000",
        TransferNumber = order.WebSiteOrderNumber,
        EmployeeNumber2 = employeeNumber,
        ScannedIndication = 0
      };

      _list.Add(mutation);


      return _list;
    }

    private List<TransferOrderLine> GetOrderLineStatus(List<OrderLine> order, List<Objects.Model.TransferOrderModel> list, int orderID, int vendorID, IUnitOfWork unit)
    {
      List<TransferOrderLine> orderLines = new List<TransferOrderLine>();

      order.ForEach((line, idx) =>
      {
        var match = list.FirstOrDefault(x => string.Format("{0} {1} {2}", x.ArtikelNumber, x.ColorCode, x.SizeCode).ToLower() == line.Product.VendorItemNumber.ToLower());

        var shippedFromPFA = match == null ? 0 : match.Shipped; //If match is not found in PFA, assume shipped = 0

        var receivedTransferLedger = line.OrderLedgers.FirstOrDefault(c => c.Status == (int)OrderLineStatus.ReceivedTransfer);

        orderLines.Add(new TransferOrderLine
        {
          OrderLine = line,
          ShippedFromPFA = shippedFromPFA,
          ReceivedFromWehkamp = receivedTransferLedger != null ? receivedTransferLedger.Quantity.Value : 0
        });
      });

      foreach (var lineInPFA in list) //change to support lines in PFA and Wehkamp but not in Concentrator
      {
        var match = order.FirstOrDefault(x => string.Format("{0} {1} {2}", lineInPFA.ArtikelNumber, lineInPFA.ColorCode, lineInPFA.SizeCode).ToLower() == x.Product.VendorItemNumber.ToLower());
        if (match == null)
        {
          //create an order line in Concentrator 
          //create orderledgers in Concentrator
          //append to order list
          using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
          {
            var productID = db.FirstOrDefault<int>(@"select productid from product where vendoritemnumber = @0", string.Format("{0} {1} {2}", lineInPFA.ArtikelNumber, lineInPFA.ColorCode, lineInPFA.SizeCode));
            var orderLineID = CreateOrderLineRow(orderID, productID, vendorID, db, lineInPFA.Shipped);

            db.Execute(string.Format("INSERT INTO OrderLedger ([OrderLineID],[Status],[LedgerDate],[Quantity]) (SELECT TOP 1 OrderLineID, '{2}', '{3}', '{4}' FROM OrderLine WHERE OrderID = {0} AND ProductID = {1})", orderID, productID, (int)OrderLineStatus.ReceivedTransfer, DateTime.Now.ToUniversalTime(), 0));

            //add it
            var lineExtra = unit.Scope.Repository<OrderLine>().GetSingle(c => c.OrderLineID == orderLineID);
            order.Add(lineExtra);
            orderLines.Add(new TransferOrderLine
            {
              OrderLine = lineExtra,
              ReceivedFromWehkamp = 0,
              ShippedFromPFA = lineInPFA.Shipped
            });
          }

        }
      }

      return orderLines;
    }

    private int CreateOrderLineRow(int orderID, int productID, int vendorID, Database db, int qty)
    {
      const string insert = "INSERT INTO [dbo].[OrderLine] ([OrderID],[ProductID],[Quantity],[isDispatched],[DispatchedToVendorID]) OUTPUT Inserted.OrderLineID VALUES ";
      var sql = string.Format("{0} ({1}, {2}, {3}, {4}, {5})", insert, orderID, productID, qty, 1, vendorID);
      var orderLineID = -1;

      db.CommandTimeout = 30;
      orderLineID = db.ExecuteScalar<int>(sql);

      return orderLineID;
    }
  }

  public class TransferOrderLine
  {
    public OrderLine OrderLine { get; set; }

    public int ShippedFromPFA { get; set; }

    public int ReceivedFromWehkamp { get; set; }
  }


}
