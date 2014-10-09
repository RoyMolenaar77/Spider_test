using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Net;
using System.Xml;
using System.Text.RegularExpressions;

namespace Concentrator.Plugins.EDI.Five4u
{
  public interface IEnvelope { }

  public static class XmlCommunicator
  {
    public static String ExtractData(string data, string key)
    {
      var regex = new Regex("<" + key + ">(.*?)</" + key + ">");
      var match = regex.Match(data);
      return match.Success ? match.Groups[1].Value : null;
    }

    public static List<String> ExtractDataList(string data, string key)
    {
      var regex = new Regex("<" + key + ">(.*?)</" + key + ">", RegexOptions.Multiline | RegexOptions.Singleline);
      var matches = regex.Matches(data);
      var l = new List<string>();
      foreach (Match m in matches)
      {
        if (m.Success)
          l.Add(m.Groups[1].Value);
      }
      return l;
    }

    public static KeyValuePair<string, string> SplitZipcode(string zipcode)
    {
      var regex = new Regex(@"(^[0-9]+)\s*([a-zA-Z]+)");
      var match = regex.Match(zipcode);
      var l = new List<string>();

      if (match.Success)
      {
        var dic = new KeyValuePair<string, string>(match.Groups[1].Value, match.Groups[2].Value);
        return dic;
      }
      return new KeyValuePair<string, string>(zipcode, string.Empty);
    }

    public static WebResponse SendRequest(IEnvelope xml, System.Configuration.Configuration config)
    {

      var requestString = new StringBuilder();

      var settings = new XmlWriterSettings
      {
        Indent = true,
        Encoding = Encoding.UTF8,
        OmitXmlDeclaration = true
      };

      using (var writer = XmlWriter.Create(requestString, settings))
      {

        if (writer == null) throw new Exception("Unable to write to stream.");

        var ns = new XmlSerializerNamespaces();
        ns.Add("urn", "urn:tempuri-org:webservice:Web");
        ns.Add("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");

        var ser = new XmlSerializer(xml.GetType());

        ser.Serialize(writer, xml, ns);

      }

      var requestXml = new XmlDocument();
      requestXml.LoadXml(requestString.ToString());

      var byteData = Encoding.UTF8.GetBytes(requestXml.OuterXml);

      string url = config.AppSettings.Settings["AWWebServiceURL"].Value;
      var req = (HttpWebRequest)WebRequest.Create(url);
      req.Method = "POST";
      req.ContentType = "text/xml";
      req.ContentLength = byteData.Length;
      req.Headers.Add("SOAPAction", "");

      using (var requestStream = req.GetRequestStream())
      {
        requestStream.Write(byteData, 0, byteData.Length);
      }

      WebResponse response;
      try
      {
        response = req.GetResponse();
      }
      catch (WebException ex)
      {
        response = ex.Response;
      }

      return response;

    }

  }

  [Serializable]
  [XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
  public class CreateOrganisationAddressEnvelope : IEnvelope
  {
    [XmlElement]
    public Header Header { get; set; }

    [XmlElement]
    public CreateOrganisationAddressBody Body { get; set; }
  }

  [Serializable]
  [XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
  public class GetPartialDeliveryEnvelope : IEnvelope
  {

    [XmlElement]
    public Header Header { get; set; }

    [XmlElement]
    public GetPartialDeliveryBody Body { get; set; }

  }

  [Serializable]
  [XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
  public class PlaceOrderEnvelope : IEnvelope
  {

    [XmlElement]
    public Header Header { get; set; }

    [XmlElement]
    public PlaceOrderBody Body { get; set; }

  }

  [Serializable]
  [XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
  public class CheckTempOrderEnvelope : IEnvelope
  {

    [XmlElement]
    public Header Header { get; set; }

    [XmlElement]
    public CheckTempOrderBody Body { get; set; }

  }

  [Serializable]
  [XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
  public class GetAddressEnvelope : IEnvelope
  {

    [XmlElement]
    public Header Header { get; set; }

    [XmlElement]
    public GetAddressBody Body { get; set; }

  }

  [Serializable]
  [XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
  public class GetBelongsToOrgAdressEnvelope : IEnvelope
  {

    [XmlElement]
    public Header Header { get; set; }

    [XmlElement]
    public GetBelongsToOrgAdressBody Body { get; set; }

  }

  [Serializable]
  [XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
  public class GetBelongsToOrgsEnvelope : IEnvelope
  {

    [XmlElement]
    public Header Header { get; set; }

    [XmlElement]
    public GetBelongsToOrgsBody Body { get; set; }

  }

  [Serializable]
  [XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
  public class LoginDebtorEnvelope : IEnvelope
  {

    [XmlElement]
    public Header Header { get; set; }

    [XmlElement]
    public LoginDebtorBody Body { get; set; }

  }

  [Serializable]
  [XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
  public class GetPricesAndStocks : IEnvelope
  {
    [XmlElement]
    public Header Header { get; set; }

    [XmlElement]
    public GetPriceAndStocksBody Body { get; set; }

  }

  [Serializable]
  [XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
  public class GetInvoiveLineEnvelope : IEnvelope
  {
    [XmlElement]
    public Header Header { get; set; }

    [XmlElement]
    public GetInvoiceLineBody Body { get; set; }

  }

  [Serializable]
  [XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
  public class GetOutstandingInvoices : IEnvelope
  {
    [XmlElement]
    public Header Header { get; set; }

    [XmlElement]
    public GetOutStandingInvoiceBody Body { get; set; }

  }

  [Serializable]
  [XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
  public class GetOrderLineEnvelope : IEnvelope
  {
    [XmlElement]
    public Header Header { get; set; }

    [XmlElement]
    public GetOrderLineBody Body { get; set; }

  }

  [Serializable]
  public class Header
  {

    [XmlElement(Namespace = "urn:tempuri-org:webservice:Web")]
    public WebID WebID { get; set; }

  }

  [Serializable]
  public class WebID
  {

    [SoapElement]
    // ReSharper disable InconsistentNaming
    public String UUID { get; set; }
    // ReSharper restore InconsistentNaming

  }

  [Serializable]
  public class PlaceOrderBody
  {

    [XmlElement(Namespace = "urn:tempuri-org:webservice:Web")]
    // ReSharper disable InconsistentNaming
    public Placeorder placeorder { get; set; }
    // ReSharper restore InconsistentNaming
  }

  [Serializable]
  public class CheckTempOrderBody
  {

    [XmlElement(Namespace = "urn:tempuri-org:webservice:Web")]
    // ReSharper disable InconsistentNaming
    public CheckTempOrder checktemporder { get; set; }
    // ReSharper restore InconsistentNaming
  }

  [Serializable]
  public class GetPriceAndStocksBody
  {

    [XmlElement(Namespace = "urn:tempuri-org:webservice:Web")]
    // ReSharper disable InconsistentNaming
    public PriceAndStocks getpricesandstocks { get; set; }
    // ReSharper restore InconsistentNaming
  }

  [Serializable]
  public class GetBelongsToOrgAdressBody
  {

    [XmlElement(Namespace = "urn:tempuri-org:webservice:Web")]
    // ReSharper disable InconsistentNaming
    public GetBelongsToOrgsAddress getBelongsToOrgsAddress { get; set; }
    // ReSharper restore InconsistentNaming
  }

  [Serializable]
  public class GetBelongsToOrgsBody
  {

    [XmlElement(Namespace = "urn:tempuri-org:webservice:Web")]
    // ReSharper disable InconsistentNaming
    public GetBelongsToOrgs getBelongsToOrgs { get; set; }
    // ReSharper restore InconsistentNaming
  }

  [Serializable]
  public class GetAddressBody
  {

    [XmlElement(Namespace = "urn:tempuri-org:webservice:Web")]
    // ReSharper disable InconsistentNaming
    public GetAddress getaddress { get; set; }
    // ReSharper restore InconsistentNaming
  }

  [Serializable]
  public class GetPartialDeliveryBody
  {

    [XmlElement(Namespace = "urn:tempuri-org:webservice:Web")]
    // ReSharper disable InconsistentNaming
    public GetPartialDelivery getpartialdelivery { get; set; }
    // ReSharper restore InconsistentNaming

  }

  [Serializable]
  public class LoginDebtorBody
  {

    [XmlElement(Namespace = "urn:tempuri-org:webservice:Web")]
    // ReSharper disable InconsistentNaming
    public LoginDebtor logindebtor { get; set; }
    // ReSharper restore InconsistentNaming

  }

  [Serializable]
  public class CreateOrganisationAddressBody
  {

    [XmlElement(Namespace = "urn:tempuri-org:webservice:Web")]
    // ReSharper disable InconsistentNaming
    public CreateOrganisationAddress createOrganisationAddress { get; set; }
    // ReSharper restore InconsistentNaming

  }


  [Serializable]
  public class GetInvoiceLineBody
  {
    [XmlElement(Namespace = "urn:tempuri-org:webservice:Web")]
    // ReSharper disable InconsistentNaming
    public GetInvoiceLine getinvoiceline { get; set; }
    // ReSharper restore InconsistentNaming

  }

  [Serializable]
  public class GetOutStandingInvoiceBody
  {
    [XmlElement(Namespace = "urn:tempuri-org:webservice:Web")]
    // ReSharper disable InconsistentNaming
    public GetOutstandingInvoice getoutstandinginvoice { get; set; }
    // ReSharper restore InconsistentNaming

  }

  [Serializable]
  public class GetOrderLineBody
  {
    [XmlElement(Namespace = "urn:tempuri-org:webservice:Web")]
    // ReSharper disable InconsistentNaming
    public GetOrderLine getorderline { get; set; }
    // ReSharper restore InconsistentNaming

  }


  [Serializable]
  public class LoginDebtor
  {

    [SoapElement]
    public String DebtorCode { get; set; }

    [SoapElement]
    public String AdminCode { get; set; }

  }

  [Serializable]
  public class GetPartialDelivery
  {

    [SoapElement]
    public String AdminCode { get; set; }

    [SoapElement]
    public String DebtorCode { get; set; }

    [SoapElement]
    public String OrderSort { get; set; }

    [SoapElement]
    public String ParamPartialDeliveryChange { get; set; }

  }

  [Serializable]
  public class Placeorder
  {
    [SoapElement]
    public String LanguageCode { get; set; }

    [SoapElement]
    public String UserCode { get; set; }

    [SoapElement]
    public String DebtorCode { get; set; }

    [SoapElement]
    public String AdminCode { get; set; }

    [SoapElement]
    public String BranchCode { get; set; }

    [SoapElement]
    public String OrderSort { get; set; }

    [SoapElement]
    public String CountPurchase { get; set; }

    [SoapElement]
    public String AccountManager { get; set; }

    [SoapElement]
    public String SplitOrderLines { get; set; }

    [SoapElement]
    public String PriceAlwaysPositive { get; set; }

    [SoapElement]
    public String InstructionSaveInTextLine { get; set; }

    [SoapElement]
    public String ProdCatCode { get; set; }

    [SoapElement]
    public String BlockedProdGrpCodeList { get; set; }

    [SoapElement]
    public String iDeal { get; set; }

    [SoapElement]
    public String OrderType { get; set; }

    [SoapElement]
    public String RemainingQtyInBackOrder { get; set; }

    [SoapElement]
    public String ContactPersonCode { get; set; }

    [SoapElement]
    // ReSharper disable InconsistentNaming
    public ShoppingcardHeaderRow[] ttShoppingcardHeader { get; set; }
    // ReSharper restore InconsistentNaming

    [SoapElement]
    // ReSharper disable InconsistentNaming
    public ShoppingcardLineRow[] ttShoppingcardLine { get; set; }
    // ReSharper restore InconsistentNaming
  }

  [Serializable]
  public class ShoppingcardHeaderRow
  {
    [SoapElement]
    public String Instruction { get; set; }

    [SoapElement]
    public String Reference { get; set; }

    [SoapElement]
    public String InvoiceAddressCode { get; set; }

    [SoapElement]
    public String PartialDelivery { get; set; }

    [SoapElement]
    public String DeliveryCondition { get; set; }

    [SoapElement]
    public String PaymentCondition { get; set; }

    [SoapElement]
    public String OrderCostsManually { get; set; }

    [SoapElement]
    public String OrderCostsAmount { get; set; }

    [SoapElement]
    public String OrderDiscountManually { get; set; }

    [SoapElement]
    public String OrderDiscountAmount { get; set; }
  }

  [Serializable]
  public class TempShoppingcardHeaderRow
  {
    [SoapElement]
    public String Instruction { get; set; }

    [SoapElement]
    public String Reference { get; set; }

    [SoapElement]
    public String InvoiceAddressCode { get; set; }

    [SoapElement]
    public String PartialDelivery { get; set; }

    [SoapElement]
    public String DeliveryCondition { get; set; }

    [SoapElement]
    public String PaymentCondition { get; set; }

    [SoapElement]
    public String OrderCostsManually { get; set; }

    [SoapElement]
    public String OrderCostsAmount { get; set; }

    [SoapElement]
    public String OrderDiscountManually { get; set; }

    [SoapElement]
    public String OrderDiscountAmount { get; set; }

    [SoapElement]
    public String HeaderWarning { get; set; }
  }

  [Serializable]
  public class ShoppingcardLineRow
  {

    [SoapElement]
    public String ItemCode { get; set; }

    [SoapElement]
    public String UnitCode { get; set; }

    [SoapElement]
    public String Ordered { get; set; }

    [SoapElement]
    public String NettoPrice { get; set; }

    [SoapElement]
    public String DiscountPerc { get; set; }

    [SoapElement]
    public String DeliveryDate { get; set; }

    [SoapElement]
    public String DeliveryAddressCode { get; set; }

    [SoapElement]
    public String DeliveryAddressOrg { get; set; }

    [SoapElement]
    public String Instruction { get; set; }

    [SoapElement]
    public String BackOrder { get; set; }

    [SoapElement]
    public String ReturnOrderReason { get; set; }

    [SoapElement]
    public String SalesOrder { get; set; }

    [SoapElement]
    public String InitialDate { get; set; }
  }

  [Serializable]
  public class TempShoppingcardLineRow
  {

    [SoapElement]
    public String ItemCode { get; set; }

    [SoapElement]
    public String UnitCode { get; set; }

    [SoapElement]
    public String Ordered { get; set; }

    [SoapElement]
    public String NettoPrice { get; set; }

    [SoapElement]
    public String DiscountPerc { get; set; }

    [SoapElement]
    public String DeliveryDate { get; set; }

    [SoapElement]
    public String DeliveryAddressCode { get; set; }

    [SoapElement]
    public String DeliveryAddressOrg { get; set; }

    [SoapElement]
    public String Instruction { get; set; }

    [SoapElement]
    public String BackOrder { get; set; }

    [SoapElement]
    public String ReturnOrderReason { get; set; }

    [SoapElement]
    public String SalesOrder { get; set; }

    [SoapElement]
    public String MsgString { get; set; }

    [SoapElement]
    public String PreliminaryReceipts { get; set; }

    [SoapElement]
    public String InitialDate { get; set; }
  }

  [Serializable]
  public class CreateOrganisationAddress
  {

    [SoapElement]
    public String LanguageCode { get; set; }

    [SoapElement]
    public String DebtorCode { get; set; }

    [SoapElement]
    public String AdminCode { get; set; }

    [SoapElement]
    public String OrganisationCode { get; set; }

    [SoapElement]
    public String Organisationname { get; set; }

    [SoapElement]
    public String Streetname { get; set; }

    [SoapElement]
    public String Streetnumber { get; set; }

    [SoapElement]
    public String Postcodenumm { get; set; }

    [SoapElement]
    public String Postcodealpha { get; set; }

    [SoapElement]
    public String City { get; set; }

    [SoapElement]
    public String Extraline { get; set; }

    [SoapElement]
    public String Country { get; set; }
  }

  [Serializable]
  public class CheckTempOrder
  {
    [SoapElement]
    public String LanguageCode { get; set; }

    [SoapElement]
    public String UserCode { get; set; }

    [SoapElement]
    public String DebtorCode { get; set; }

    [SoapElement]
    public String AdminCode { get; set; }

    [SoapElement]
    public String BranchCode { get; set; }

    [SoapElement]
    public String OrderSort { get; set; }

    [SoapElement]
    public String CountPurchase { get; set; }

    [SoapElement]
    public String AccountManager { get; set; }

    [SoapElement]
    public String SplitOrderLines { get; set; }

    [SoapElement]
    public String PriceAlwaysPositive { get; set; }

    [SoapElement]
    public String InstructionSaveInTextLine { get; set; }

    [SoapElement]
    public String ProdCatCode { get; set; }

    [SoapElement]
    public String BlockedProdGrpCodeList { get; set; }

    [SoapElement]
    public String CheckShipmentQty { get; set; }

    [SoapElement]
    public String OrderType { get; set; }

    [SoapElement]
    public String CheckPreliminaryReceipts { get; set; }

    [SoapElement]
    public String RemainingQtyInBackOrder { get; set; }

    [SoapElement]
    // ReSharper disable InconsistentNaming
    public TempShoppingcardHeaderRow[] ttShoppingcardHeader { get; set; }
    // ReSharper restore InconsistentNaming

    [SoapElement]
    // ReSharper disable InconsistentNaming
    public TempShoppingcardLineRow[] ttShoppingcardLine { get; set; }
    // ReSharper restore InconsistentNaming
  }

  [Serializable]
  public class PriceAndStocks
  {
    [SoapElement]
    public String LanguageCode { get; set; }

    [SoapElement]
    public String DebtorCode { get; set; }

    [SoapElement]
    public String AdminCode { get; set; }

    [SoapElement]
    public String ProdCatCode { get; set; }

    [SoapElement]
    public String UsePricePer { get; set; }

    [SoapElement]
    public String InclPrice { get; set; }

    [SoapElement]
    public String InclStock { get; set; }

    [SoapElement]
    public String OrderSort { get; set; }

    [SoapElement]
    public String CountPurchase { get; set; }

    [SoapElement]
    public String OrderQty { get; set; }

    [SoapElement]
    public String BlockedProdGrpCodeList { get; set; }

    [SoapElement]
    public String ItemDescrLong { get; set; }

    [SoapElement]
    public String SearchDebtorItem { get; set; }

    [SoapElement]
    public String ProductMemoType { get; set; }

    [SoapElement]
    public String OrderType { get; set; }

    [SoapElement]
    public String CalculateStockComposedProducts { get; set; }

    [SoapElement]
    public String OneUnitCode { get; set; }

    [SoapElement]
    public String SkipInvalidItems { get; set; }

    [SoapElement]
    // ReSharper disable InconsistentNaming
    public ItemRequestRow[] ttItemRequest { get; set; }
    // ReSharper restore InconsistentNaming
  }

  [Serializable]
  public class ItemRequestRow
  {
    [SoapElement]
    public String ItemCode { get; set; }

    [SoapElement]
    public String UnitCode { get; set; }
  }

  [Serializable]
  public class GetAddress
  {
    [SoapElement]
    public String AddressCode { get; set; }
  }

  [Serializable]
  public class GetBelongsToOrgsAddress
  {
    [SoapElement]
    public String LanguageCode { get; set; }

    [SoapElement]
    public String DebtorCode { get; set; }

    [SoapElement]
    public String AdminCode { get; set; }

    [SoapElement]
    public String OrganisationCode { get; set; }
  }

  [Serializable]
  public class GetBelongsToOrgs
  {
    [SoapElement]
    public String LanguageCode { get; set; }

    [SoapElement]
    public String DebtorCode { get; set; }

    [SoapElement]
    public String AdminCode { get; set; }
  }

  [Serializable]
  public class GetInvoiceLine
  {
    [SoapElement]
    public String DebtorCode { get; set; }

    [SoapElement]
    public String AdminCode { get; set; }

    [SoapElement]
    public String InvoiceNr { get; set; }

    [SoapElement]
    public String OrderNr { get; set; }
  }

  [Serializable]
  public class GetOutstandingInvoice
  {
    [SoapElement]
    public String DebtorCode { get; set; }

    [SoapElement]
    public String AdminCode { get; set; }
  }

  [Serializable]
  public class GetOrderLine
  {
    [SoapElement]
    public String LanguageCode { get; set; }

    [SoapElement]
    public String DebtorCode { get; set; }

    [SoapElement]
    public String AdminCode { get; set; }

    [SoapElement]
    public String OrderNr { get; set; }
  }
}
