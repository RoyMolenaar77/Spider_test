using System;
using System.Collections.Generic;

namespace Concentrator.Objects.Archiving
{
  public interface ISimpleArchiveService
  {
    bool ArchiveLines(List<string> lines, string archive, DateTime sessionStart, string fileName);
    bool IsArchived(string archive, string fileName);
  }
}