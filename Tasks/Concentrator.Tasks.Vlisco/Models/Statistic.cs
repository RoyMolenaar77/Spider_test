using System;
using System.Collections.Generic;
using System.Linq;

namespace Concentrator.Tasks.Vlisco.Models
{
  public class Statistic : IEquatable<Statistic>
  {
    public String Atmosphere
    {
      get;
      set;
    }

    public String Context
    {
      get;
      set;
    }

    public DateTime DateTime
    {
      get;
      set;
    }

    public String ShopCode
    {
      get;
      set;
    }

    public Decimal ClientSales
    {
      get;
      set;
    }

    public Decimal GeneralSales
    {
      get;
      set;
    }

    public Decimal SalesPerClient
    {
      get;
      set;
    }

    public Decimal TotalAmount
    {
      get;
      set;
    }

    public Decimal TotalSales
    {
      get;
      set;
    }

    public Decimal UnitsPerClient
    {
      get;
      set;
    }

    public Decimal UnitsSold
    {
      get;
      set;
    }

    public Int32? VisitorCount
    {
      get;
      set;
    }

    public Int32? VisitorsWithNoSales
    {
      get;
      set;
    }

    public Boolean Equals(Statistic other)
    {
      return other != null && ShopCode == other.ShopCode;
    }

    public override Int32 GetHashCode()
    {
      return ShopCode.GetHashCode();
    }
  }
}
