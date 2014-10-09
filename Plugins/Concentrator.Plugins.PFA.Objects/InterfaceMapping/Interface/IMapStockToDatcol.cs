using Concentrator.Plugins.PFA.Objects.Model;
using System;
using System.Collections.Generic;

namespace Concentrator.Plugins.PFA.Objects.InterfaceMapping.Interface
{
  public interface IMapStockToDatcol
  {
    List<DatColStockModel> MapToDatCol(Int32 vendorId, List<WehkampStockMutation> mutations);
  }
}
