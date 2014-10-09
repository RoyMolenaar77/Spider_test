using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Concentrator.ui.Management.Models.Anychart
{
  public class Point
  {
    /// <summary>
    /// Name of the point
    /// </summary>
    public string Name
    {
      get
      {
        return _name;
      }
    }

    /// <summary>
    /// An Anychart Action object for this point
    /// </summary>
    public AnychartAction Action { get; private set; }

    private string _name;

    /// <summary>
    /// Value of the point. 
    /// </summary>
    public object Value
    {
      get
      {
        return _value;
      }
    }

    private object _value;

    public Point(string _name, object _value, AnychartAction _action = null)
    {
      this._name = _name;
      this._value = _value;
      Action = _action;
    }
  }
}