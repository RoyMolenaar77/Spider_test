using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Prices
{
  public class PriceTag
  {
    public Int32 PriceTagID { get; set; }
          
    public String Name { get; set; }
          
    public String Description { get; set; }
          
    public Boolean IsLandscape { get; set; }
          
    public String PageSize { get; set; }
          
    public Boolean HasFreeTextLine { get; set; }
          
    public String Action { get; set; }
          
    public Int32 MaximumQuantity { get; set; }
          
    public String Icon { get; set; }
          
  }
}