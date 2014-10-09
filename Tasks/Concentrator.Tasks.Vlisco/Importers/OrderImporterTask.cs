using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Tasks.Vlisco.Importers
{
  [Task(Constants.Vendor.Vlisco + " Order Importer Task")]
  public class OrderImporterTask : MultiMagImporterTask<Models.Order, Models.OrderMapping>
  {
    protected override String ImportFilePrefix
    {
      get
      {
        return Constants.Prefixes.Transaction;
      }
    }

    private Int32? GetTransactionProductID(Models.Order transaction)
    {
      return Database.FirstOrDefault<Int32?>("SELECT [ProductID] FROM [dbo].[Product] WHERE [VendorItemNumber] = @0"
        , Constants.GetVendorItemNumber(transaction.ArticleCode, transaction.ColorCode, transaction.SizeCode));
    }

    protected override void Import(IDictionary<Models.Order, String> transactions)
    {
      var customerRepository = Unit.Scope
        .Repository<Objects.Models.Orders.Customer>();
      var orderRepository = Unit.Scope
        .Repository<Objects.Models.Orders.Order>()
        .Include(order => order.OrderLines)
        .Include(order => order.OrderResponses);

      foreach (var transactionGrouping in transactions.Keys.GroupBy(transaction => transaction.Ticket))
      {
        var firstTransaction = transactionGrouping.First();
        var ticket = transactionGrouping.Key;
        var transactionProductIDs = transactionGrouping.ToDictionary(transaction => transaction, GetTransactionProductID);

        if (transactionProductIDs.Values.All(productID => productID.HasValue))
        {
          var orderType = default(Int32);

          switch (firstTransaction.SaleType.ToUpper())
          {
            case Constants.Transaction.Return:
              orderType = (Int32)Objects.Models.Orders.OrderTypes.ReturnOrder;
              break;

            case Constants.Transaction.Sale:
              orderType = (Int32)Objects.Models.Orders.OrderTypes.SalesOrder;
              break;
          }

          var order = orderRepository.GetSingle(o => o.WebSiteOrderNumber == ticket && o.OrderType == orderType);

          if (order == null)
          {
            var customer = customerRepository.GetSingle(c => c.ServicePointID == firstTransaction.Client && c.ServicePointCode == firstTransaction.ShopCode);

            order = new Objects.Models.Orders.Order
            {
              BSKIdentifier = firstTransaction.SalesPerson,
              ConnectorID = Context.ConnectorID,
              IsDispatched = true,
              Document = String.Join(Environment.NewLine, transactionGrouping.Select(transaction => transactions[transaction])),
              OrderLines = transactionGrouping
                .OrderBy(transaction => transaction.Line)
                .Select(transaction => new Objects.Models.Orders.OrderLine
                {
                  BasePrice = (Double)transaction.PurchasePrice,
                  CustomerOrderNr = transactionGrouping.Key,
                  CustomerItemNumber = String.Join(Environment.NewLine, transaction.ArticleCode, transaction.ColorCode, transaction.SizeCode),
                  DispatchedToVendorID = Context.AdministrativeVendorID,
                  ProductID = transactionProductIDs[transaction],
                  isDispatched = true,
                  LineDiscount = (Double)transaction.DiscountValue,
                  PriceOverride = true,
                  Price = (Double)transaction.NettoPrice,
                  OriginalLine = transactions[transaction],
                  Quantity = transaction.Quantity,
                  UnitPrice = (Double)transaction.SalePrice,
                  TaxRate = transaction.VAT,
                  WareHouseCode = transaction.ShopCode
                })
                .ToList(),
              OrderType = orderType,
              ReceivedDate = transactionGrouping.First().OrderDate.Date + transactionGrouping.First().OrderTime.TimeOfDay,
              ShippedToCustomer = customer,
              SoldToCustomer = customer,
              WebSiteOrderNumber = String.Join("/", firstTransaction.ShopCode, ticket)
            };

            if (order.OrderLines.All(orderLine => orderLine.ProductID.HasValue))
            {
              orderRepository.Add(order);
            }
          }
        }
        else
        {
          foreach (var transaction in transactionProductIDs.Keys)
          {
            if (!transactionProductIDs[transaction].HasValue)
            {
              TraceWarning("{0}: Unable to find a product for transaction '{1}', line {2}.", Context.Name, ticket, transaction.Line);
            }
          }
        }
      }

      Unit.Save();
    }
  }
}
