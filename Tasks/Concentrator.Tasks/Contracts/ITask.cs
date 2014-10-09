using System;

namespace Concentrator.Tasks.Contracts
{
  /// <summary>
  /// Defines properties and methods that a task must have to be able to execute.
  /// </summary>
  public interface ITask
  {
    /// <summary>
    /// Get the user friendly name of the task.
    /// </summary>
    String Name
    {
      get;
    }

    /// <summary>
    /// Execute the task.
    /// </summary>
    void Execute();
  }
}
