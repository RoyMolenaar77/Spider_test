using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Concentrator.Vendors.Sennheiser.Models
{
  public class PriceListModel
  {
    public int ConcentratorProductID { get; set; }
    public string ProductID { get; set; }
    public string Artnr { get; set; }
    public string BrandName { get; set; }
    public string Chapter { get; set; }
    public string Category { get; set; }
    public string Subcategory { get; set; }
    public string PriceGroup { get; set; }
    public string Description1 { get; set; }
    public string Description2 { get; set; }
    public decimal? PriceNL { get; set; }
    public decimal? PriceBE { get; set; }
    public decimal? VatExcl { get; set; }
    public decimal? BEVatExcl { get; set; }
    public string ProductImage { get; set; }
    public string Features { get; set; }
    public string[] Top5Features { get; set; }
    public string New { get; set; }
    public string Image { get; set; }
  }
}