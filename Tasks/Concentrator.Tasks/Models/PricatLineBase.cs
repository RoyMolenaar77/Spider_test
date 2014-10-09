using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Concentrator.Tasks.Models
{
  public class PricatLineBase
  {
    // This expression splits a string on a comma, unless the comma is in between double quotes, then it becomes part of the value.
    public static readonly Regex SplitRegex = new Regex(
      "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*)\\s*,\\s*(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)",
      RegexOptions.Compiled | RegexOptions.Singleline);
  }
}