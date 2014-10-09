using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Utility;
using System.Globalization;

namespace Concentrator.Objects.Logic
{
  public class PriceRounder
  {

    private static Evaler _eval;

    public PriceRounder()
    {
      _eval = new Evaler();
    }


    /// <summary>
    /// Rounds a price based on an expression.
    /// Expression needs to be a price rounding formula    
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="price"></param>
    /// <returns></returns>
    public decimal RoundPrice(string expression, decimal price, decimal? margin = null)
    {
      //declare result equals to starting value
      decimal resultingPrice = price;

      var lastIndexOfEq = expression.LastIndexOf('=');
      var isMargin = expression.IndexOf('m') >= 0;
      var val = expression.Substring(lastIndexOfEq + 1).Trim();
      if ((val.StartsWith(",")) || (val.StartsWith(".")))
      {
        val = String.Format("0{0}", val);
      }
      //var roundValue = decimal.Parse(expression.Substring(lastIndexOfEq + 1).Trim());
      var roundValue = decimal.Parse(val,new CultureInfo("nl-NL"));
      expression = expression.Replace(',', '.');
      
      //get boolean condition
      expression = expression.Substring(0, lastIndexOfEq);
      string condition = string.Empty;
      if (isMargin) //margin
      {
        if (margin.HasValue)
          condition = expression.Replace("m", margin.ToString());
      }
      //fixed value
      else
      {
        condition = expression.Replace("x", price.ToString());
        condition = condition.Replace("*", ((int)price).ToString()); //whole number
      }
      condition = condition.Replace(',', '.');
      if ((Boolean)_eval.EvalExpression(condition))
      {
        int p = (int)price;
        resultingPrice = (decimal)p + roundValue;
      }

      return resultingPrice;

    }
  }
}
