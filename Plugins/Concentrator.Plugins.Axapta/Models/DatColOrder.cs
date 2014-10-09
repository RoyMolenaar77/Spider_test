using FileHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Axapta.Models
{
  [DelimitedRecord(",")]
  public class DatColEnvelope
  {
    [FieldQuoted('"')]
    public string EnvelopeCode = "ENV";
    [FieldQuoted('"')]
    public string ILNClientNumber;
    [FieldQuoted('"')]
    public string ILNSapphNumber;
    [FieldQuoted('"')]
    public string MagentoOrderNumber;
    public int FixedField = 0;
  }

  [DelimitedRecord(",")]
  public class DatColHeader
  {
    [FieldQuoted('"')]
    public string HeaderCode = "HDR";
    [FieldQuoted('"')]
    public string Identifyer = "74";
    [FieldQuoted('"')]
    public string MagentoOrderNumber;
    [FieldConverter(ConverterKind.Date, "yyyyMMdd")]
    public DateTime ShipmentDate = DateTime.Now;
    [FieldConverter(ConverterKind.Date, "yyyyMMdd")]
    public DateTime OrderDate = DateTime.Now;
    [FieldConverter(ConverterKind.Date, "yyyyMMdd")]
    public DateTime RequestedReceiptDate = DateTime.Now.AddDays(1);
    [FieldQuoted('"')]
    public string FixedField1 = "EAN";
    [FieldQuoted('"')]
    public string ILNSapphNumber;
    [FieldQuoted('"')]
    public string FixedField2 = "EAN";
    [FieldQuoted('"')]
    public string ILNClientNumber;
    [FieldQuoted('"')]
    public string EANIdentifier = "EAN";
    [FieldQuoted('"')]
    public string ILNClientNumber2;
    [FieldQuoted('"')]
    public string EmptyField1 = string.Empty;
  }

  [DelimitedRecord(",")]
  public class DatColDate
  {
    [FieldQuoted('"')]
    public string DateCode = "PER";
    [FieldConverter(ConverterKind.Date, "yyyyMMdd")]
    public DateTime Today = DateTime.Now;
  }

  [DelimitedRecord(",")]
  public class DatColOrderLine
  {
    [FieldQuoted('"')]
    public string OrderLineCode = "LIN";
    public int OrderLineNumber;
    [FieldQuoted('"')]
    public string FixedField = "1";
    [FieldQuoted('"')]
    public string FixedField1 = "EAN";
    [FieldQuoted('"')]
    public string Barcode;
    [FieldQuoted('"')]
    public string EmptyField1 = string.Empty;
    [FieldQuoted('"')]
    public string EmptyField2 = string.Empty;
    [FieldQuoted('"')]
    public string EmptyField3 = string.Empty;
    [FieldQuoted('"')]
    public string EmptyField4 = string.Empty;
    [FieldQuoted('"')]
    public string EmptyField5 = string.Empty;
    [FieldQuoted('"')]
    public string EmptyField6 = string.Empty;
    [FieldQuoted('"')]
    public string EmptyField7 = string.Empty;
    [FieldQuoted('"')]
    public string EmptyField8 = string.Empty;
    [FieldQuoted('"')]
    public string EmptyField9 = string.Empty;
    [FieldQuoted('"')]
    public string EmptyField10 = string.Empty;
    [FieldQuoted('"')]
    public string EmptyField11 = string.Empty;
    [FieldQuoted('"')]
    public string EmptyField12 = string.Empty;
    public int Quantity;
    public string EmptyField13 = string.Empty;
    public string EmptyField14 = string.Empty;
    public string EmptyField15 = string.Empty;
    public decimal TotalUnitPrice;
    public string EmptyField16 = string.Empty;
    public string EmptyField17 = string.Empty;
    public string EmptyField18 = string.Empty;
    public string ClosingRow = "";
  }

  [DelimitedRecord(",")]
  public class DatColCount
  {
    [FieldQuoted('"')]
    public string CountCode = "CNT";
    public int TotalOrderLine;
    public int TotalQuantity;
  }
}
