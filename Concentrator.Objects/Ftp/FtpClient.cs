using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Concentrator.Objects.Ftp
{
  /// <summary>
  /// Represents a simple FTP-client for performing basic FTP-commands.
  /// </summary>
  public class FtpClient : IFtpClient
  {
    /// <summary>
    /// The default FTP-port.
    /// </summary>
    public const Int32 DefaultPort = 21;

    protected static readonly TraceSource DefaultTraceSource = new TraceSource("Concentrator FTP");

    public Uri BaseUri
    {
      get;
      private set;
    }

    public NetworkCredential Credential
    {
      get;
      set;
    }

    public IEnumerable<String> Directories
    {
      get;
      private set;
    }

    public IEnumerable<FtpClientFile> Files
    {
      get;
      private set;
    }

    private TraceSource traceSource;

    /// <summary>
    /// Gets or sets the trace source of logging.
    /// </summary>
    public TraceSource TraceSource
    {
      get
      {
        return traceSource ?? DefaultTraceSource;
      }
      set
      {
        traceSource = value;
      }
    }

    #region Constructors
    static FtpClient()
    {
      ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
    }

    public FtpClient(String host)
      : this(host, DefaultPort, null)
    {
    }

    public FtpClient(String host, Int32 port)
      : this(host, port, null)
    {
    }

    public FtpClient(String host, Int32 port, NetworkCredential credential)
      : this(new UriBuilder(Uri.UriSchemeFtp, host, port).Uri, credential)
    {
    }

    public FtpClient(String host, Int32 port, String userName, String password)
      : this(host, port, new NetworkCredential(userName, password))
    {
    }

    public FtpClient(Uri baseUri, String userName, String password)
      : this(baseUri, new NetworkCredential(userName, password))
    {
    }

    public FtpClient(Uri baseUri)
      : this(baseUri, null)
    {
    }

    public FtpClient(Uri baseUri, NetworkCredential credential)
    {
      var uriBuilder = new UriBuilder(baseUri);

      BaseUri = baseUri;
      Credential = credential ?? new NetworkCredential
      {
        Domain = String.Empty,
        Password = HttpUtility.UrlDecode(uriBuilder.Password),
        UserName = HttpUtility.UrlDecode(uriBuilder.UserName)
      };
      Directories = Enumerable.Empty<String>();
      Files = Enumerable.Empty<FtpClientFile>();
    }
    #endregion

    private FtpWebRequest CreateRequest(String method)
    {
      return CreateRequest(method, BaseUri);
    }

    private FtpWebRequest CreateRequest(Uri requestUri, String method)
    {
      var request = (FtpWebRequest)WebRequest.Create(requestUri);

      request.Credentials = Credential;
      request.EnableSsl = false;
      request.KeepAlive = false;
      request.Method = method;
      request.UseBinary = true;
      request.UsePassive = true;

      return request;
    }

    private FtpWebRequest CreateRequest(String method, String relativeUri)
    {
      var uriBuilder = new UriBuilder(BaseUri);

      if (!relativeUri.IsNullOrEmpty())
      {
        uriBuilder.Path = Path.Combine(uriBuilder.Path, relativeUri).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
      }

      return CreateRequest(uriBuilder.Uri, method);
    }

    private FtpWebRequest CreateRequest(String method, Uri relativeUri)
    {
      var requestUri = relativeUri != null
        ? new Uri(BaseUri, relativeUri)
        : BaseUri;

      return CreateRequest(requestUri, method);
    }

    public void CreateDirectory(params String[] directoryNameParts)
    {
      var directoryName = String.Join("/", directoryNameParts.Select(directoryNamePart => directoryNamePart.Trim(' ', '/')));

      using (CreateRequest(WebRequestMethods.Ftp.MakeDirectory, directoryName).GetResponse())
      {
        TraceSource.TraceEvent(TraceEventType.Information, 0, "");
      }
    }

    public void DeleteFile(String remoteFileName)
    {
      using (CreateRequest(WebRequestMethods.Ftp.DeleteFile, remoteFileName).GetResponse())
      {
      }
    }

    /// <summary>
    /// Download the specified remote file to a stream.
    /// </summary>
    public Stream DownloadFile(String remoteFileName)
    {
      var request = CreateRequest(WebRequestMethods.Ftp.DownloadFile, remoteFileName);

      using (var response = request.GetResponse())
      using (var responseStream = response.GetResponseStream())
      {
        var returnStream = new MemoryStream();

        responseStream.CopyTo(returnStream);

        return returnStream.Reset();
      }
    }

    public void MoveFile(String remoteFileName, String remoteDirectory)
    {
      RenameFile(remoteFileName, remoteDirectory + Path.AltDirectorySeparatorChar + remoteFileName);
    }

    public Boolean TryMoveFile(String remoteFileName, String remoteDirectory)
    {
      var error = default(Exception);

      return TryMoveFile(remoteFileName, remoteDirectory, ref error);
    }

    public Boolean TryMoveFile(String remoteFileName, String remoteDirectory, ref Exception error)
    {
      try
      {
        MoveFile(remoteFileName, remoteDirectory);

        return true;
      }
      catch (Exception exception)
      {
        error = exception;

        return false;
      }
    }

    public void RenameFile(String remoteFileName, String newRemoteFileName)
    {
      var request = CreateRequest(WebRequestMethods.Ftp.Rename, remoteFileName);

      request.RenameTo = newRemoteFileName;

      using (var response = request.GetResponse() as FtpWebResponse)
      {
        TraceSource.TraceEvent(TraceEventType.Verbose, 0, "{0} {1} {2}", request.RequestUri, request.Method, request.RenameTo);
      }
    }

    private static readonly Regex ListDirectoryDetailsRegex = new Regex(@"^
      (?<DirectoryFlag>d|-)
      (?<OwnerPermissions>[rwx-]{3})
      (?<GroupPermissions>[rwx-]{3})
      (?<OtherPermissions>[rwx-]{3})
      \s+
      (?<FileCode>\S+)
      \s+
      (?<OwnerID>\S+)
      \s+
      (?<GroupID>\S+)
      \s+
      (?<FileSize>\d+)
      \s+
      (?<DateTime>\w+\s+\d+\s+(\d{2}:\d{2})|\d{4})
      \s+
      (?<Name>[^$]*)"
      , RegexOptions.Compiled 
      | RegexOptions.IgnoreCase 
      | RegexOptions.IgnorePatternWhitespace);

    private static readonly CultureInfo LinuxCultureInfo = new CultureInfo("en");

    private static readonly String[] LinuxDateTimeFormats = new String[] 
    {
      "MMM dd HH':'mm", 
      "MMM dd yyyy"
    };

    /// <summary>
    /// Updates the Directories- and Files properties.
    /// </summary>
    public void Update(String relativePath = null)
    {
      var request = CreateRequest(WebRequestMethods.Ftp.ListDirectoryDetails, relativePath);

      TraceSource.TraceEvent(TraceEventType.Verbose, 0, "{0} {1}"
        , request.RequestUri
        , request.Method);

      using (var response = (FtpWebResponse)request.GetResponse())
      using (var reader = new StreamReader(response.GetResponseStream()))
      {
        var lines = reader.ReadToEnd().Split(Environment.NewLine);

        var directories = new SortedList<String, DateTime>();
        var files = new SortedList<String, DateTime>();
        
        foreach (var line in lines)
        {
          TraceSource.TraceEvent(TraceEventType.Verbose, 0, line);

          var match = ListDirectoryDetailsRegex.Match(line);

          if (match.Success)
          {
            var fileOrDirectoryTime = default(DateTime);

            if (DateTime.TryParseExact(match.Groups["DateTime"].Value
              , LinuxDateTimeFormats
              , LinuxCultureInfo
              , DateTimeStyles.None
              , out fileOrDirectoryTime))
            {
              var fileOrDirectoryName = match.Groups["Name"].Value;

              switch (match.Groups["DirectoryFlag"].Value)
              {
                case "d":
                  directories.Add(fileOrDirectoryName, fileOrDirectoryTime);
                  break;

                case "-":
                  files.Add(fileOrDirectoryName, fileOrDirectoryTime);
                  break;

                default:
                  throw new InvalidDataException();
              }
            }
            else
            {
              TraceSource.TraceEvent(TraceEventType.Warning, 0, "'{0}' is not a valid date-time format.", match.Groups["DateTime"].Value);
            }
          }
          else
          {
            TraceSource.TraceEvent(TraceEventType.Warning, 0, "'{0}' is not a valid LIST result.", line);
          }
        }

        Directories = directories
          .OrderBy(directory => directory.Value)
          .Select(directory => directory.Key)
          .ToArray();

        Files = files
          .OrderBy(file => file.Value)
          .Select(file => new FtpClientFile
            {
              FileName = file.Key,
              CreationTime = file.Value
            })
          .ToArray();
      }
    }

    /// <summary>
    /// Download the specified remote file to a stream.
    /// </summary>
    public void UploadFile(String remoteFileName, Stream fileStream)
    {
      var request = CreateRequest(WebRequestMethods.Ftp.UploadFile, remoteFileName);

      request.ContentLength = fileStream.Length;

      using (var requestStream = request.GetRequestStream())
      {
        fileStream.CopyTo(requestStream);
      }

      using (var response = request.GetResponse() as FtpWebResponse)
      {
        TraceSource.TraceEvent(TraceEventType.Information, 0, "Upload completed, status: {0} - '{1}'", response.StatusCode, response.StatusDescription);
      }
    }
  }

  public class FtpClientFile
  {
    public String FileName { get; set; }
    public DateTime CreationTime { get; set; }
  }
}
