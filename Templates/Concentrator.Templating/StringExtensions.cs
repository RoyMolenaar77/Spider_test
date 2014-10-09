namespace System
{
  using Text;

  public static class StringExtensions
  {
    public static String ToLowerCamelCase(this String value)
    {
      var builder = new StringBuilder(value);

      builder[0] = Char.ToLower(builder[0]);

      return builder.ToString();
    }

    public static String ToUpperCamelCase(this String value)
    {
      var builder = new StringBuilder(value);

      builder[0] = Char.ToUpper(builder[0]);

      return builder.ToString();
    }
  }
}
