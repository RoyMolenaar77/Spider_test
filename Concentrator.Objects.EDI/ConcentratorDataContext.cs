using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.EDI.Order;
using System.Data.Linq;
using Concentrator.Objects.EDI.Response;
using Concentrator.Objects.EDI.Post;
using Concentrator.Objects.EDI.Validation;
using Concentrator.Objects.EDI.FieldMapping;
using Concentrator.Objects.EDI.Vendor;

namespace Concentrator.Objects.EDI
{
  public class ConcentratorDataContext : Concentrator.Objects.ConcentratorDataContext
  {

    public Table<EdiOrder> EdiOrders
    {
      get { return GetTable<EdiOrder>(); }
    }

    public Table<EdiOrderLedger> EdiOrderLedgers
    {
      get { return GetTable<EdiOrderLedger>(); }
    }

    public Table<EdiOrderLine> EdiOrderLines
    {
      get { return GetTable<EdiOrderLine>(); }
    }

    public Table<EdiOrderResponse> EdiOrderResponses
    {
      get { return GetTable<EdiOrderResponse>(); }
    }

    public Table<EdiOrderResponseLine> EdiOrderResponseLines
    {
      get { return GetTable<EdiOrderResponseLine>(); }
    }

    public Table<EdiVendor> EdiVendors
    {
      get { return GetTable<EdiVendor>(); }
    }

    public Table<EdiFieldMapping> EdiFieldMappings
    {
      get { return GetTable<EdiFieldMapping>(); }
    }

    public Table<EdiValidate> EdiValidations
    {
      get { return GetTable<EdiValidate>(); }
    }

    public Table<EdiOrderListener> EdiOrderListeners
    {
      get { return GetTable<EdiOrderListener>(); }
    }

    public Table<EdiOrderPost> EdiOrderPosts
    {
      get { return GetTable<EdiOrderPost>(); }
    }

    public Table<EdiOrderType> EdiOrderTypes
    {
      get { return GetTable<EdiOrderType>(); }
    }

    public Table<EdiCommunication> EdiCommunications
    {
      get { return GetTable<EdiCommunication>(); }
    }


  }
}
