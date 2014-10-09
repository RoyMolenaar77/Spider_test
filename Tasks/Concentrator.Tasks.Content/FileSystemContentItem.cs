using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Concentrator.Tasks.Content
{
  /// <summary>
  /// Represents a file system content item.
  /// </summary>
  public class FileSystemContentItem : IContentItem
  {
    private Stream contentData = null;

    public Stream ContentData
    {
      get
      {
        if (IsDisposed)
        {
          throw new InvalidOperationException("Already disposed");
        }

        if (contentData == null)
        {
          contentData = File.Open(ContentName, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        return contentData;
      }
    }

    public String ContentName
    {
      get;
      private set;
    }

    private Boolean IsDisposed
    {
      get;
      set;
    }

    public FileSystemContentItem(String contentName)
    {
      if (contentName.IsNullOrWhiteSpace())
      {
        throw new ArgumentException("contentName is null or empty");
      }

      if (!File.Exists(contentName))
      {
        throw new FileNotFoundException(contentName);
      }

      ContentName = contentName;
    }

    public void Dispose()
    {
      if (contentData != null)
      {
        contentData.Dispose();
      }

      IsDisposed = true;
    }
  }
}
