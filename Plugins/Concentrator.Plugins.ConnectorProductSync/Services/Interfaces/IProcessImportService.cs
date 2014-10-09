using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.ConnectorProductSync.Services
{
  public interface IProcessImportService
  {
    void ImportProductGroups();
    void ImportProductGroupMappings();
  }
}
