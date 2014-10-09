using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Concentrator.Tasks.Management.Configuration
{
  public class TaskSchedulerElement : ConfigurationElement
  {
    #region Folder Property

    private const String DirectoryProperty = "folder";

    [ConfigurationProperty(DirectoryProperty, IsRequired = true)]
    public String Folder
    {
      get
      {
        return (String)base[DirectoryProperty];
      }
    }

    #endregion
  }
}
