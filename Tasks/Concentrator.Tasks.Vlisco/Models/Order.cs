using System;
using System.Text;

namespace Concentrator.Tasks.Vlisco.Models
{
  public class Order : IEquatable<Order>
  {
    public DateTime OrderDate
    {
      get;
      set;
    }

    public DateTime OrderTime
    {
      get;
      set;
    }

    public String SaleType
    {
      get;
      set;
    }

    public String Ticket
    {
      get;
      set;
    }

    public Int32 Line
    {
      get;
      set;
    }

    public String ArticleCode
    {
      get;
      set;
    }

    public String ColorCode
    {
      get;
      set;
    }

    public String SizeCode
    {
      get;
      set;
    }

    public Int32 Quantity
    {
      get;
      set;
    }

    public Decimal PurchasePrice
    {
      get;
      set;
    }

    public Decimal SalePrice
    {
      get;
      set;
    }

    public Decimal BrutoPrice
    {
      get;
      set;
    }

    public Decimal VAT
    {
      get;
      set;
    }

    public Decimal NettoPrice
    {
      get;
      set;
    }

    public Decimal DiscountPercentage
    {
      get;
      set;
    }

    public Decimal DiscountValue
    {
      get;
      set;
    }

    public String ShopCode
    {
      get;
      set;
    }

    public String SalesPerson
    {
      get;
      set;
    }

    public String Client
    {
      get;
      set;
    }

    public Boolean Equals(Order other)
    {
      return other != null && Line == other.Line && Ticket.Equals(other.Ticket, StringComparison.InvariantCultureIgnoreCase);
    }

    public override Int32 GetHashCode()
    {
      return Ticket != null
        ? Ticket.GetHashCode() ^ Line
        : 0;
    }

    public String GetProductNumber()
    {
      var vendorItemNumber = ArticleCode;

      if (!Constants.IgnoreCode.Equals(ColorCode))
      {
        vendorItemNumber = String.Join(Constants.VendorItemNumberSegmentSeparator, vendorItemNumber, ColorCode);
      }

      if (!Constants.IgnoreCode.Equals(SizeCode))
      {
        vendorItemNumber = String.Join(Constants.VendorItemNumberSegmentSeparator, vendorItemNumber, SizeCode);
      }

      return vendorItemNumber;
    }
  }
}
