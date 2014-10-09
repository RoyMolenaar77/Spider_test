using System;

namespace Concentrator.Tasks.Vlisco.Models
{
  public class Item : IEquatable<Item>
  {
    public DateTime DateTime
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

    public String ColorName
    {
      get;
      set;
    }

    public String SizeCode
    {
      get;
      set;
    }

    public String Barcode
    {
      get;
      set;
    }

    public String DescriptionLong
    {
      get;
      set;
    }

    public String DescriptionShort
    {
      get;
      set;
    }

    public Decimal Price
    {
      get;
      set;
    }

    public String OriginCode
    {
      get;
      set;
    }

    public String OriginName
    {
      get;
      set;
    }

    public String LabelCode
    {
      get;
      set;
    }

    public String LabelName
    {
      get;
      set;
    }

    public String SegmentCode
    {
      get;
      set;
    }

    public String SegmentName
    {
      get;
      set;
    }

    public String GroupCode
    {
      get;
      set;
    }

    public String GroupName
    {
      get;
      set;
    }

    public Boolean Equals(Item other)
    {
      return other != null && ArticleCode == other.ArticleCode && ColorCode == other.ColorCode && SizeCode == other.SizeCode;
    }

    public override Int32 GetHashCode()
    {
      return ArticleCode.GetHashCode() ^ ColorCode.GetHashCode() ^ SizeCode.GetHashCode();
    }
  }
}
