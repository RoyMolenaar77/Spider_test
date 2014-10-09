using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Web.ServiceModels
{
  public class JdeCustomerData
  {
    public decimal? BackendRelationID { get; set; }
    public decimal? ParentBackendRelationID { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
    public string TaxNumber { get; set; }
    public string KvkNr { get; set; }
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string City { get; set; }
    public string ZipCode { get; set; }
    public string Country { get; set; }
    public string AddressType { get; set; }
    public string AccountManagerEmailAddress { get; set; }
    public string AccountManagerName { get; set; }
    public string AccountManagerPhoneNumber { get; set; }
    public decimal? DefaultCarrier { get; set; }
    public string DefaultCarrierName { get; set; }
    public string Currency { get; set; }
    public decimal? CreditLimit { get; set; }
    public string PaymentDays { get; set; }
    public string PaymentInstrument { get; set; }
    public string RouteCode { get; set; }
    public double? InvoiceAmount { get; set; }
    public double? OpenInvoiceAmount { get; set; }
    public string InvoiceCurrency { get; set; }
  }
}
