using System;
using System.Collections.Generic;

using CsvHelper;
using CsvHelper.Configuration;

namespace Concentrator.Tasks.Vlisco.Models
{
  public sealed class ItemMapping : CsvClassMap<Item>
  {
    public ItemMapping()
    {
      Map(i => i.DateTime).Index(0).TypeConverterOption(Constants.Culture.English);
      Map(i => i.DescriptionLong).Index(1);
      Map(i => i.DescriptionShort).Index(2);
      Map(i => i.ArticleCode).Index(3);
      Map(i => i.ColorCode).Index(4);
      Map(i => i.ColorName).Index(5);
      Map(i => i.SizeCode).Index(6);
      Map(i => i.Price).Index(7).TypeConverterOption(Constants.Culture.English);
      Map(i => i.OriginCode).Index(8);
      Map(i => i.OriginName).Index(9);
      Map(i => i.LabelCode).Index(10);
      Map(i => i.LabelName).Index(11);
      Map(i => i.SegmentCode).Index(12);
      Map(i => i.SegmentName).Index(13);
      Map(i => i.GroupCode).Index(14);
      Map(i => i.GroupName).Index(15);
      Map(i => i.Barcode).Index(16);
    }
  }
}
