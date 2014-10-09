using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;

namespace Concentrator.ui.Management.Models.Anychart
{
  public class PieChartPoint : Point
  {
    public PieChartPoint(string _name, object _value, string color = "Blue", AnychartAction action = null)
      : base(_name, _value, action)
    {
      _color = color;
    }

    private string _color;

    /// <summary>
    /// A color to use on the point
    /// Can be color name(black, red) or HEX (#EEEEEE)
    /// </summary>
    public string Color
    {
      get { return _color; }
    }
  }
}