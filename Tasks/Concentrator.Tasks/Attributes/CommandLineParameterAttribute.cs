using System;

namespace Concentrator.Tasks
{
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class CommandLineParameterAttribute : Attribute
  {
    /// <summary>
    /// Gets whether the command line switches are case sensative.
    /// Default: false
    /// </summary>
    public Boolean IsCaseSensative
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the command line switch names to look for in the command line.
    /// </summary>
    public String[] Names
    {
      get;
      private set;
    }

    /// <summary>
    /// Initializes the command line switch attribute.
    /// </summary>
    /// <param name="names">
    /// A sequences of the command line switches to look for in the command line.
    /// </param>
    public CommandLineParameterAttribute(params String[] names)
      : this(false, names)
    {
    }

    /// <summary>
    /// Initializes the command line switch attribute.
    /// </summary>
    /// <param name="names">
    /// A sequences of the command line switches to look for in the command line.
    /// </param>
    public CommandLineParameterAttribute(Boolean isCaseSensative, params String[] names)
    {
      IsCaseSensative = isCaseSensative;
      Names = names;
    }
  }
}
