using System;

namespace Concentrator.Tasks.Vlisco.Models
{
  public class Movement : IEquatable<Movement>
  {
    public String ShopCode
    {
      get;
      set;
    }

    public Int32 MovementNumber
    {
      get;
      set;
    }

    public String MovementType
    {
      get;
      set;
    }

    public String MovementDescription
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

    public String MovementDirection
    {
      get;
      set;
    }

    public DateTime MovementDate
    {
      get;
      set;
    }

    public DateTime MovementTime
    {
      get;
      set;
    }

    public String SalesPerson
    {
      get;
      set;
    }

    public Decimal? CostPrice
    {
      get;
      set;
    }

    public Decimal? UnitPrice
    {
      get;
      set;
    }

    public String DocumentNumber
    {
      get;
      set;
    }

    public String DocumentLine
    {
      get;
      set;
    }

    public String LocationFrom
    {
      get;
      set;
    }

    public String LocationTo
    {
      get;
      set;
    }

    public Int32 Quantity
    {
      get;
      set;
    }

    public String TransactionNumber
    {
      get;
      set;
    }

    public String LotNumber
    {
      get;
      set;
    }

    public Boolean Equals(Movement other)
    {
      return other != null && ShopCode == other.ShopCode && MovementNumber == other.MovementNumber;
    }

    public override Int32 GetHashCode()
    {
      return ShopCode.GetHashCode() ^ MovementNumber;
    }
  }
}
