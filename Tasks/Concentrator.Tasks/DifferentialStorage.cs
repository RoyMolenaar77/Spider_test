using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using System.Linq;

namespace Concentrator.Tasks
{
  public class DifferentialStorage : IEnumerable<String>
  {
    private FileInfo StorageFileInfo
    {
      get;
      set;
    }

    public DifferentialStorage(String storageFileName)
      : this(new FileInfo(storageFileName))
    {
    }

    public DifferentialStorage(FileInfo storageFileInfo)
    {
      StorageFileInfo = storageFileInfo;
    }
    
    public IEnumerator<String> GetEnumerator()
    {
      StorageFileInfo.Refresh();

      if (StorageFileInfo.Exists)
      { 
        using (var fileStream = StorageFileInfo.OpenRead())
        using (var streamReader = new StreamReader(fileStream))
        {
          for (var line = streamReader.ReadLine(); line != null; line = streamReader.ReadLine())
          {
            yield return line;
          }
        }
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    internal StreamWriter Open()
    {
      StorageFileInfo.Directory.Create();

      return new StreamWriter(StorageFileInfo.Open(FileMode.Create, FileAccess.Write, FileShare.Read));
    }
  }
}
