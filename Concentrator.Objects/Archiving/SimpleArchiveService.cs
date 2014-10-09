using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.AccessControl;

namespace Concentrator.Objects.Archiving
{
  public class SimpleArchiveService : ISimpleArchiveService
  {
    private const string RootFolderMissing = "The archive root folder \"{0}\" does not exist. Error '{1}'";
    private const string RootFolderUnauthorized = "The archive root folder \"{0}\" is not writeable for the user.";

    private string RootFolder { set; get; }
    private TraceSource TraceSource { set; get; }

    public SimpleArchiveService(string rootFolder, TraceSource traceSource)
    {
      RootFolder = rootFolder;
      TraceSource = traceSource;
    }

    public bool ArchiveLines(List<string> lines, string archive, DateTime sessionStart, string fileName)
    {
      var success = false;
      
      var directoryPath = Path.Combine(
        Path.Combine(RootFolder, archive), 
        sessionStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));


      if (EnsureDirectory(directoryPath))
      {
        var absoluteFilePath = Path.Combine(directoryPath, fileName);
        try
        {
          File.WriteAllLines(absoluteFilePath, lines.ToArray());
          success = true;
        }
        catch { }
      }
      return success;
    }

    public bool IsArchived(string archive, string fileName)
    {
      var absoluteFilePath = Path.Combine(Path.Combine(RootFolder, archive), fileName);
      return File.Exists(absoluteFilePath);
    }

    private bool EnsureDirectory(string accessPath)
    {
      var archivePath = Path.Combine(RootFolder, accessPath);

      if (!Directory.Exists(archivePath))
      {
        try
        {
          Directory.CreateDirectory(archivePath);
        }
        catch (IOException exception)
        {
          TraceSource.TraceError(RootFolderMissing, archivePath, exception.Message);
          return false;
        }
      }

      return true;

      //var rootFolderInfo = new DirectoryInfo(RootFolder);
      //if (!rootFolderInfo.HasAccess(FileSystemRights.FullControl))
      //{
      //  TraceSource.TraceError(RootFolderUnauthorized, RootFolder);
      //  return assured;
      //}

      //assured = true;
      //if (!Directory.Exists(Path.Combine(RootFolder, accessPath)))
      //{
      //  try
      //  {
      //    Directory.CreateDirectory(accessPath);
      //  }
      //  catch { assured = false; }
      //}
      //return assured;
    }
  }
}