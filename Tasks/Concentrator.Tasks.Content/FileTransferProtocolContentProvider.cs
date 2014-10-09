using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Concentrator.Tasks.Content
{
  using Objects.Ftp;

  public class FileTransferProtocolContentProvider : IContentProvider
  {
    public Uri ContentUri
    {
      get;
      private set;
    }

    public FileTransferProtocolContentProvider(Uri contentUri)
    {
      ContentUri = contentUri;

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
    }

    public IEnumerator<IContentItem> GetEnumerator()
    {
      foreach (var fileName in GetFileNamesRecursively())
      {
        yield return new FileTransferProtocolContentItem(ContentUri, fileName);
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    private IEnumerable<String> GetFileNamesRecursively(String relativePath = null)
    {
      var ftpClient = new FtpClient(ContentUri);

      ftpClient.Update(relativePath);

      foreach (var directoryName in ftpClient.Directories)
      {
        foreach (var fileName in GetFileNamesRecursively(!relativePath.IsNullOrWhiteSpace()
          ? String.Join("/", relativePath, directoryName)
          : directoryName))
        {
          yield return fileName;
        }
      }

      foreach (var fileName in ftpClient.Files)
      {
        yield return fileName;
      }
    }
  }
}
