using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Concentrator.Objects.Logic
{
  public class PriceRoundingFormulaParser
  {
    /// <summary>
    /// Indicates whether the formula is related to a fixed amount or a margin
    /// </summary>
    public bool IsMargin { get; private set; }

    /// <summary>
    /// The value that a price should be rounded to
    /// </summary>
    public decimal RoundValue { get; private set; }

    public string Formula { get; private set; }

    public PriceRoundingFormulaParser(string formula)
    {
      ParseCondition(formula);
    }

    private void ParseCondition(string formula)
    {
      var lastIndexOfEq = formula.LastIndexOf('=');
      IsMargin = formula.IndexOf('m') >= 0;

      RoundValue = decimal.Parse(formula.Substring(lastIndexOfEq + 1).Trim(), new CultureInfo("nl-NL"));

      //parse formula in correct format
      formula = formula.Replace(',', '.');

      Formula = formula.Substring(0, lastIndexOfEq);
    }
  }
}
