using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileHelpers;
using Concentrator.Vendors.PFA.Helpers;

namespace Concentrator.Vendors.PFA.FileFormats
{
  [DelimitedRecord("|")]
  public class DatColReceiveRegular
  {
    public string ShopAndPosNr;
    public int EmployeeNumber;
    public int SalesslipNumber;
    public  string TransactionType = "21";
    [FieldConverter(ConverterKind.Date, "yyyyMMddHHmm")]
    public DateTime DateStamp;
    public string RecordType = "99";
    public string SubType = "02";
    [FieldConverter(typeof(QuantityConverter), 7)]
    public int Quantity;
    [FieldConverter(typeof(QuantityConverter))]
    public int ReceivedFrom = 1;
    public string FixedField0 = "000000000+";
    [FieldConverter(typeof(QuantityConverter))]
    public int NumberOfItemsForBox;
    public string FixedField1 = "000";
		[FieldConverter(typeof(QuantityConverter), 10)]
		public int FixedField2;
    public string FixedField3 = "000";
    public string FixedField4 = "000000000+";
    public string FixedField5 = "000";
    public string FixedField6 = "000000000+";
    public string FixedField7 = "00";
    public string FixedField8 = "00000000000000000000";
    public string FixedField9 = "00000000000000000000";
    public string TransferNumber;
    public string FixedField10 = "0";
    public int EmployeeNumber2;
    public int ScannedWithBarcodeReader = 1;
    public string Dummy="";
  }

  [DelimitedRecord("|")]
  public class DatColTransfer
  {
    public string ShopAndPosNr;
    [FieldConverter(typeof(PaddedNumberConverter), 4)]
    public int EmployeeNumber = 0004;
    [FieldConverter(typeof(PaddedNumberConverter), 4)]
    public int SalesslipNumber;
    public string TransactionType = "20";
    [FieldConverter(ConverterKind.Date, "yyyyMMddHHmm")]
    public DateTime DateStamp;
    public string RecordType = "01";
    public string SubType = "00";
    [FieldConverter(typeof(QuantityConverter), 7)]
    public int Quantity;
    [FieldConverter(typeof(PaddedNumberConverter), 10)]
    public int ReceivedFrom;
    [FieldConverter(typeof(QuantityConverter))]
    public int MarkdownValue;
    [FieldConverter(typeof(QuantityConverter))]
    public int RecordSequence;
    public string FixedField1 = "000";
		[FieldConverter(typeof(QuantityConverter), 10)]
		public int FixedField2;
    public string FixedField3 = "000";
    public string FixedField4 = "000000000+";
    public string FixedField5 = "000";
    [FieldConverter(typeof(QuantityConverter))]
    public int OriginalRetailValue;
    public string FixedField8 = "00";

    [FieldConverter(typeof(PaddedNumberConverter), 20)]
    public string ArticleColorSize;
    [FieldConverter(typeof(PaddedNumberConverter), 20)]
    public string Barcode;
    public string TransferNumber;
    
    public string FixedField11 = "1";
    [FieldConverter(typeof(PaddedNumberConverter), 4)]
    public int EmployeeNumber2 = 0004;
    public string FixedField12 = "0";
    public string Dummy = "";
  }

  [DelimitedRecord("|")]
  public class DatColNormalSales
  {
    public string ShopAndPosNr;
    [FieldConverter(typeof(PaddedNumberConverter), 4)]
    public int EmployeeNumber = 0004;
    [FieldConverter(typeof(PaddedNumberConverter), 4)]
    public int SalesslipNumber;
    public string TransactionType = "01";
    [FieldConverter(ConverterKind.Date, "yyyyMMddHHmm")]
    public DateTime DateStamp;
    public string RecordType = "01";
    public string SubType = "00";
    [FieldConverter(typeof(QuantityConverter), 7)]
    public int Quantity;
    [FieldConverter(typeof(QuantityConverter), 10)]
    public int ReceivedFrom;
    [FieldConverter(typeof(QuantityConverter), 10)]
    public int MarkdownValue;
    [FieldConverter(typeof(QuantityConverter), 10)]
    public int Discount;
    public string FixedField1 = "000";
		[FieldConverter(typeof(QuantityConverter), 10)]
		public int FixedField2;
    public string FixedField3 = "000";
    public string FixedField4 = "000000000+";
    public string FixedField5 = "000";
    [FieldConverter(typeof(QuantityConverter), 10)]
    public int Revenue;
    public string FixedField6 = "00";
    [FieldConverter(typeof(PaddedNumberConverter), 20)]
    public string ArticleColorSize;
    [FieldConverter(typeof(PaddedNumberConverter), 20)]
    public string Barcode;
    public string TransferNumber = "000000000";
    public string VatCode = "1";
    [FieldConverter(typeof(PaddedNumberConverter), 4)]
    public int EmployeeNumber2 = 0004;
    public int ScannedWithBarcodeReader = 1;
    public string Dummy = "";
  }

  [DelimitedRecord("|")]
  public class DatColReturn
  {
    public string ShopAndPosNr;
    [FieldConverter(typeof(PaddedNumberConverter), 4)]
    public int EmployeeNumber = 0004;
    [FieldConverter(typeof(PaddedNumberConverter), 4)]
    public int SalesslipNumber;
    public string TransactionType = "01";
    [FieldConverter(ConverterKind.Date, "yyyyMMddHHmm")]
    public DateTime DateStamp;
    public string RecordType = "01";
    public string SubType = "00";
    [FieldConverter(typeof(QuantityConverter), 7)]
    public int Quantity;
    [FieldConverter(typeof(QuantityConverter), 10)]
    public int ReceivedFrom;
    [FieldConverter(typeof(QuantityConverter), 10)]
    public int MarkdownValue;
    [FieldConverter(typeof(QuantityConverter), 10)]
    public int Discount;
    public string FixedField1 = "000";
		[FieldConverter(typeof(QuantityConverter), 10)]
		public int FixedField2;
    public string FixedField3 = "000";
    public string FixedField4 = "000000000+";
    public string FixedField5 = "000";
    [FieldConverter(typeof(QuantityConverter), 10)]
    public int Revenue;
    public string FixedField6 = "22";
    [FieldConverter(typeof(PaddedNumberConverter), 20)]
    public string ArticleColorSize;
    [FieldConverter(typeof(PaddedNumberConverter), 20)]
    public string Barcode;
    public string TransferNumber = "000000000";
    public string VatCode = "1";
    [FieldConverter(typeof(PaddedNumberConverter), 4)]
    public int EmployeeNumber2 = 0004;
    public int ScannedWithBarcodeReader = 1;
    public string Dummy = "";
  }
}
