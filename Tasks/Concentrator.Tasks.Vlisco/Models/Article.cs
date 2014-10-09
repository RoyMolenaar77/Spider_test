using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using Concentrator.Objects.Models.Contents;
using Concentrator.Tasks.Vlisco.Extensions;

namespace Concentrator.Tasks.Vlisco.Models
{
  public class Article
  {
    public Int32 BrandID
    {
      get;
      set;
    }

    public Int32 ArticleID
    {
      get;
      set;
    }

    public Int32 ProductID
    {
      get;
      set;
    }

    public Int32 VendorID
    {
      get;
      set;
    }

    public string ArticleCode
    {
      get;
      set;
    }

    public string ColorCode
    {
      get;
      set;
    }

    public string SizeCode
    {
      get;
      set;
    }

    public string DescriptionLong
    {
      get;
      set;
    }

    public string DescriptionShort
    {
      get;
      set;
    }

    public string ColorName
    {
      get;
      set;
    }

    public decimal CostPrice
    {
      get;
      set;
    }

    public decimal Price
    {
      get;
      set;
    }

    public Int32 Stock
    {
      get;
      set;
    }

    public Int32 CategoryID
    {
      get;
      set;
    }

    public string CategoryCode
    {
      get;
      set;
    }

    public string CategoryName
    {
      get;
      set;
    }

    public Int32 FamilyID
    {
      get;
      set;
    }

    public string FamilyCode
    {
      get;
      set;
    }

    public string FamilyName
    {
      get;
      set;
    }

    public Int32 SubfamilyID
    {
      get;
      set;
    }

    public string SubfamilyCode
    {
      get;
      set;
    }

    public string SubfamilyName
    {
      get;
      set;
    }

    public string Barcode
    {
      get;
      set;
    }

    public int ReplenishmentMinimum
    {
      get;
      set;
    }

    public int ReplenishmentMaximum
    {
      get;
      set;
    }

    public decimal Tax
    {
      get;
      set;
    }

    public string Collection
    {
      get;
      set;
    }

    public string ReferenceCode
    {
      get;
      set;
    }

    public string SupplierName
    {
      get;
      set;
    }

    public string SupplierCode
    {
      get;
      set;
    }

    public string LabelCode
    {
      get;
      set;
    }

    public string MaterialCode
    {
      get;
      set;
    }

    public string OriginCode
    {
      get;
      set;
    }

    public string ShapeCode
    {
      get;
      set;
    }

    public string CountryCode
    {
      get;
      set;
    }

    public string CurrencyCode
    {
      get;
      set;
    }

    public string StockType
    {
      get;
      set;
    }

    public string ZoneCode4
    {
      get;
      set;
    }

    public string ZoneCode5
    {
      get;
      set;
    }

    public string ProFrom
    {
      get;
      set;
    }

    public string ProTo
    {
      get;
      set;
    }

    public override int GetHashCode()
    {
      var hashCode = 0;

      if (ArticleCode != null)
      {
        hashCode ^= ArticleCode.GetHashCode();
      }

      if (ColorCode != null)
      {
        hashCode ^= ColorCode.GetHashCode();
      }

      if (SizeCode != null)
      {
        hashCode ^= SizeCode.GetHashCode();
      }

      return hashCode;
    }

    public String GetColorVendorItemNumber()
    {
      var returnStringBuilder = new StringBuilder(ArticleCode);

      if (!Constants.IgnoreCode.Equals(ColorCode))
      {
        returnStringBuilder.Append(Constants.VendorItemNumberSegmentSeparator);
        returnStringBuilder.Append(ColorCode);
      }
      return returnStringBuilder.ToString();
    }


    public String GetVendorItemNumber()
    {
      var stringBuilder = new StringBuilder(ArticleCode);

      if (!Constants.IgnoreCode.Equals(ColorCode))
      {
        stringBuilder.Append(Constants.VendorItemNumberSegmentSeparator);
        stringBuilder.Append(ColorCode);
      }

      if (!Constants.IgnoreCode.Equals(SizeCode))
      {
        stringBuilder.Append(Constants.VendorItemNumberSegmentSeparator);
        stringBuilder.Append(SizeCode);
      }

      return stringBuilder.ToString();
    }

    public override string ToString()
    {
      return GetVendorItemNumber();
    }

    public void ApplySaleLogic(Article article, ContentPrice priceRule)
    {
      if (priceRule.FromDate != null || priceRule.ToDate != null) //If the UnitPriceIncrease is a number below 1.0 the priceRule is a discount. If it is, the ProFrom and ProTo need to be filled, and the CountryCode needs to be filled with "SLD".
      {
        article.ProFrom = priceRule.FromDate.ToNullOrLocal().ParseToFormatOrReturnEmptyString(Constants.SaleDateFormat);
        article.ProTo = priceRule.ToDate.ToNullOrLocal().ParseToFormatOrReturnEmptyString(Constants.SaleDateFormat);
        article.CountryCode = Constants.SaleTarrif;
      }
    }
  }
}
