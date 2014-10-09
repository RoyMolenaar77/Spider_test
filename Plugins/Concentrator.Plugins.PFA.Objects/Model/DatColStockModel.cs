using Concentrator.Vendors.PFA.Helpers;
using FileHelpers;
using System;
using System.Globalization;

namespace Concentrator.Plugins.PFA.Objects.Model
{

  [DelimitedRecord("|")]
  public class DatColStockModel
  {

    public String StoreNumber;

    public String EmployeeNumber;

    [FieldConverter(typeof(PaddedNumberConverter), 4)]
    public Int32 ReceiptNumber;

    [FieldConverter(typeof(PaddedNumberConverter), 2)]
    public String TransactionType;

    [FieldConverter(typeof(PaddedNumberConverter), 12)]
    public String DateNotified;

    [FieldConverter(typeof(PaddedNumberConverter), 2)]
    public String RecordType;

    [FieldConverter(typeof(PaddedNumberConverter), 2)]
    public String SubType;

    [FieldConverter(typeof(QuantityConverter), 7)]
    public Int32 NumberOfSkus;

    [FieldConverter(typeof(QuantityConverter), 10)]
    public Int32 MancoOrSurplus;

    [FieldConverter(typeof(PaddedNumberConverter), 10)]
    public String FixedField1;

    [FieldConverter(typeof(QuantityConverter), 10)]
    public Int32 RecordSequence;

    [FieldConverter(typeof(PaddedNumberConverter), 3)]
    public String FixedField2;

    [FieldConverter(typeof(PaddedNumberConverter), 10)]
    public String FixedField3;

    [FieldConverter(typeof(PaddedNumberConverter), 3)]
    public String FixedField4;

    [FieldConverter(typeof(PaddedNumberConverter), 10)]
    public String FixedField5;

    [FieldConverter(typeof(PaddedNumberConverter), 3)]
    public String FixedField6;

    [FieldConverter(typeof(QuantityConverter), 10)]
    public Decimal OriginalSellingPrice;

    [FieldConverter(typeof(PaddedNumberConverter), 2)]
    public String FixedField7;

    [FieldConverter(typeof(PaddedNumberConverter), 20)]
    public String ArticleNumberColorCodeSizeCode;

    [FieldConverter(typeof(PaddedNumberConverter), 20)]
    public String Barcode;

    [FieldConverter(typeof(PaddedNumberConverter), 9)]
    public String Receipt;

    [FieldConverter(typeof(PaddedNumberConverter), 1)]
    public String TaxCode;

    [FieldConverter(typeof(PaddedNumberConverter), 4)]
    public String EmployeeNumber2;

    [FieldConverter(typeof(PaddedNumberConverter), 1)]
    public Int32 ScannedWithBarcodeReader;

    public String Dummy = String.Empty;
  }
}