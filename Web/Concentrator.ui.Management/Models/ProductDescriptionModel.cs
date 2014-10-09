using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Concentrator.ui.Management.Models
{
  public class ProductDescriptionModel
  {
    public string ShortContentDescription { get; set; }
    public string Url { get; set; }
    public int VendorID { get; set; }
    public string ProductName { get; set; }
    public string PDFUrl { get; set; }
    public string LongContentDescription { get; set; }
    public string MaterialOptionID { get; set; }
    public string MaterialDescription { get; set; }
    public string DessinOptionID { get; set; }
    public string KraagvormOptionID { get; set; }
    public string PijpwijdteOptionID { get; set; }
    public string ShortSummaryDescription { get; set; }
    public string LongSummaryDescription { get; set; }
  }
}