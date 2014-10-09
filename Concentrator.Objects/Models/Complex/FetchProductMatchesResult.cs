using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;

namespace Concentrator.Objects.Models.Complex
{
  public class FetchProductMatchesResult : ComplexObject
  {
    public Int32 ProductMatchID { get; set; }
    public Int32 ProductID { get; set; }
    public bool isMatched { get; set; }
    public Int32? MatchPercentage { get; set; }
    public Int32 MatchStatus { get; set; }
    public bool? CalculatedMatch { get; set; }
  }
}
