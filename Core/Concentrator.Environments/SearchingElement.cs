using System;
using System.Configuration;
using System.IO;

namespace Concentrator.Configuration
{
  public sealed class SearchingElement : ConfigurationElement
  {
    private const String DirectoryPropertyName = "directory";

    [ConfigurationProperty(DirectoryPropertyName, IsRequired = true)]
    public String Directory
    {
      get
      {
        return base[DirectoryPropertyName] as String ?? Environment.CurrentDirectory;
      }
    }
  }
}
