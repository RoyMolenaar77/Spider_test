using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using CsvHelper;
using CsvHelper.Configuration;

namespace Concentrator.Tasks.Vlisco.Models
{
  public sealed class StatisticMapping : CsvClassMap<Statistic>
  {
    public StatisticMapping()
    {
      Map(s => s.DateTime).Index(0).TypeConverterOption(Constants.Culture.English);
      Map(s => s.ShopCode).Index(1);
      Map(s => s.TotalSales).Index(2).TypeConverterOption(Constants.Culture.English);
      Map(s => s.ClientSales).Index(3).TypeConverterOption(Constants.Culture.English);
      Map(s => s.GeneralSales).Index(4).TypeConverterOption(Constants.Culture.English);
      Map(s => s.UnitsSold).Index(5).TypeConverterOption(Constants.Culture.English);
      Map(s => s.UnitsPerClient).Index(6).TypeConverterOption(Constants.Culture.English);
      Map(s => s.TotalAmount).Index(7).TypeConverterOption(Constants.Culture.English);
      Map(s => s.SalesPerClient).Index(8).TypeConverterOption(Constants.Culture.English);
      Map(s => s.VisitorsWithNoSales).Index(9);
      Map(s => s.VisitorCount).Index(10);
      Map(s => s.Context).Index(11);
      Map(s => s.Atmosphere).Index(12);
    }
  }
}
