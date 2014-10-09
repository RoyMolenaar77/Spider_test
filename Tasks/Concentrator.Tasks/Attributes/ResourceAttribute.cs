using System;

namespace Concentrator.Tasks
{
  /// <summary>
  /// Inserts the content of the assembly specific embedded resource into assembly file or property (as <see cref="System.String"/>).
  /// </summary>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public sealed class ResourceAttribute : Attribute
  {
    public String Name
    {
      get;
      private set;
    }

    public ResourceAttribute(String name = null)
    {
      Name = null;
    }
  }
}
