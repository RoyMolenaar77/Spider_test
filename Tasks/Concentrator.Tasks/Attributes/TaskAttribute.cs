using System;

namespace Concentrator.Tasks
{
  /// <summary>
  /// Specifies the name of the Concentrator-tasks as it should be displayed in the logging and for user-interfacing.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class)]
  public sealed class TaskAttribute : Attribute
  {
    public String Name
    {
      get;
      private set;
    }

    public TaskAttribute(String name)
    {
      Name = name;
    }
  }
}
