using System;
using CsvHelper.Configuration;

namespace Concentrator.Tasks.Vlisco.Models
{
  internal class ArticleMapping : CsvClassMap<Article>
  {
    public ArticleMapping()
    {
      Map(c => c.ArticleCode).Index(0);
      Map(c => c.ColorCode).Index(1).Default(Constants.IgnoreCode);
      Map(c => c.SizeCode).Index(2).Default(Constants.IgnoreCode);
      Map(c => c.DescriptionLong).Index(3).Default(String.Empty);
      Map(c => c.DescriptionShort).Index(4).Default(String.Empty);
      Map(c => c.ColorName).Index(5).Default(Constants.IgnoreCode);
      Map(c => c.CostPrice).Index(6).TypeConverterOption(Constants.Culture.Dutch);
      Map(c => c.Price).Index(7).TypeConverterOption(Constants.Culture.Dutch);
      Map(c => c.Stock).Index(8).Default(0);
      Map(c => c.CategoryName).Index(9).Default(Constants.DiversName);
      Map(c => c.CategoryCode).Index(10).Default(Constants.DiversCode);
      Map(c => c.FamilyName).Index(11).Default(Constants.DiversName);
      Map(c => c.FamilyCode).Index(12).Default(Constants.DiversCode);
      Map(c => c.SubfamilyName).Index(13).Default(Constants.DiversName);
      Map(c => c.SubfamilyCode).Index(14).Default(Constants.DiversCode);
      Map(c => c.Barcode).Index(15).Default(String.Empty);
      Map(c => c.ReplenishmentMinimum).Index(16).TypeConverterOption(Constants.Culture.Dutch);
      Map(c => c.ReplenishmentMaximum).Index(17).TypeConverterOption(Constants.Culture.Dutch);
      Map(c => c.Tax).Index(18).TypeConverterOption(Constants.Culture.Dutch);
      Map(c => c.Collection).Index(19).Default(Constants.DiversCode);
      Map(c => c.ReferenceCode).Index(20).Default(String.Empty);
      Map(c => c.SupplierName).Index(21).Default(String.Empty);
      Map(c => c.SupplierCode).Index(22).Default(String.Empty);
      Map(c => c.LabelCode).Index(23).Default(Constants.MissingCode);
      Map(c => c.MaterialCode).Index(24).Default(Constants.MissingCode);
      Map(c => c.ShapeCode).Index(25).Default(Constants.MissingCode);
      Map(c => c.OriginCode).Index(26).Default(Constants.MissingCode);
      Map(c => c.ZoneCode4).Index(27).Default(Constants.MissingCode);
      Map(c => c.ZoneCode5).Index(28).Default(Constants.MissingCode);
      Map(c => c.CountryCode).Index(29).Default(String.Empty);
      Map(c => c.CurrencyCode).Index(30).Default(String.Empty);
      Map(c => c.StockType).Index(31).Default(String.Empty);
    }
  }
}