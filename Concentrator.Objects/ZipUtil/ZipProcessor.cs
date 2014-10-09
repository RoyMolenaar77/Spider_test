using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using Ionic.Zip;

namespace Concentrator.Objects.ZipUtil
{
  public class ZipProcessor : IEnumerable<Concentrator.Objects.ZipUtil.ZipProcessor.ZippedFile>, IDisposable
  {
    private ZipFile _zipFile;

    protected ZipFile zipFile
    {
      get { return _zipFile; }
    }

    public ZipFile File { get { return _zipFile; } }

    public ZipProcessor(Stream zipSource)
    {
      _zipFile = ZipFile.Read(zipSource);
    }

    public ZipProcessor(string pathToZip)
    {
      using (FileStream fs = new FileStream(pathToZip, FileMode.Open, FileAccess.Read, FileShare.None, 256))
      {
        byte[] buffer = new byte[fs.Length];
        fs.Read(buffer, 0, buffer.Length);
        _zipFile = ZipFile.Read(new MemoryStream(buffer));
      }
    }

    private void LoadToMemory()
    {

    }
    #region IEnumerable Members

    #region IEnumerable<ZippedFile> Members

    public IEnumerator<ZippedFile> GetEnumerator()
    {

      foreach (var file in zipFile)
      {
        ZippedFile zippedFile = new ZippedFile() { FileName = file.FileName, Data = new MemoryStream() };
        file.Extract(zippedFile.Data);
        zippedFile.Data.Position = 0; //set position to 0
        yield return zippedFile;
      }

      yield break;
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
      zipFile.Dispose();
    }

    #endregion

    /// <summary>
    /// Saves the entire zip file to the passed stream
    /// </summary>
    /// <param name="s"></param>
    public void Save(Stream s)
    {
      _zipFile.Save(s);
    }

    #region Zipped File
    public struct ZippedFile : IDisposable
    {

      public string FileName;
      public MemoryStream Data;

      #region IDisposable Members

      public void Dispose()
      {
        if (Data != null)
          Data.Dispose();
      }

      #endregion
    }
    #endregion

  }
}
