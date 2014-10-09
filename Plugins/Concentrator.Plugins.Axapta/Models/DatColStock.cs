using System;
using System.Collections.Generic;
using FileHelpers;
using System.Text.RegularExpressions;

namespace Concentrator.Plugins.Axapta.Models
{
  [DelimitedRecord(";")]
  [IgnoreEmptyLines]
  public class DatColStock
  {
    public string ModelCode;
    public string CustomItemNumber;
    public string StockWarehouse;
    public int Quantity;

    public string CustomItemNumberWithoutWhiteSpace
    {
      get
      {
        return Regex.Replace(CustomItemNumber, @"\s+", "");
      }
    }
  }

  [DelimitedRecord(";")]
  [IgnoreEmptyLines]
  public class DatColStockMutation
  {
    public string VendorItemNumber;
    public string FromStockWarehouse;
    public string ToStockWarehouse;

    [FieldConverter(ConverterKind.Date, "dd-MM-yyyy")]
    public DateTime StockTransferDate = DateTime.Now;

    public int Quantity;

    public bool SetVendorItemNumber(string vendorItemNumber, string color, string size)
    {
      var axaptaVendorItemNumber = new DatColAxaptaVendorItemNumber();

      if (axaptaVendorItemNumber.SetVendorItemNumber(vendorItemNumber, color, size))
      {
        try
        {
          var engine = new FileHelperEngine(typeof(DatColAxaptaVendorItemNumber));
          var vendorItemNumberString = engine.WriteString(new List<DatColAxaptaVendorItemNumber> { axaptaVendorItemNumber });

          VendorItemNumber = vendorItemNumberString.TrimEnd();
          return true;
        }
        catch (Exception)
        {
          return false;
        }        
      }
      return false;
    }
  }

  [FixedLengthRecord]
  public class DatColAxaptaVendorItemNumber
  {
    [FieldFixedLength(15)] 
    public string ModelCode;

    [FieldFixedLength(15)] 
    public string ColorCode;

    [FieldFixedLength(10)]
    public string Size;
    
    [FieldFixedLength(10)]
    public string SubSize;

    public bool SetVendorItemNumber(string vendorItemNumber, string color, string size)
    {
      var vendorItemNumberParts = vendorItemNumber.Split(' ');

      if (vendorItemNumberParts.Length > 0)
      {
        ModelCode = vendorItemNumberParts[0];
        ColorCode = color;

        var sizeParts = size.ToCharArray();
        if (sizeParts.Length > 0)
        {
          if (Char.IsNumber(sizeParts[0]))
          {
            Size = Regex.Split(size, @"\D+")[0];
            SubSize = Regex.Split(size, @"\d+")[1];
          }
          else
          {
            Size = size;
            SubSize = string.Empty;
          }
          return true;          
        }
      }
      return false;
    }
  }
}
