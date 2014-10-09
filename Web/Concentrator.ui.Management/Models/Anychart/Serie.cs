using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Concentrator.ui.Management.Models.Anychart
{
  public class Serie : IEnumerable<Point>
  {
    public string Name
    {
      get;
      private set;
    }

    public string Palette
    {
      get;
      private set;
    }

    public IEnumerable<Point> Points
    {
      get;
      private set;
    }

    public Serie(IEnumerable<Point> points, string name, string palette = "")
    {
      Name = name;
      Palette = palette;
      Points = points.ToList();
    }

    #region IEnumerable<Point> Members

    public IEnumerator<Point> GetEnumerator()
    {
      return Points.GetEnumerator();
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion
  }
}