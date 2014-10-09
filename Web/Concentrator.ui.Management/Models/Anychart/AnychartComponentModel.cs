using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;

namespace Concentrator.ui.Management.Models.Anychart
{
  public class AnychartComponentModel
  {
    /// <summary>
    /// A serie - collection of points
    /// </summary>
    public IEnumerable<Serie> Series
    {
      get;
      private set;
    }

    public string XAxesType
    {
      get;
      private set;
    }

    public AnychartComponentModel(IEnumerable<Serie> serie, String xAxesType = null)
    {
      Series = serie.ToList();
      XAxesType = xAxesType ?? String.Empty;
    }
  }
}