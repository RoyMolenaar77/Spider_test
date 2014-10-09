using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace System
{
  public static class StringExtensions
  {
    public static string ToSafeChars(string s)
    {
      return Regex.Replace(s, "[^a-zA-Z0-9_\\.:,\\s\\&\\-\\\\/\\(\\)\\+=]", "");
    }

    /// <summary>
    /// Returns a copy of this string converted the to titlecase, using the specified culture.
    /// </summary>
    [DebuggerStepThrough]
    public static String ToTitle(this String value, CultureInfo cultureInfo = null)
    {
      value.ThrowIfNull("value");

      return (cultureInfo ?? CultureInfo.CurrentCulture).TextInfo.ToTitleCase(value);
    }

    public static string TailWith(this string s, string tail)
    {
      return s.EndsWith(tail) ? s : s + tail;
    }

    /// <summary>
    /// Returns trimmed string when not null
    /// Returns null when null
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string NullOrTrim(this string s)
    {
      return s != null ? s.Trim() : null;
    }

    /// <summary>
    /// Returns a String array that contains the substrings in this String that are delimited by elements of a specified Unicode character array. A parameter specifies whether to return empty array elements.
    /// </summary>
    [DebuggerStepThrough]
    public static String[] Split(this String instance, String separator, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
    {
      instance.ThrowIfNull("instance");

      return instance.Split(new[] { separator }, options);
    }

    /// <summary>
    /// Substring and trim when null or null
    /// </summary>
    /// <param name="s"></param>
    public static string SubstringNullOrTrim(this string s, int startIndex)
    {
      if (s == null)
      {
        throw new NullReferenceException();
      }

      return s.Substring(startIndex).NullOrTrim();
    }


    /// <summary>
    /// Substring and trim when null or null
    /// </summary>
    /// <param name="s"></param>
    /// <param name="startIndex"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static string SubstringNullOrTrim(this string s, int startIndex, int length)
    {
      var isValid = (!string.IsNullOrEmpty(s) && (startIndex + length) <= s.Length);
      return isValid ? s.Substring(startIndex, length).NullOrTrim() : null;
    }

    public static string SubstringEmptyOrTrim(this string s, int startIndex, int length)
    {
      var isValid = (!string.IsNullOrEmpty(s) && (startIndex + length) <= s.Length);
      return isValid ? s.Substring(startIndex, length).NullOrTrim() : string.Empty;
    }

    public static void SetIfNullOrEmpty(this string s, string value = "n/a")
    {
      if (string.IsNullOrEmpty(s)) s = value;
    }

    public static string IfNullOrEmpty(this string s, string value = "n/a")
    {
      if (string.IsNullOrEmpty(s)) return value;

      return s;
    }

    public static void ThrowIfNullOrEmpty(this string s, Exception e = null)
    {
      if (string.IsNullOrEmpty(s))
      {
        throw e ?? new ArgumentNullException("Argument cannot be empty");
      }
    }

    /// <summary>
    /// Try to parse the value to a <see cref="T:System.Boolean"/>.
    /// </summary>
    public static Boolean? ParseToBool(this String value)
    {
      if (value == null)
      {
        throw new NullReferenceException();
      }

      Boolean result;

      if (Boolean.TryParse(value, out result))
      {
        return result;
      }

      return null;
    }

    /// <summary>
    /// Try to parse the string to a decimal-precision floating-point.
    /// </summary>
    /// <returns>
    /// Returns the decimal if it could parsed or it returns null.
    /// </returns>
    [DebuggerStepThrough]
    public static Decimal? ParseToDecimal(this String value, IFormatProvider formatProvider = null)
    {
      if (value == null)
      {
        throw new NullReferenceException();
      }

      var result = Decimal.Zero;

      return Decimal.TryParse(value, NumberStyles.Number, formatProvider, out result)
        ? result
        : default(Decimal?);
    }

    /// <summary>
    /// Try to parse the string to a date time.
    /// </summary>
    /// <returns>
    /// Returns the date time if it could parsed or it returns null.
    /// </returns>
    [DebuggerStepThrough]
    public static DateTime? ParseToDateTime(this String value, String format)
    {
      return ParseToDateTime(value, DateTimeStyles.None, format);
    }

    /// <summary>
    /// Try to parse the string to a date time.
    /// </summary>
    /// <returns>
    /// Returns the date time if it could parsed or it returns null.
    /// </returns>
    [DebuggerStepThrough]
    public static DateTime? ParseToDateTime(this String value, params String[] formats)
    {
      return ParseToDateTime(value, DateTimeStyles.None, formats);
    }

    /// <summary>
    /// Try to parse the string to a date time.
    /// </summary>
    /// <returns>
    /// Returns the date time if it could parsed or it returns null.
    /// </returns>
    [DebuggerStepThrough]
    public static DateTime? ParseToDateTime(this String value, DateTimeStyles styles, params String[] formats)
    {
      if (value == null)
      {
        throw new NullReferenceException();
      }

      DateTime result;

      if (formats.Length > 0
        ? DateTime.TryParseExact(value, formats, null, styles, out result)
        : DateTime.TryParse(value, out result))
      {
        return result;
      }

      return null;
    }

    /// <summary>
    /// Try to parse the string to a date time.
    /// </summary>
    /// <returns>
    /// Returns the date time if it could parsed or it returns null.
    /// </returns>
    [DebuggerStepThrough]
    public static Double? ParseToDouble(this String value, IFormatProvider formatProvider = null, NumberStyles numberStyles = NumberStyles.Number)
    {
      if (value == null)
      {
        throw new NullReferenceException();
      }

      var result = default(Double);

      if (Double.TryParse(value, numberStyles, formatProvider, out result))
      {
        return result;
      }

      return null;
    }

    public static int? ParseToInt(this string value)
    {
      if (value == null)
      {
        throw new NullReferenceException();
      }

      int result;

      if (Int32.TryParse(value, out result))
      {
        return result;
      }

      return null;
    }

    public static bool IfNullOrEmpty(this string s)
    {
      return string.IsNullOrEmpty(s);
    }

    public static bool IsNullOrWhiteSpace(this string s)
    {
      return string.IsNullOrWhiteSpace(s);
    }

    public static bool IsNullOrEmpty(this string s)
    {
      return string.IsNullOrWhiteSpace(s);
    }

    public static int? ToInt(this string s)
    {
      int result;
      return int.TryParse(s, out result) ? (int?)result : null;
    }

    public static T? ToEnum<T>(this string subject)
           where T : struct
    {
      T parsed;
      return Enum.TryParse<T>(subject, true, out parsed) ? (T?)parsed : null;
    }


    public static void ThrowIfLengthExceeds(this string subject, int max, string parameterName = "value")
    {
      if (subject == null) return;

      if (subject.Length > max)
        throw new ArgumentException(string.Format("Length({0}) of string  exceeded maximum ({1})", subject.Length, max), parameterName);
    }

    public static string ShortenIfLengthExceeds(this string subject, int max)
    {
      return subject.Cap(max);
    }

    /// <summary>
    /// Caps a string
    /// </summary>
    /// <param name="source">Instance</param>
    /// <param name="maxLength">The maximum length allowed for the string</param>
    /// <param name="elipsis">Whether to put an elipsis at the end(...). Elipsis will be included in the max length</param>
    /// <param name="capSymbol">The symbol to cap the text with</param>
    public static string Cap(this string source, int maxLength, bool elipsis = false, string capSymbol = "...")
    {
      if (string.IsNullOrEmpty(source))
      {
        return source;
      }

      if (elipsis)
      {
        return source.Length >= maxLength
          ? source.Substring(0, (maxLength - capSymbol.Length)) + capSymbol
          : source;
      }
      else
      {
        return source.Length >= maxLength
          ? source.Substring(0, maxLength)
          : source;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static List<int> SplitToInts(this string source)
    {
      return source.SplitToInts(new char[] { ',' });
    }

    /// <summary>
    /// Splits a string and projects it to a List<int>
    /// </summary>
    /// <param name="source"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public static List<int> SplitToInts(this string source, params char[] separator)
    {
      return (from s in source.Split(separator, StringSplitOptions.RemoveEmptyEntries)
              select int.Parse(s)).ToList();
    }

    public static String PadLeftFixed(this string s, int length, char paddingChar)
    {
      s = s ?? String.Empty;
      return s.Length < length ? s.PadLeft(length, paddingChar) : s.Truncate(length);
    }

    public static String PadRightFixed(this string s, int length, char paddingChar)
    {
      s = s ?? String.Empty;
      return s.Length < length ? s.PadRight(length, paddingChar) : s.Truncate(length);
    }

    public static String Truncate(this string s, int length)
    {
      if (s.Length > length)
      {
        s = s.Substring(0, length);
      }
      return s;
    }

    public static string RemoveControlCharacters(this string inString)
    {
      if (inString == null) return "";

      StringBuilder newString = new StringBuilder();
      char ch;

      for (int i = 0; i < inString.Length; i++)
      {

        ch = inString[i];

        if (!char.IsControl(ch))
        {
          newString.Append(ch);
        }
      }
      return newString.ToString();
    }

    public static String RemoveNonSpacingMarks(this string s)
    {
      if (s == null) return "";

      s = s.Replace("Æ", "AE");
      s = s.Replace("æ", "ae");
      s = s.Replace("Ö", "OE");
      s = s.Replace("ö", "oe");
      s = s.Replace("Œ", "OE");
      s = s.Replace("œ", "oe");
      s = s.Replace("Ü", "UE");
      s = s.Replace("ü", "ue");
      s = s.Replace("ß", "ss");

      var invalid = new[] { UnicodeCategory.NonSpacingMark, UnicodeCategory.Control };

      return new String((from c in s.Normalize(NormalizationForm.FormD)
                         where !invalid.Contains(CharUnicodeInfo.GetUnicodeCategory(c))
                         select c).ToArray());
    }

    public static string ToUpperFirstLetter(this string source)
    {
      if (string.IsNullOrEmpty(source))
        return string.Empty;
      // convert to char array of the string
      char[] letters = source.ToCharArray();
      // upper case the first char
      letters[0] = char.ToUpper(letters[0]);
      // return the array made of the new char array
      return new string(letters);
    }

    /// <summary>
    /// Returns a new string containing a specified amount of characters. 
    /// </summary>
    [DebuggerStepThrough]
    public static String Wrap(this String instance, Int32 count)
    {
      return instance.Substring(0, Math.Min(instance.Length, count));
    }

  }
}
