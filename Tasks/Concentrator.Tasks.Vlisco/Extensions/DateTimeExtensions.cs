using System;

namespace Concentrator.Tasks.Vlisco.Extensions
{
  public static class DateTimeExtensions
  {
    /// <summary>
    /// Takes a DateTime? value and a string containing the format that should be returned if the value is not null.
    /// If the value is null, it will return an empty string.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="format">Example: yyyymmdd </param>
    /// <returns></returns>
    public static String ParseToFormatOrReturnEmptyString(this DateTime? value, String format)
    {
      if (value == null)
      {
        return String.Empty;
      }

      var returnDateTime = (DateTime)value;
      return returnDateTime.ToString(format);
    }
  }
}
