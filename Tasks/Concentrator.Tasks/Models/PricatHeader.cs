using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Tasks.Models
{
  public class PricatHeader : PricatLineBase
  {
    public String MessageNumber;
    public String MessageDate;
    public String SupplierType;
    public String Supplier;
    public String BuyerType;
    public String Buyer;
    public String Currency;

    public static PricatHeader Parse(String line)
    {
      var result = new PricatHeader();
      var values = SplitRegex
        .Split(line)
        .Select(value => value.Trim('\"'))
        .ToArray();

      if (values.Length != 9)
      {
        return null;
      }

      result.MessageNumber = values[2];
      result.MessageDate = values[3];
      result.SupplierType = values[4];
      result.Supplier = values[5];
      result.BuyerType = values[6];
      result.Buyer = values[7];
      result.Currency = values[8];

      return result;
    }

    private PricatHeader()
    {
    }
  }
}