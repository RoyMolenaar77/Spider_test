namespace Concentrator.Core.Services.Models.Products
{
  public class Price
  {
    public decimal TaxRate { get; set; }

    public string CommercialStatus { get; set; }

    public int MinimumQuantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal CostPrice { get; set; }

    public decimal? SpecialPrice { get; set; }
  }
}
