using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Concentrator.Objects.Web.ServiceModels;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Configuration;
using System.Xml;
using System.IO;

namespace Concentrator.Vendors.BAS.Web.Services
{
  /// <summary>
  /// Summary description for JDECustomer
  /// </summary>
  [WebService(Namespace = "http://tempuri.org/")]
  [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
  [System.ComponentModel.ToolboxItem(false)]
  // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
  // [System.Web.Script.Services.ScriptService]
  public class JDECustomer : System.Web.Services.WebService
  {
    [WebMethod(Description = "Retrieves Customer Information from JD Edwards")]
    public string GetCustomerData(string BusinessUnit)
    {
      using (JDECustomerDataContext context = new JDECustomerDataContext(ConfigurationManager.ConnectionStrings["Xtract"].ConnectionString))
      {
        //TODO: optimize lookup in xml generation.
        //Figure out encoding 

        var customerDataSet = context.PortalGetWebCustomers(BusinessUnit);
        var contactPersons = context.PortalFetchContactPersons().ToList();

        var customerData = customerDataSet.GetResult<JdeCustomerData>().GroupBy(c => c.BackendRelationID).Select(g => g.First()).ToList();
        var customerPhoneData = customerDataSet.GetResult<JdeCustomerTelephone>().ToList();
        var customerEmailData = customerDataSet.GetResult<JdeCustomerEmailAddress>().ToList();

        var res = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"),
          new XElement("Customers",
            (from customer in customerData
             select new XElement("Customer",
                new XAttribute("BackendRelationID", customer.BackendRelationID),
                new XAttribute("ParentBackendRelationID", customer.ParentBackendRelationID.Try(c => c.Value, 0)),
                    new XElement("Name", customer.Name.Try(c => c.Trim(), string.Empty)),
                    new XElement("Password", HttpUtility.HtmlEncode(customer.Password.Try(c => c.Trim(), string.Empty))),
                    new XElement("TaxNumber", customer.TaxNumber.Try(c => c.Trim(), string.Empty)),
                    new XElement("KvkNr", customer.KvkNr.Try(c => c.Trim(), string.Empty)),
               new XElement("ContactPersons",
                   (from cp in contactPersons.Where(c => c.EAAN8 == (double?)customer.BackendRelationID)
                    select new XElement("Person",
                     new XElement("FirstName", SanitizeXmlString(cp.WWGNNM.Try(c => c.Trim(), string.Empty))),
                     new XElement("LastName", SanitizeXmlString(cp.WWSRNM.Try(c => c.Trim(), string.Empty))),
                     new XElement("Phone", SanitizeXmlString(cp.WPAR1 + cp.WPPH1)),
                     new XElement("Email", SanitizeXmlString(cp.EAEMAL.Try(c => c.Trim(), string.Empty))),
                     new XElement("Occupation", cp.WWREM1.Try(c => c.Trim(), string.Empty)),
                     new XElement("ContactType", cp.WWATTL.Try(c => c.Trim(), string.Empty))
                      ))
                  ),
               new XElement("AddressInformation",
                   new XElement("AddressLine1", SanitizeXmlString(customer.AddressLine1.Try(c => c.Trim(), string.Empty))),
                   new XElement("AddressLine2", SanitizeXmlString(customer.AddressLine2.Try(c => c.Trim(), string.Empty))),
                   new XElement("ZipCode", SanitizeXmlString(customer.ZipCode.Try(c => c.Trim(), string.Empty))),
                   new XElement("City", SanitizeXmlString(customer.City.Try(c => c.Trim(), string.Empty))),
                   new XElement("Country", SanitizeXmlString(customer.Country.Try(c => c.Trim(), string.Empty))),
                   new XElement("AddressType", SanitizeXmlString(customer.AddressType.Try(c => c.Trim(), string.Empty)))
                 ),

                      new XElement("TelephoneNumbers",
                          (from pn in customerPhoneData.Where(x => x.TelephoneType != "PAS") // exclude password record
                           where pn.BackendRelationID == (double)customer.BackendRelationID.Try(c => c.Value, 0)
                           select new XElement("TelephoneNumber",
                             new XElement("TelephoneType", pn.TelephoneType.Try(c => c.Trim(), string.Empty)),
                             new XElement("AreaCode", pn.AreaCode.Try(c => c.Trim(), string.Empty)),
                             new XElement("Number", pn.PhoneNumber.Try(c => c.Trim(), string.Empty)))
                             )
                        ),

                        new XElement("Addresses",
                            (from address in customerEmailData
                             where address.BackendRelationID == (double)customer.BackendRelationID.Try(c => c.Value, 0)
                             select new XElement("Address",
                               new XElement("ElectronicAddress", SanitizeXmlString(address.Address.Try(c => c.Trim(), string.Empty))),
                               new XElement("ElectronicAddressType", address.ElectronicAddressType.Try(c => c.Trim(), string.Empty))
                               ))
                          ),
                      new XElement("AccountManager",
                        new XElement("Name", SanitizeXmlString(customer.AccountManagerName.Try(c => c.Trim(), string.Empty))),
                        new XElement("Email", SanitizeXmlString(customer.AccountManagerEmailAddress.Try(c => c.Trim(), string.Empty))),
                        new XElement("PhoneNumber", SanitizeXmlString(customer.AccountManagerPhoneNumber.Try(c => c.Trim(), string.Empty)))
                          ),
                      new XElement("DefaultCarrier", customer.DefaultCarrier),
                      new XElement("DefaultCarrierName", customer.DefaultCarrierName.Try(c => c.Trim(), string.Empty)),
                      new XElement("Currency", customer.Currency.Try(c => c.Trim(), string.Empty)),
                      new XElement("CreditLimit", customer.CreditLimit),
                      new XElement("PaymentDays", customer.PaymentDays.Try(c => c.Trim(), string.Empty)),
                      new XElement("PaymentInstrument", customer.PaymentInstrument.Try(c => c.Trim(), string.Empty)),
                      new XElement("RouteCode", customer.RouteCode.Try(c => c.Trim(), string.Empty)),
                      new XElement("InvoiceAmount", customer.InvoiceAmount.Try(c => c.Value.ToString(), "0")),
                      new XElement("OpenInvoiceAmount", customer.OpenInvoiceAmount.Try(c => c.Value.ToString(), "0")),
                      new XElement("InvoiceCurrency", customer.InvoiceCurrency)
               ))));




        return res.ToString();

      }




    }

    /// <summary>
    /// Remove illegal XML characters from a string.
    /// </summary>
    public string SanitizeXmlString(string xml)
    {
      if (xml == null)
      {
        throw new ArgumentNullException("xml");
      }

      StringBuilder buffer = new StringBuilder(xml.Length);

      foreach (char c in xml)
      {
        if (IsLegalXmlChar(c))
        {
          buffer.Append(c);
        }
      }

      return buffer.ToString();
    }

    /// <summary>
    /// Whether a given character is allowed by XML 1.0.
    /// </summary>
    public bool IsLegalXmlChar(int character)
    {
      return
      (
         character == 0x9 /* == '\t' == 9   */          ||
         character == 0xA /* == '\n' == 10  */          ||
         character == 0xD /* == '\r' == 13  */          ||
        (character >= 0x20 && character <= 0xD7FF) ||
        (character >= 0xE000 && character <= 0xFFFD) ||
        (character >= 0x10000 && character <= 0x10FFFF)
      );
    }
  }
}
