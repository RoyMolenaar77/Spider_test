using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
                  
namespace Concentrator.Objects.Models.Prices 
{
  public class PriceCorrection
  {
    public Int32 PriceCorrectionID { get; set; }
          
    public String ProductID { get; set; }
          
    public String AdditionalLine { get; set; }

    public DateTime CreationTime { get; set; }          
  }
}