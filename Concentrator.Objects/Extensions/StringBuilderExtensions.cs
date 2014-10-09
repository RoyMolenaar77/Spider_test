namespace System.Text
{
  public static class StringBuilderExtensions
  {
    /// <summary>
    /// Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance and then appends the 
    /// default line terminator to the end of the current <see cref="T:StringBuilder"/> object. Each format item is replaced by the string representation 
    /// of a corresponding argument in a parameter array.
    /// </summary>
    public static StringBuilder AppendFormattedLine(this StringBuilder stringBuilder, String format, params Object[] arguments)
    {
      stringBuilder.AppendFormat(format, arguments);

      return stringBuilder.AppendLine();
    }

    //     
    /// <summary>
    /// Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance and then appends the 
    /// default line terminator to the end of the current <see cref="T:StringBuilder"/> object. Each format item is replaced by the string representation 
    /// of a corresponding argument in a parameter array using a specified format provider.
    /// </summary>
    public static StringBuilder AppendFormattedLine(this StringBuilder stringBuilder, IFormatProvider formatProvider, String format, params Object[] arguments)
    {
      stringBuilder.AppendFormat(formatProvider, format, arguments);
      
      return stringBuilder.AppendLine();
    }
  }
}
