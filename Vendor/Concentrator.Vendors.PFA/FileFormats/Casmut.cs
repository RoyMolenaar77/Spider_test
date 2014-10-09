using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileHelpers;
//using Concentrator.Vendors.PFA.Helpers;
using System.IO;
using Concentrator.Vendors.PFA.Helpers;

namespace Concentrator.Vendors.PFA.FileFormats
{
  [FixedLengthRecord]
  public class Casmut
  {
    [FieldFixedLength(1)]
    [FieldConverter(typeof(DefaultValueConverter), "M")]
    private string Default;
    [FieldFixedLength(8)]
    [FieldConverter(typeof(DateConverter))]
    public DateTime SalesDate;
    [FieldFixedLength(2)]
    [FieldConverter(typeof(DefaultValueConverter), "01")]
    private string RecType;
    [FieldFixedLength(3)]
    public int ShopNumber;
    [FieldFixedLength(4)]
    [FieldConverter(typeof(PaddedNumberConverter), 4)]
    public int PosNumber;
    [FieldFixedLength(4)]
    [FieldConverter(typeof(PaddedNumberConverter), 4)]
    public int TicketNumber;
    [FieldFixedLength(14)]
    [FieldConverter(typeof(PaddedNumberConverter), 14)]
    public int PickTicketNumber;
    [FieldFixedLength(4)]
    [FieldConverter(typeof(DefaultValueConverter), "0004")]
    private string SalesPerson;
    [FieldFixedLength(12)]
    public string EAN;
    [FieldFixedLength(24)]
    [FieldConverter(typeof(PaddedNumberConverter), 24)]
    private int default24x0;
    [FieldFixedLength(6)]
    [FieldConverter(typeof(SignConverter), "+", 5)]
    public int SKUCount;
    [FieldFixedLength(10)]
    [FieldConverter(typeof(SignConverter), "+", 9)]
    public int SalesValue;
    [FieldFixedLength(95)]
    [FieldConverter(typeof(PaddedNumberConverter), 95)]
    private int default95x0;
  }
}
