using System;
using System.Collections.Generic;

namespace Concentrator.Tasks.Content
{
  /// <summary>
  /// Defines the property and methods for accessing zero or more content items.
  /// </summary>
  public interface IContentProvider : IDisposable, IEnumerable<IContentItem>
  {
    Uri ContentUri
    {
      get;
    }
  }
}
