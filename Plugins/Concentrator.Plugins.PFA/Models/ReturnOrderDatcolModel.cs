using Concentrator.Vendors.PFA.Helpers;
using FileHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PFA.Models
{
  [DelimitedRecord("|")]
  internal class ReturnOrderDatcolModel
  {
    [FieldConverter(typeof(PaddedNumberConverter), 6)]
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
    public int NumberOfDifferences;

    [FieldConverter(typeof(QuantityConverter), 10)]
    public int ReceivingStore;

    [FieldConverter(typeof(QuantityConverter), 10)]
    public int FixedField2 = 0;

    [FieldConverter(typeof(QuantityConverter), 10)]
    public int RecordSequence;

    [FieldConverter(typeof(PaddedNumberConverter), 3)]
    public int FixedField3 = 0;

    [FieldConverter(typeof(QuantityConverter), 10)]
    public int FixedField4 = 0;

    [FieldConverter(typeof(PaddedNumberConverter), 3)]
    public int FixedField5 = 0;

    [FieldConverter(typeof(QuantityConverter), 10)]
    public int FixedField6 = 0;

    [FieldConverter(typeof(PaddedNumberConverter), 3)]
    public int FixedField7 = 0;

    [FieldConverter(typeof(QuantityConverter), 10)]
    public int Price;

    [FieldConverter(typeof(PaddedNumberConverter), 2)]
    public int FixedField9 = 0;

    [FieldConverter(typeof(PaddedNumberConverter), 20)]
    public String SkuVendorItemNumber;

    public String Barcode;

    [FieldConverter(typeof(PaddedNumberConverter), 9)]
    public String TransferNumber;

    public string FixedField13 = "1";

    [FieldConverter(typeof(PaddedNumberConverter), 4)]
    public String EmployeeNumber2;

    [FieldConverter(typeof(PaddedNumberConverter), 1)]
    public Int32 ScannedIndication = 0;

    public String Dummy = String.Empty;
  }
}
