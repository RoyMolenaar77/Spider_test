using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Concentrator.Tasks.Content
{
  /// <summary>
  /// Represents a content provider implementation for accessing the windows file system.
  /// </summary>
  public class FileSystemContentProvider : IContentProvider
  {
    public Uri ContentUri
    {
      get;
      private set;
    }

    public FileSystemContentProvider(String uri)
      : this(new Uri(uri))
    {
    }

    public FileSystemContentProvider(Uri uri)
    {
      ContentUri = uri;

      Initialize();
    }

    public void Dispose()
    {
    }

    private void Initialize()
    {
      if (ContentUri == null)
      {
        throw new InvalidOperationException("ContentUri is null");
      }

      if (!Directory.Exists(ContentUri.LocalPath))
      {
        throw new DirectoryNotFoundException(ContentUri.LocalPath);
      }
    }

    public IEnumerator<IContentItem> GetEnumerator()
    {
      var searchDirectory = ContentUri.LocalPath;
      var searchPattern = ContentUri.Segments.Last();

      if (searchPattern.EndsWith("/"))
      {
        searchDirectory = Path.GetDirectoryName(searchDirectory);
        searchPattern = "*";
      }

      foreach (var fileName in Directory.EnumerateFiles(searchDirectory, searchPattern, SearchOption.AllDirectories))
      {
        yield return new FileSystemContentItem(fileName);
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}
