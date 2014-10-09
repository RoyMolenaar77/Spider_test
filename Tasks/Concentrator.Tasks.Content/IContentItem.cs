using System;
using System.IO;

namespace Concentrator.Tasks.Content
{
  /// <summary>
  /// Defines the properties for identifying and accessing a content item.
  /// </summary>
  public interface IContentItem : IDisposable
  {
    Stream ContentData
    {
      get;
    }

    String ContentName
    {
      get;
    }
  }
}
