using Concentrator.Vendors.PFA.Helpers;
using FileHelpers;
using System;

namespace Concentrator.Plugins.PFA.Transfer.Model.ReceivedTransfer
{

  [DelimitedRecord("|")]
  public class ReceivedTransferMutation
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

    [FieldConverter(typeof(PaddedNumberConverter), 7)]
    public String NumberOfDifferences;

    [FieldConverter(typeof(PaddedNumberConverter), 10)]
    public String FixedField1;

    [FieldConverter(typeof(QuantityConverter), 10)]
    public int FixedField2;

    [FieldConverter(typeof(PaddedNumberConverter), 10)]
    public String NumberOfSkus;

    [FieldConverter(typeof(PaddedNumberConverter), 3)]
    public String FixedField3;

    [FieldConverter(typeof(PaddedNumberConverter), 10)]
    public String FixedField4;

    [FieldConverter(typeof(PaddedNumberConverter), 3)]
    public String FixedField5;

    [FieldConverter(typeof(QuantityConverter), 10)]
    public int FixedField6;

    [FieldConverter(typeof(PaddedNumberConverter), 3)]
    public String FixedField7;

    [FieldConverter(typeof(QuantityConverter), 10)]
    public int FixedField8;

    [FieldConverter(typeof(PaddedNumberConverter), 2)]
    public String FixedField9;

    [FieldConverter(typeof(PaddedNumberConverter), 20)]
    public String FixedField10;

    public String FixedField11;

    [FieldConverter(typeof(PaddedNumberConverter), 9)]
    public String TransferNumber;

    public string FixedField13 = "0";

    [FieldConverter(typeof(PaddedNumberConverter), 4)]
    public String EmployeeNumber2;

    [FieldConverter(typeof(PaddedNumberConverter), 1)]
    public Int32 ScannedIndication;

    public String Dummy = String.Empty;
  }
}
