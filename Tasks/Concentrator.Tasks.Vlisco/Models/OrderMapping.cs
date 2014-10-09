using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using CsvHelper;
using CsvHelper.Configuration;

namespace Concentrator.Tasks.Vlisco.Models
{
  public sealed class OrderMapping : CsvClassMap<Order>
  {
    public OrderMapping()
    {
      Map(o => o.OrderDate).Index(0).TypeConverterOption(Constants.Culture.English);
      Map(o => o.OrderTime).Index(1).TypeConverterOption(Constants.Culture.English);
      Map(o => o.SaleType).Index(2);
      Map(o => o.Ticket).Index(3);
      Map(o => o.Line).Index(4);
      Map(o => o.ArticleCode).Index(5);
      Map(o => o.ColorCode).Index(6);
      Map(o => o.SizeCode).Index(7);
      Map(o => o.Quantity).Index(8);
      Map(o => o.PurchasePrice).Index(9).TypeConverterOption(Constants.Culture.English);
      Map(o => o.SalePrice).Index(10).TypeConverterOption(Constants.Culture.English);
      Map(o => o.BrutoPrice).Index(11).TypeConverterOption(Constants.Culture.English);
      Map(o => o.VAT).Index(12).TypeConverterOption(Constants.Culture.English);
      Map(o => o.NettoPrice).Index(13).TypeConverterOption(Constants.Culture.English);
      Map(o => o.DiscountPercentage).Index(14).TypeConverterOption(Constants.Culture.English);
      Map(o => o.DiscountValue).Index(15).TypeConverterOption(Constants.Culture.English);
      Map(o => o.ShopCode).Index(16);
      Map(o => o.SalesPerson).Index(17);
      Map(o => o.Client).Index(18);
    }
  }
}
