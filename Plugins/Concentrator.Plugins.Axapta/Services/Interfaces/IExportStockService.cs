using Concentrator.Plugins.Axapta.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Axapta.Services
{
  public interface IExportStockService
  {
    void ProcessExportStock(List<DatColStock> listOfStocks);
  }
}
