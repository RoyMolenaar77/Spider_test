using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Tasks.Models
{
  public class  PricatLine : PricatLineBase
  {
    public String Number;
    public String ArticleType;
    public String ArticleID;
    public String SupplierCode;
    public String ArticleGroupCode;
    public String ArticleGroupDescription;
    public String Description;
    public String Brand;
    public String Model;
    public String ColorSupplier;
    public String ColorCode;
    public String SizeRulerSupplier;
    public String SizeSupplier;
    public String SubsizeSupplier;

    public String VAT;
    public String NettoPrice;
    public String AdvicePrice;
    public String LabelPrice;

    public static PricatLine Parse(String line)
    {
      var result = new PricatLine();
      var values = SplitRegex
        .Split(line)
        .Select(value => value.Trim('\"'))
        .ToArray();

      if (values.Length != 65)
      {
        return null;
      }

      result.Number = values[01];
      result.ArticleType = values[05];
      result.ArticleID = values[06];
      result.SupplierCode = values[07];
      result.ArticleGroupCode = values[13];
      result.ArticleGroupDescription = values[14];
      result.Description = values[48];
      result.Brand = values[17];
      result.Model = values[18];
      result.ColorSupplier = values[26];
      result.ColorCode = values[30];
      result.SizeRulerSupplier = values[34];
      result.SizeSupplier = values[35];
      result.SubsizeSupplier = values[36];

      result.VAT = values[59];
      result.NettoPrice = values[60];
      result.AdvicePrice = values[62];
      result.LabelPrice = values[63];

      return result;
    }

    private PricatLine()
    {
    }
  }
}