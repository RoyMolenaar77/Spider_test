using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Objects.Models.Products
{
  public class ProductCompare : AuditObjectBase<ProductCompare>
  {
    public Int32 CompareProductID { get; set; }

    public Int32? ConnectorID { get; set; }

    public String ConnectorCustomItemNumber { get; set; }

    public String VendorItemNumber { get; set; }

    public Decimal? MinPrice { get; set; }

    public Decimal? MaxPrice { get; set; }

    public Int32 ProductCompareSourceID { get; set; }

    public Boolean? HotSeller { get; set; }

    public Decimal? PriceIndex { get; set; }

    public String UPID { get; set; }

    public String EAN { get; set; }

    public String SourceProductID { get; set; }

    public Decimal? AveragePrice { get; set; }

    public Int32? TotalStock { get; set; }

    public Int32? MinStock { get; set; }

    public Int32? MaxStock { get; set; }

    public Decimal? PriceGroup1Percentage { get; set; }

    public Decimal? PriceGroup2Percentage { get; set; }

    public Decimal? PriceGroup3Percentage { get; set; }

    public Decimal? PriceGroup4Percentage { get; set; }

    public Decimal? PriceGroup5Percentage { get; set; }

    public Decimal? TotalSales { get; set; }

    public Decimal? Popularity { get; set; }

    public Decimal? Price { get; set; }

    public DateTime LastImport { get; set; }

    public virtual ICollection<ProductCompetitorPrice> ProductCompetitorPrices { get; set; }

    public virtual Connector Connector { get; set; }

    public virtual ProductCompareSource ProductCompareSource { get; set; }

    public override System.Linq.Expressions.Expression<Func<ProductCompare, bool>> GetFilter()
    {
      return null;
    }
  }
}