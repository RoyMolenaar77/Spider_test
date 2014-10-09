using Concentrator.Plugins.PFA.Objects.InterfaceMapping.Classes;
using Concentrator.Plugins.PFA.Objects.Model;
using Concentrator.Plugins.PfaCommunicator.Objects.Models;
using Concentrator.Plugins.PfaCommunicator.Objects.Services;
using FileHelpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace Concentrator.Plugins.PFA.Objects
{
  public class StockMutationGenerator
  {
    public MapStockToDatcol _mapper;
    private String _fileName;

    private FileHelperEngine _stockEngine;

    public StockMutationGenerator()
    {
      _mapper = new MapStockToDatcol();

      _fileName = String.Format("{0}{1}", "vrdmut", DateTime.Now.ToString("yyyyMMddHHmmss"));

      _stockEngine = new FileHelperEngine(typeof(DatColStockModel));
    }

    public void GenerateStockMutations(Int32 vendorId, List<WehkampStockMutation> mutations)
    {
      var mappedList = _mapper.MapToDatCol(vendorId, mutations);

      var flatFile = _stockEngine.WriteString(mappedList);

      var path = CommunicatorService.GetMessagePath(vendorId, MessageTypes.StockMutation);

      File.WriteAllText(Path.Combine(path.MessagePath, _fileName), flatFile);
    }
  }
}
