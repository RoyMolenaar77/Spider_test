using System;
using System.IO;

namespace Concentrator.Plugins.PFA.Transfer.Helper
{
  internal static class Files
  {
    internal static void MoveFileToPath(string path, string file, bool addTimeStamp = false)
    {
      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);

      var fileName = Path.GetFileName(file);

      if (fileName != null)
      {
        if (addTimeStamp)
          fileName = string.Format("{0}-{1}", fileName, DateTime.Now.ToString("yyyyMMddHHmmss"));

        if (File.Exists(Path.Combine(path, fileName)))
          File.Delete(Path.Combine(path, fileName));

        File.Move(file, Path.Combine(path, fileName));
      }
    }

    internal static void LogExceptionAndMove(string path, string file, Exception e)
    {
      MoveFileToPath(path, file);

      using (var writer = File.CreateText(Path.Combine(path, Path.GetFileNameWithoutExtension(file) + ".err")))
      {
        writer.WriteLine(e.ToString());
      }
    }
  }
}
