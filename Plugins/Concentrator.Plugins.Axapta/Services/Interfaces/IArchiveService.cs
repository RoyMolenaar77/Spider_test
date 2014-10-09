using System.Collections.Generic;
using System.IO;
using Concentrator.Plugins.Axapta.Enum;
using Concentrator.Plugins.Axapta.Models;

namespace Concentrator.Plugins.Axapta.Services
{
  public interface IArchiveService
  {
    void ExportToArchive<T>(IEnumerable<T> listOfDatCol, SaveTo saveTo, string fileName);
    void CopyToArchive(string ftpUri, SaveTo saveTo, string fileName);

    void ExportToAxapta<T>(IEnumerable<T> listOfDatCol, SaveTo saveTo, string fileName);
    void StreamToAxapta(Stream stream, SaveTo saveTo, string fileName);
  }
}
