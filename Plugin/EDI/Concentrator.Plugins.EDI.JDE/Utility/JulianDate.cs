using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.EDI.JDE.Utility
{
  public class JulianDate
  {
    private DateTime _dateTime = DateTime.Now;

    public decimal Value
    {
      get
      {
        decimal value = (_dateTime.Year - 1900) * 1000;
        value += (_dateTime.DayOfYear);
        return value;
      }
      set
      {
        _dateTime = new DateTime(1900 + (int)(value / 1000), 1, 1);
        _dateTime = _dateTime.AddDays((int)(value % 1000) - 1);
      }
    }

    public DateTime Date
    {
      get { return _dateTime; }
    }

    public static JulianDate Now
    {
      get
      {
        return new JulianDate();
      }
    }

    public JulianDate(DateTime dt)
    {
      _dateTime = dt;
    }

    public JulianDate(decimal julianDate)
    {
      Value = julianDate;
    }

    public JulianDate()
    {
    }


    public static JulianDate FromDate(DateTime dt)
    {
      return new JulianDate(dt);
    }



    public static JulianDate FromDate(int year, int month, int day)
    {
      JulianDate d = new JulianDate();
      d._dateTime = new DateTime(year, month, day);
      return d;
    }

    public static DateTime ToDate(decimal julianDate)
    {
      return new JulianDate(julianDate).Date;
    }



  }
}
