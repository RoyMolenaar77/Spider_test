using Concentrator.Plugins.PFA.Objects.Model;
using Concentrator.Plugins.PFA.Objects.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PFA.Objects.Helper
{
  public class PfaOrderHelper
  {
    private int _vendorID;
    private PfaOrderRespository _orderRepository;

    public PfaOrderHelper(int vendorID)
    {
      _vendorID = vendorID;

      _orderRepository = new PfaOrderRespository(VendorHelper.GetConnectionStringForPFA(vendorID));
    }

    public List<TransferOrderModel> GetShippedQuantitiesForOrder(string orderWebsiteOrderNumber, int connectorIDForOrder)
    {
      var storeNumber = ConnectorHelper.GetStoreNumber(connectorIDForOrder);

      return _orderRepository.GetShippedQuantitiesPerOrder(storeNumber, orderWebsiteOrderNumber);
    }
  }
}
