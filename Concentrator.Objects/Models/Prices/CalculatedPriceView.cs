using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Products;

namespace Concentrator.Objects.Models.Prices
{
  public class CalculatedPriceView
  {

    public Int32 ProductID { get; set; }

    public Decimal? PriceEx { get; set; }

    public Decimal? priceInc { get; set; }

    public Decimal? CostPrice { get; set; }

    public Decimal? SpecialPrice { get; set; }

    public Decimal? minPriceInc { get; set; }

    public Decimal? maxPriceInc { get; set; }

    public Int32? competitorcount { get; set; }

    public Int64? RankNumber { get; set; }

    //public Int32? ContentPriceRuleIndex { get; set; }

    public Decimal? OwnPriceInc { get; set; }

    public Decimal? AverageMarketPriceInc { get; set; }

    public Int32? CurrentRank { get; set; }


    public Int32? ConcentratorStatusID { get; set; }

    public string CommercialStatus { get; set; }

    public Decimal? TaxRate { get; set; }

    public Int32? MinimumQuantity { get; set; }

    public Int32? ProductCompareSourceID { get; set; }

    public String ContentPriceLabel { get; set; }

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public Decimal? BottomMargin { get; set; }
    public Int32? ComparePricePosition { get; set; }
    public Int32? MinComparePricePosition { get; set; }
    public Int32? MaxComparePricePosition { get; set; }
    public Decimal? Margin { get; set; }

    public String CustomItemNumber { get; set; }
    public String ShortDescription { get; set; }
    public Int32 VendorID { get; set; }
    public DateTime? lastImport { get; set; }
    public String CompetitorStock { get; set; }
    public Int32 BrandID { get; set; }
    public String VendorItemNumber { get; set; }
    public Int32 ConnectorID { get; set; }
    public String CompetitorSource { get; set; }
  }
}