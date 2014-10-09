using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using CsvHelper;
using CsvHelper.Configuration;

namespace Concentrator.Tasks.Vlisco.Models
{
  public sealed class StockMapping : CsvClassMap<Stock>
  {
    public StockMapping()
    {
      Map(stock => stock.DateTime).Index(0).TypeConverterOption(Constants.Culture.English);
      Map(stock => stock.ShopCode).Index(1);
      Map(stock => stock.ArticleCode).Index(2);
      Map(stock => stock.ColorCode).Index(3);
      Map(stock => stock.SizeCode).Index(4);
      Map(stock => stock.InStock).Index(5);
      Map(stock => stock.Maximum).Index(6);
      Map(stock => stock.Minimum).Index(7);
      Map(stock => stock.Reserved).Index(8);
      Map(stock => stock.Ordered).Index(9);
      Map(stock => stock.Delivered).Index(10);
      Map(stock => stock.Available).Index(11);
      Map(stock => stock.TotalIn).Index(12);
      Map(stock => stock.TotalOut).Index(13);
      Map(stock => stock.WAC).Index(14);
    }
  }
}
