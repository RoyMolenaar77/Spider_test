using System;

namespace System
{
  public class ActionDisposable : IDisposable
  {
    private Action Action
    {
      get;
      set;
    }

    public void Dispose()
    {
      if (Action != null)
      {
        Action.Invoke();
      }
    }

    public ActionDisposable(Action action)
    {
      Action = action;
    }
  }
}
