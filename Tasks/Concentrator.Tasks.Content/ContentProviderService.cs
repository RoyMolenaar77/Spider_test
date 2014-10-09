using System;

namespace Concentrator.Tasks.Content
{
  /// <summary>
  /// Represents a content provider service.
  /// </summary>
  public static class ContentProviderService
  {
    /// <summary>
    /// Returns a <see cref="Concentrator.Tasks.Content.IContentProvider"/> instance that can be use to access content.
    /// The type of content provider that is returned depends on the uri-scheme.
    /// </summary>
    public static IContentProvider GetContentProvider(Uri contentUri)
    {
      if (contentUri == null)
      {
        throw new ArgumentNullException("contentUri");
      }

      if (Uri.UriSchemeFtp.Equals(contentUri.Scheme, StringComparison.OrdinalIgnoreCase))
      {
        return new FileTransferProtocolContentProvider(contentUri);
      }
      else if (Uri.UriSchemeFile.Equals(contentUri.Scheme, StringComparison.OrdinalIgnoreCase) || String.IsNullOrWhiteSpace(contentUri.Scheme))
      {
        return new FileSystemContentProvider(contentUri);
      }
      else
      {
        throw new NotSupportedException(String.Format("The scheme '{0}' is not supported", contentUri.Scheme));
      }
    }
  }
}
