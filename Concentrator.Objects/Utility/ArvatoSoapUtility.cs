using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Utility
{
  class ArvatoSoapUtility
  {
    private RED.RetailerSoapClient client;
    private RED.AuthenticationHeader authHeader;

    public ArvatoSoapUtility(string RED_Username, string RED_Password)
    {
      this.client = new Concentrator.Objects.RED.RetailerSoapClient();
      this.authHeader = new Concentrator.Objects.RED.AuthenticationHeader();
      this.authHeader.Username = RED_Username;
      this.authHeader.Password = RED_Password;
    }
    /// <summary>
    /// Lookup Detailed License information - Activation /Install Counts - Adjustment requests made for the order
    /// </summary>
    /// <param name="OrderID"></param>
    /// <param name="OrderLineID"></param>
    /// <param name="Language_Code"></param>
    /// <returns>The xml encoded response containing the detailed order fulfillment information</returns>
    public string PurchaseOrderItemFulfillmentDetailRequest(string OrderID, string OrderLineID, string Language_Code)
    {
      if(client != null)
      {
        return client.PurchaseOrderItemFulfillmentDetailRequest(authHeader, OrderID, OrderLineID, Language_Code);
      }
      return "Failed";
    }
    /// <summary>
    /// Adjust Total unlock limit that the end user is able to complete. Only for Binary type products with support type "Secure package"
    /// </summary>
    /// <param name="OrderID"></param>
    /// <param name="OrderLineID"></param>
    /// <param name="Support_ID"></param>
    /// <param name="Unlock_Limit_Total"></param>
    /// <returns>The number of secure package unlocks an end user is still able to complete.</returns>
    public string SecurePackageToleranceAdjustmentRequest(string OrderID, string OrderLineID, string Support_ID, int Unlock_Limit_Total)
    {
      if(client != null)
      {
        return client.SecurePackageToleranceAdjustmentRequest(authHeader, OrderID, OrderLineID, Support_ID, Unlock_Limit_Total);
      }
      return "Failed";
    }
    /// <summary>
    /// Adjust the total activation tolerance limit to the value specified in the call
    /// </summary>
    /// <param name="OrderID"></param>
    /// <param name="OrderLineID"></param>
    /// <param name="Language_Code"></param>
    /// <param name="Serial_Number"></param>
    /// <param name="Activation_Limit_Total"></param>
    /// <returns>The number of activations an end user is still able to complete.</returns>
    public string SerialNumberToleranceAdjustmentRequest(string OrderID, string OrderLineID, string Language_Code, string Support_ID, string Serial_Number, int Activation_Limit_Total)
    {
      if(client != null)
      {
        client.SerialNumberToleranceAdjustmentRequest(authHeader, OrderID, OrderLineID, Serial_Number, Support_ID,
                                                      Activation_Limit_Total);
      }
      return "Failed";
    }
    /// <summary>
    /// Notify RED of orders that are refunded in your system 
    /// </summary>
    /// <param name="OrderID"></param>
    /// <param name="OrderLineID"></param>
    /// <returns>The amount that the retailers account was adjusted by based on the rules given in this document</returns>
    public string PurchaseOrderItemRefundRequest(string OrderID, string OrderLineID)
    {
      if(client != null)
      {
        client.PurchaseOrderItemRefundRequest(authHeader, OrderID, OrderLineID);
      }
      return "Failed";
    }
    /// <summary>
    /// Notify RED of orders that have been charged back in your system
    /// </summary>
    /// <param name="OrderID"></param>
    /// <param name="Language_Code"></param>
    /// <returns>The xml encoded response containing the chargeback adjustments for each line item in the purchase order.</returns>
    public string PurchaseOrderChargebackRequest(string OrderID, string Language_Code)
    {
      if(client != null)
      {
        return client.PurchaseOrderChargebackRequest(authHeader, OrderID);
      }
      return "Failed";
    }
  }
}

public class ClientException : ApplicationException
{

  // Implement the standard constructors

  public ClientException() : base() { }

  public ClientException(string str) : base(str) { }

  // Override ToString for ClsCustomException.

  public override string ToString()
  {

    return Message;

  }
}
