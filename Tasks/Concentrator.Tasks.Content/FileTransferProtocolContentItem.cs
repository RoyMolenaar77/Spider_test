using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Concentrator.Tasks.Content
{
  using Objects.Ftp;

  /// <summary>
  /// Represents a file transfer protocol content item.
  /// </summary>
  public class FileTransferProtocolContentItem : IContentItem
  {
    private Stream contentData = null;

    /// <summary>
    /// Gets the content data as a stream.
    /// </summary>
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
          var ftpClient = new FtpClient(ContentUri);

          contentData = ftpClient.DownloadFile(ContentName);
        }

        return contentData;
      }
    }

    /// <summary>
    /// Gets the name of the content.
    /// </summary>
    public String ContentName
    {
      get;
      private set;
    }

    public Uri ContentUri
    {
      get;
      private set;
    }

    private Boolean IsDisposed
    {
      get;
      set;
    }

    public FileTransferProtocolContentItem(Uri contentUri, String contentName)
    {
      if (contentUri == null)
      {
        throw new ArgumentNullException("contentUri");
      }

      if (String.IsNullOrWhiteSpace(contentName))
      {
        throw new ArgumentNullException("contentName");
      }

      ContentUri = contentUri;
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
