using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Orders;

namespace Concentrator.Plugins.PFA.Helpers
{
  public static class DatcolHelper
  {
    /// <summary>
    /// Returns the discount of a datcol line
    /// Returns int -> decimals are multiplied by 100. So 5,95 => 595
    /// </summary>
    /// <param name="line">The orderline to process</param>
    /// <param name="qtyDispatched">The actual dispatched qty</param>
    /// <returns></returns>
    public static int GetDiscount(OrderLine line, int qtyDispatched)
    {
      if (qtyDispatched == 0) qtyDispatched = line.Quantity;

      if (!line.LineDiscount.HasValue || line.LineDiscount.Value == 0) return 0;

      var lineDiscount = line.LineDiscount;
      if (qtyDispatched < line.Quantity)
      {
        lineDiscount = (line.LineDiscount / line.Quantity) * qtyDispatched;
      }

      return (int)Math.Round(lineDiscount.Value * 100);
    }

    public static int GetNegativeRevenue(OrderLine line, int returnedQty)
    {
      double revenue = 0;

      var individualDiscount = line.LineDiscount.Try(c => c.Value, 0) / line.Quantity; //individual discount

      revenue = (line.UnitPrice.Value - individualDiscount) * returnedQty;

      return -(int)Math.Round(revenue * 100);
    }


    public static double GetUnformatedNegativeRevenue(OrderLine line, int returnedQty)
    {
      double revenue = 0;

      var individualDiscount = line.LineDiscount.Try(c => c.Value, 0) / line.Quantity; //individual discount

      revenue = (line.UnitPrice.Value - individualDiscount) * returnedQty;

      return -revenue;
    }

    public static int FormatNegativeRevenueForDatcol(double revenue)
    {
      return (int)Math.Round(revenue * 100);
    }


    /// <summary>
    /// Returns the revenue of an orderline
    /// Support for AT datcols
    /// </summary>
    /// <param name="line"></param>
    /// <param name="qtyDispatched"></param>
    /// <returns></returns>
    public static int GetRevenue(OrderLine line, int qtyProcessed, int qtyDispatched)
    {
      if (qtyDispatched == 0) qtyDispatched = line.Quantity;

      var revenue = line.Price.Value - line.LineDiscount.Try(c => c.Value, 0); //total amount

      //anything processed in the kasmut?
      if (qtyProcessed > 0)
      {
        var discountPerItem = 0.00;
        if (line.LineDiscount.HasValue && line.LineDiscount.Value > 0)
        {
          discountPerItem = line.LineDiscount.Value / line.Quantity;
        }
        revenue = revenue - (qtyProcessed * (line.UnitPrice.Try(c => c.Value, 0) - discountPerItem));
      }
      return (int)Math.Round(Math.Max(revenue * 100, 0)); //support for 0 shipment costs
    }

    /// <summary>
    /// Returns the revenue of an orderline
    /// </summary>
    /// <param name="line"></param>
    /// <param name="qtyDispatched"></param>
    /// <returns></returns>
    public static double GetUnformattedRevenue(OrderLine line, int qtyProcessed, int qtyDispatched)
    {
      if (qtyDispatched == 0) qtyDispatched = line.Quantity;

      var revenue = line.Price.Value - line.LineDiscount.Try(c => c.Value, 0); //total amount

      //anything processed in the kasmut?
      if (qtyProcessed > 0)
      {
        var discountPerItem = 0.00;
        if (line.LineDiscount.HasValue && line.LineDiscount.Value > 0)
        {
          discountPerItem = line.LineDiscount.Value / line.Quantity;
        }
        revenue = revenue - (qtyProcessed * (line.UnitPrice.Try(c => c.Value, 0) - discountPerItem));
      }
      return Math.Max(revenue, 0);
    }

    public static int FormatRevenueForDatcol(double revenue)
    {
      return (int)Math.Round(revenue * 100);
    }

    public static void IncrementSalesSlipNumber(ref int currentSalesSlip)
    {
      if (currentSalesSlip == 9999)
        currentSalesSlip = 0;

      currentSalesSlip++;
    }

    /// <summary>
    /// Returns the BTW code to be used in the DATCOL
    /// </summary>
    /// <param name="taxRate"></param>
    /// <returns>1 for high tax, 2 for low. By default 1</returns>
    public static int GetBTWCode(string itemNumer)
    {
      int result = 1; //by default 1

      if (itemNumer.StartsWith("7"))
        result = 2; //low tax items

      return result;
    }

    public static bool IsSetSale(OrderLine line)
    {
      bool isSet = false;

      if (line.OrderLineAppliedDiscountRules != null && line.OrderLineAppliedDiscountRules.Count > 0)
      {
        isSet = line.OrderLineAppliedDiscountRules.Any(c => c.IsSet);
      }

      return isSet;
    }

    public static void SaveDatcolLink(Order o, string shopNumber, DateTime entryTime, decimal amount, int datcolNumber, string sourceMessage)
    {
      o.DatcolLinks.Add(new DatcolLink()
      {
        Order = o,
        ShopNumber = shopNumber,
        Amount = amount,
        DateCreated = entryTime,
        DatcolNumber = datcolNumber.ToString(),
        SourceMessage = sourceMessage
      });
    }
  }
}
