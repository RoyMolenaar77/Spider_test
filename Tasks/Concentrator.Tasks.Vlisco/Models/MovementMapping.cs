using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using CsvHelper;
using CsvHelper.Configuration;

namespace Concentrator.Tasks.Vlisco.Models
{
  public sealed class MovementMapping : CsvClassMap<Movement>
  {
    public MovementMapping()
    {
      Map(m => m.ShopCode).Index(0);
      Map(m => m.MovementNumber).Index(1);
      Map(m => m.MovementType).Index(2);
      Map(m => m.MovementDescription).Index(3);
      Map(m => m.ArticleCode).Index(4);
      Map(m => m.ColorCode).Index(5);
      Map(m => m.SizeCode).Index(6);
      Map(m => m.MovementDirection).Index(7);
      Map(m => m.MovementDate).Index(8).TypeConverterOption(Constants.Culture.English);
      Map(m => m.MovementTime).Index(9).TypeConverterOption(Constants.Culture.English);
      Map(m => m.SalesPerson).Index(10);
      Map(m => m.CostPrice).Index(11).TypeConverterOption(Constants.Culture.English);
      Map(m => m.UnitPrice).Index(12).TypeConverterOption(Constants.Culture.English);
      Map(m => m.DocumentNumber).Index(13);
      Map(m => m.DocumentLine).Index(14);
      Map(m => m.LocationFrom).Index(15);
      Map(m => m.LocationTo).Index(16);
      Map(m => m.Quantity).Index(17);
      Map(m => m.TransactionNumber).Index(18);
      Map(m => m.LotNumber).Index(19);
    }
  }
}
