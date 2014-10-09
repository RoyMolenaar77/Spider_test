using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.CalculatedPrice
{
  public class CalculatedPriceModel
  {
    public int ActivePricePosition { get; set; }
    public int FirstPosition { get; set; }
    public int SecondPosition { get; set; }
    public decimal? BottomMargin { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Index { get; set; }
    public int ProductGroupMappingID { get; set; }
    public int ConnectorID { get; set; }
  }
}
