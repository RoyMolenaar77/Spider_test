namespace System
{
  public static class BooleanExtensions
  {
    public static bool IsTrue(this bool? value)
    {
      return value.HasValue && value.Value;
    }

    public static bool IsFalse(this bool? value)
    {
      return value.HasValue && !value.Value;
    }
  }
}
