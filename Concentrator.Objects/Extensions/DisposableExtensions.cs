namespace System
{
  public static class DisposableExtensions
  {
    public static void DisposeIfNotNull(this IDisposable disposable)
    {
      if (disposable != null)
        disposable.Dispose();
    }
  }
}
