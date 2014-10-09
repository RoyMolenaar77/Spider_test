using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.EDI.Response;
using Concentrator.Objects.Models.EDI.Order;
using Concentrator.Objects.Models.EDI.Vendor;
using System.Data;
using Concentrator.Objects.DataAccess.UnitOfWork;

namespace Concentrator.Objects.EDI
{
  public interface IEdiProcessor
  {
    /// <summary>
    /// Translate received order to Vendor Document
    /// </summary>
    /// <param name="requestDocument"></param>
    /// <returns></returns>
    string DocumentType(string requestDocument);
    
    /// <summary>
    /// Validate order for specified Vendor and server functionality
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    EdiOrderResponse ValidateOrder(EdiOrder order, EdiVendor ediVendor, ConnectorRelation ediRelation, System.Configuration.Configuration config, IUnitOfWork unit);
     
    /// <summary>
    /// Process order for specified Vendor
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    List<EdiOrder> ProcessOrder(string type, string document, int? connectorID, int EdiRequestID, IUnitOfWork unit);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    int ProcessOrderToVendor(EdiOrder order, ConnectorRelation ediRelation, System.Configuration.Configuration config, IUnitOfWork unit);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    int ProcessVendorResponse(EdiOrderType type, System.Configuration.Configuration config, IUnitOfWork unit);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="data"></param>
    void GenerateOrderResponse(EdiOrderResponse orderResponse, IUnitOfWork unit, System.Configuration.Configuration config);

    void GetCustomOrderResponses(IUnitOfWork unit, System.Configuration.Configuration config);

    EdiOrder GetOrderInformation(EdiOrderResponse ediOrderResponse, System.Configuration.Configuration config);

    void GetCustomReponses(IUnitOfWork unit, System.Configuration.Configuration config, string debtor, ConnectorRelation connectorRelation, EdiOrderResponse ediOrderResponse = null, DateTime? invoiceDate = null);
  }
}
