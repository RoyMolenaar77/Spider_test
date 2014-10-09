using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

namespace Concentrator.Objects.Sftp
{
  /// <summary>
  /// Downloades files via WinSCP to a designated location and iterates over the files
  /// </summary>
  public class WinSCPDownloader : IEnumerable<RemoteFile>, IDisposable
  {
    private static XNamespace defaultNS = @"http://winscp.net/schema/session/1.0";

    private string _winscpSessionName;
    private string _logPath;
    private string _winscpApplicationDir;
    private string _downloadPath;
    private string _wildCard;
    private Process winscp;


    /// <summary>
    /// Creates an instance of the winscp downloader
    /// </summary>
    /// <param name="sessionName">the name of the session created for connecting in winscp</param>
    /// <param name="logPath">the path to the log file for winscp</param>
    /// <param name="applicationDir">Path to winscp.exe</param>
    /// <param name="pathToDownload">Path to download remote files to</param>
    /// <param name="fileName">Wildcard for remote files to match. Can be test.*.zip</param>
    public WinSCPDownloader(string sessionName, string logPath, string applicationDir, string pathToDownload, string wildCard)
    {
      _winscpApplicationDir = applicationDir;
      _logPath = logPath;
      _winscpSessionName = sessionName;
      _downloadPath = pathToDownload;
      _wildCard = wildCard;
      InitWinscp();
    }



    private void InitWinscp()
    {
      winscp = new Process();
      winscp.StartInfo.FileName = _winscpApplicationDir;
      winscp.StartInfo.Arguments = _winscpSessionName + " /log=" + _logPath;
      winscp.StartInfo.UseShellExecute = false;
      winscp.StartInfo.RedirectStandardInput = true;
      winscp.StartInfo.RedirectStandardOutput = true;
      winscp.StartInfo.CreateNoWindow = false;
      winscp.StartInfo.RedirectStandardError = true;
      StartConnection();
    }

    private void DownloadFile(string fileName)
    {
      string getCommand = "get " + fileName + " " + Path.Combine(_downloadPath, fileName);
      
      ExecuteCommand("option batch abort");
      ExecuteCommand("option confirm off");
      ExecuteCommand(getCommand);
      winscp.StandardInput.Close();
      winscp.WaitForExit();
    }

    /// <summary>
    /// Moves a remote file to a remote directory
    /// </summary>
    /// <param name="remoteFile"></param>
    /// <param name="remotePath"></param>
    public void MoveRemoteFile(string remoteFile, string remotePath)
    {
      ExecuteCommand("mv " + remoteFile + (remotePath.EndsWith("/") ? remotePath : remotePath + "/"));
    }


    /// <summary>
    /// Renames a remote file
    /// </summary>
    /// <param name="remoteFileName"></param>
    /// <param name="newFileName"></param>
    public void RenameRemoteFile(string remoteFileName, string newFileName)
    {
      if(StartConnection())
      {
        ExecuteCommand("mv " + remoteFileName + " " + newFileName);
      }
    }

    private bool StartConnection()
    {
      return winscp.Start();
    }

    private void CloseConnection()
    {
      winscp.Close();
    }

    public List<string> GetRemoteContents(string wildcard)
    {

      ExecuteCommand("ls " + wildcard);
      winscp.StandardInput.Close();
      winscp.WaitForExit();

      //get file contents
      XDocument doc = XDocument.Load(_logPath);

      //var docRoot = doc.Root;


      List<string> fileCollection = (from x in doc.Element(defaultNS + "session").Element(defaultNS + "ls").Element(defaultNS + "files").Elements(defaultNS + "file")
                                     select x.Element(defaultNS + "filename").Attribute("value").Value).ToList();

      return fileCollection;

    }


    private void ExecuteCommand(string command)
    {
      winscp.StandardInput.WriteLine(command);
    }

    #region IEnumerable<string> Members

    public IEnumerator<RemoteFile> GetEnumerator()
    {
      List<string> remotes = GetRemoteContents("*.zip");
      foreach (string s in remotes)
      {
        InitWinscp();
        DownloadFile(s);
        CloseConnection();
        using (Stream stream = File.Open(Path.Combine(_downloadPath, s), FileMode.Open))
        {
          yield return new RemoteFile(s, stream);
        }
      }
      yield break;
    }

    #endregion

    #region IEnumerable Members

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
      winscp.Close();
      winscp.Dispose();
    }

    #endregion
  }

  public struct RemoteFile : IDisposable
  {
    public readonly string FileName;
    public readonly Stream Data;

    public RemoteFile(string fileName, Stream data)
    {
      FileName = fileName;
      Data = data;
    }

    #region IDisposable Members

    public void Dispose()
    {
      if (Data != null)
        Data.Dispose();

    }

    #endregion
  }
}
