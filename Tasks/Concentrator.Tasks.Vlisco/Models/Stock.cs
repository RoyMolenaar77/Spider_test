using System;

namespace Concentrator.Tasks.Vlisco.Models
{
  public class Stock : IEquatable<Stock>
  {
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

    public Int32 Available
    {
      get;
      set;
    }

    public Int32 Delivered
    {
      get;
      set;
    }

    public Int32 InStock
    {
      get;
      set;
    }

    public Int32 Maximum
    {
      get;
      set;
    }

    public Int32 Minimum
    {
      get;
      set;
    }

    public Int32 Ordered
    {
      get;
      set;
    }

    public Int32 Reserved
    {
      get;
      set;
    }

    public Int32 TotalIn
    {
      get;
      set;
    }

    public Int32 TotalOut
    {
      get;
      set;
    }

    public Decimal? WAC
    {
      get;
      set;
    }

    public Boolean Equals(Stock other)
    {
      return other != null 
        && ShopCode == other.ShopCode 
        && ArticleCode == other.ArticleCode 
        && ColorCode == other.ColorCode 
        && SizeCode == other.SizeCode;
    }

    public override Int32 GetHashCode()
    {
      return ShopCode.GetHashCode() 
        ^ ArticleCode.GetHashCode() 
        ^ ColorCode.GetHashCode() 
        ^ SizeCode.GetHashCode();
    }
  }
}
