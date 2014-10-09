using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PriceComparison
{
  public enum IceCatProperties
  {
    YourProductCode,
    Supplier,
    VendorProductCode,
    TotalStock,
    MinStock,
    MaxStock,
    MinPrice,
    MaxPrice,
    PriceGroup1,
    PriceGroup2,
    PriceGroup3,
    PriceGroup4,
    PriceGroup5,
    TotalSales,
    YourTotalSales,
    IceCatPopularity
  }

  public enum TweakersProperties
  {
    ProductId,
    Price,
    NumberCompetitors,
    NumberPrices,
    Price1,
    Competitor1,
    Price2,
    Competitor2,
    Price3,
    Competitor3,
    Price4,
    Competitor4,
    Price5,
    Competitor5,
    AveragePrice,
    PriceIndex,
    MinPrice,
    MaxPrice,
    ProductName,
    UPIDs,
    EAN,
    ConnectorCustomItemNumber
  }

  public enum ScanProperties
  {
    ProductName,
    Manufacturer,
    ProductCode,
    Model,
    Price,
    HotSeller
  }

  public static class ColumnDefitionPropertyUtility
  {
    private static List<PriceCompareProperty> _scanDefinitions;
    public static List<PriceCompareProperty> ScanDefinitions
    {
      get
      {
        return new List<PriceCompareProperty>()
                 {
                   new PriceCompareProperty(true, ScanProperties.ProductName.ToString()),
                   new PriceCompareProperty(true, ScanProperties.Manufacturer.ToString()),
                   new PriceCompareProperty(false, ScanProperties.ProductCode.ToString())
                 };
      }
    }
    public static List<PriceCompareProperty> IceCatDefinitions
    {
      get
      {
        List<PriceCompareProperty> list = new List<PriceCompareProperty>
                                            {
                                              new PriceCompareProperty(false, "YourProductCode"),
                                              new PriceCompareProperty(true, "Supplier"),
                                              new PriceCompareProperty(false, "VendorProductCode"),
                                              new PriceCompareProperty(true, "TotalStock"),
                                              new PriceCompareProperty(true, "MinStock"),
                                              new PriceCompareProperty(true, "MaxStock"),
                                              new PriceCompareProperty(false, "MinPrice"),
                                              new PriceCompareProperty(false, "MaxPrice"),
                                              new PriceCompareProperty(true, "PriceGroup1"),
                                              new PriceCompareProperty(true, "PriceGroup2"),
                                              new PriceCompareProperty(true, "PriceGroup3"),
                                              new PriceCompareProperty(true, "PriceGroup4"),
                                              new PriceCompareProperty(true, "PriceGroup5"),
                                              new PriceCompareProperty(true, "TotalSales"),
                                              new PriceCompareProperty(true, "YourTotalSales"),
                                              new PriceCompareProperty(true, "IceCatPopularity"),
                                            };
        return list;
      }
    }

    public static List<PriceCompareProperty> TweakersDefinitions
    {
      get
      {
        List<PriceCompareProperty> list = new List<PriceCompareProperty>
                                            {
                                              new PriceCompareProperty(true, TweakersProperties.ProductId.ToString()),
                                              new PriceCompareProperty(true, TweakersProperties.Price.ToString()),
                                              new PriceCompareProperty(true, TweakersProperties.NumberCompetitors.ToString()),
                                              new PriceCompareProperty(true, TweakersProperties.NumberPrices.ToString()),
                                              new PriceCompareProperty(true, TweakersProperties.Price1.ToString()),
                                              new PriceCompareProperty(true, TweakersProperties.Competitor1.ToString()),
                                              new PriceCompareProperty(true, TweakersProperties.Price2.ToString()),
                                              new PriceCompareProperty(true, TweakersProperties.Competitor2.ToString()),
                                              new PriceCompareProperty(true, TweakersProperties.Price3.ToString()),
                                              new PriceCompareProperty(true, TweakersProperties.Competitor3.ToString()),
                                              new PriceCompareProperty(true, TweakersProperties.Price4.ToString()),
                                              new PriceCompareProperty(true, TweakersProperties.Competitor4.ToString()),
                                              new PriceCompareProperty(true, TweakersProperties.Price5.ToString()),
                                              new PriceCompareProperty(true, TweakersProperties.Competitor5.ToString()),
                                              new PriceCompareProperty(true, TweakersProperties.AveragePrice.ToString()),
                                              new PriceCompareProperty(true, TweakersProperties.PriceIndex.ToString()),
                                              new PriceCompareProperty(false, TweakersProperties.MinPrice.ToString()),
                                              new PriceCompareProperty(false, TweakersProperties.MaxPrice.ToString()),
                                              new PriceCompareProperty(true, TweakersProperties.ProductName.ToString()),
                                              new PriceCompareProperty(true, TweakersProperties.UPIDs.ToString()),
                                              new PriceCompareProperty(true, TweakersProperties.EAN.ToString()),
                                              new PriceCompareProperty(false, TweakersProperties.ConnectorCustomItemNumber.ToString()),
                                            };
        return list;
      }
    }
  }


  public class PriceCompareProperty
  {
    public bool isDynamic
    {
      get;
      set;
    }
    public string PropertyDescriptor
    {
      get;
      set;
    }

    public PriceCompareProperty(bool isDynamic, string descriptor)
    {
      this.isDynamic = isDynamic;
      this.PropertyDescriptor = descriptor;
    }
  }
}
