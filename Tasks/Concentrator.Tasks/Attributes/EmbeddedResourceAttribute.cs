using System;
using System.IO;

namespace Concentrator.Tasks
{
  /// <summary>
  /// Inserts the content of the assembly specific embedded resource into assembly file or property (as <see cref="System.String"/>).
  /// </summary>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public sealed class ResourceAttribute : Attribute
  {
    public String Extension
    {
      get;
      private set;
    }

    public String Name
    {
      get;
      private set;
    }

    public ResourceAttribute(String name = null, String extension = ".sql")
    {
      Extension = extension.IsNullOrWhiteSpace()
        ? !name.IsNullOrWhiteSpace() 
          ? Path.GetExtension(name) 
          : String.Empty
        : String.Empty;

      Name = !name.IsNullOrWhiteSpace()
        ? Path.GetFileNameWithoutExtension(name)
        : null;
    }
  }
}
