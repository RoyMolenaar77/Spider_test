using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace System
{
  public static class DateTimeExtensions
  {
    public static DateTime? ToNullOrLocal(this DateTime? dateTime)
    {
      return dateTime.HasValue ? dateTime.Value.ToLocalTime() : (DateTime?)null;
    }

    public static DateTime GetBeginOfDay(this DateTime dateTime)
    {
      return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hour: 0, minute: 0, second: 0, millisecond: 0, kind: dateTime.Kind);
    }

    public static DateTime GetEndOfDay(this DateTime dateTime)
    {
      return dateTime.GetBeginOfDay().AddDays(1).AddMilliseconds(-1);
    }

    /// <summary>
    /// return the next business date of the date specified.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime NextBusinessDay(this DateTime dateTime)
    {
      DateTime result;
      switch (dateTime.DayOfWeek)
      {
        case DayOfWeek.Sunday:
        case DayOfWeek.Monday:
        case DayOfWeek.Tuesday:
        case DayOfWeek.Wednesday:
        case DayOfWeek.Thursday:
          result = dateTime.AddDays(1);
          break;

        case DayOfWeek.Friday:
          result = dateTime.AddDays(3);
          break;

        case DayOfWeek.Saturday:
          result = dateTime.AddDays(2);
          break;

        default:
          throw new ArgumentOutOfRangeException("DayOfWeek=" + dateTime.DayOfWeek);
      }
      return result;
    }
  }
}
