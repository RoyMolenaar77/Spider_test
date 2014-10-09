using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace Concentrator.Objects.Ftp
{
  public interface IFtpClient
  {
    Uri BaseUri { get; }
    IEnumerable<string> Directories { get; }
    IEnumerable<FtpClientFile> Files { get; }

    NetworkCredential Credential { get; set; }
    TraceSource TraceSource { get; set; }

    void Update(string relativePath = null);

    void CreateDirectory(params string[] directoryNameParts);
    void MoveFile(string remoteFileName, string remoteDirectory);
    void RenameFile(string remoteFileName, string newRemoteFileName);
    void DeleteFile(string remoteFileName);
    bool TryMoveFile(string remoteFileName, string remoteDirectory);
    bool TryMoveFile(string remoteFileName, string remoteDirectory, ref Exception error);

    Stream DownloadFile(string remoteFileName);
    void UploadFile(string remoteFileName, Stream fileStream);
  }
}