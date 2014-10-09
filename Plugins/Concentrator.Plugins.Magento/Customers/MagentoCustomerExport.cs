using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Web.ServiceClient.JdeCustomerService;
using System.Xml.Linq;
using Concentrator.Objects;
using System.Web;
using System.Security.Cryptography;
using MySql.Data.MySqlClient;
using System.Xml;
using System.IO;
using System.Linq.Expressions;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Plugins.Magento.Customers
{
  public class MagentoCustomerExport : MagentoBasePlugin
  {
    public MagentoCustomerExport() { }

    public override string Name
    {
      get { return "Magento Customer Export Plugin"; }
    }

    private int DetermineLevel(int backendRelationID, int parentBackendRelationID, int level, List<XElement> doc)
    {
      if (parentBackendRelationID == 0)
        return level;
      else
      {

        var nextLevelParent = doc.Where(c => c.Attribute("BackendRelationID").Value == parentBackendRelationID.ToString()).FirstOrDefault();
        if (nextLevelParent == null) return level; //missing parent
        level++;

        return DetermineLevel(int.Parse(nextLevelParent.Attribute("BackendRelationID").Value), int.Parse(nextLevelParent.Attribute("ParentBackendRelationID").Value), level, doc);
      }
    }

    protected override void Process()
    {
      SyncMagentoDatabase();
      var configuration = GetConfiguration();
      //var BusinessUnits = configuration.AppSettings.Settings["BusinessUnits"].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);


      foreach (var connector in base.Connectors.Where(c => ((ConnectorType)c.ConnectorType).Has(ConnectorType.Customers)))
      {
        log.Debug(connector.Connection);


        string BusinessUnits = string.Empty;

        BusinessUnits = connector.ConnectorSettings.GetValueByKey<string>("BusinessUnits", string.Empty);

        if (string.IsNullOrEmpty(BusinessUnits))
          throw new InvalidOperationException("No business units set for this connector");

        var units = BusinessUnits.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);


        foreach (var businessUnit in units)
        {
          JDECustomerSoapClient client = new Web.ServiceClient.JdeCustomerService.JDECustomerSoapClient();

          XDocument customerData = XDocument.Parse(client.GetCustomerData(businessUnit));

          #region Parse data
          var customers = (from customerXml in customerData.Root.Elements("Customer")
                           let allCustomers = customerData.Root.Elements("Customer").ToList()
                           let addressInfo = customerXml.Element("AddressInformation")
                           let telephoneNumbers = customerXml.Element("TelephoneNumbers").Elements("TelephoneNumber")
                           let addresses = customerXml.Element("Addresses").Elements("Address")
                           let accountManager = customerXml.Element("AccountManager")
                           let backendRelationID = customerXml.Attribute("BackendRelationID").Value
                           let parentBackendRelationID = customerXml.Attribute("ParentBackendRelationID").Value
                           let hashed = string.IsNullOrEmpty(customerXml.Element("Password").Value) ? string.Empty : new UTF8Encoding().GetString(MD5.Create().ComputeHash(Encoding.Default.GetBytes(HttpUtility.HtmlDecode(customerXml.Element("Password").Value))))
                           select new
                           {
                             BackendRelationID = backendRelationID,
                             ParentBackendRelationID = parentBackendRelationID,
                             Name = customerXml.Element("Name").Value,
                             Password = HttpUtility.HtmlDecode(customerXml.Element("Password").Value.Trim()),
                             TaxNumber = customerXml.Element("TaxNumber").Value,
                             KvkNr = customerXml.Element("KvkNr").Value,
                             Level = DetermineLevel(int.Parse(backendRelationID), int.Parse(parentBackendRelationID), 0, allCustomers),
                             Contacts = (from cp in customerXml.Element("ContactPersons").Elements("Person")
                                         select new
                                         {
                                           FirstName = cp.Element("FirstName").Value,
                                           LastName = cp.Element("LastName").Value,
                                           Phone = cp.Element("Phone").Value,
                                           Email = cp.Element("Email").Value,
                                           Occupation = cp.Element("Occupation").Value,
                                           ContactType = cp.Element("ContactType").Value
                                         }
                                           ),
                             Children = (from child in customerData.Root.Elements("Customer")
                                         where child.Attribute("ParentBackendRelationID").Value == backendRelationID
                                         select new
                                         {
                                           BackendRelationID = child.Attribute("BackendRelationID")
                                         }),
                             AddressInformation = new
                             {
                               AddressLine1 = addressInfo.Element("AddressLine1").Value,
                               AddressLine2 = addressInfo.Element("AddressLine2").Value,
                               ZipCode = addressInfo.Element("ZipCode").Value,
                               City = addressInfo.Element("City").Value,
                               Country = addressInfo.Element("Country").Value,
                               AddressType = addressInfo.Element("AddressType").Value.Trim()
                             },
                             TelephoneNumbers = (from tp in telephoneNumbers
                                                 select new
                                                 {
                                                   Type = tp.Element("TelephoneType").Value,
                                                   AreaCode = tp.Element("AreaCode").Value,
                                                   Number = tp.Element("Number").Value
                                                 }),
                             EmailAddresses = (from a in addresses
                                               select new
                                               {
                                                 EAddress = a.Element("ElectronicAddress").Value.Trim(),
                                                 EAddressType = a.Element("ElectronicAddressType").Value.Trim()
                                               }),
                             AccountManager = new
                             {
                               Name = accountManager.Element("Name").Value,
                               Email = accountManager.Element("Email").Value.Trim(),
                               PhoneNumber = accountManager.Element("PhoneNumber").Value
                             },
                             DefaultCarrier = customerXml.Element("DefaultCarrier").Value,
                             DefaultCarrierName = customerXml.Element("DefaultCarrierName").Value,
                             Currency = customerXml.Element("Currency").Value,
                             CreditLimit = customerXml.Element("CreditLimit").Value,
                             PaymentDays = customerXml.Element("PaymentDays").Value,
                             PaymentInstrument = customerXml.Element("PaymentInstrument").Value,
                             RouteCode = customerXml.Element("RouteCode").Value,
                             InvoiceAmount = customerXml.Element("InvoiceAmount").Value,
                             OpenInvoiceAmount = customerXml.Element("OpenInvoiceAmount").Value,
                             InvoiceCurrency = customerXml.Element("InvoiceCurrency").Value
                           });
          #endregion

          log.Debug("Parsed customer data");




          string connectionString = connector.ConnectionString;

          using (var container = new magentoContext(MagentoUtility.GetMagentoConnectionString(connectionString)))
          {
            try
            {
              #region Attributes
              var createdInAttribute = container.GetAttribute(1, "created_in", true, true, true);
              var firstnameAttribute = container.GetAttribute(1, "firstname", true, true, true);
              var lastNameAttribute = container.GetAttribute(1, "lastname", true, true, true);
              var passwordAttribute = container.GetAttribute(1, "password_hash", true, true, true);
              var createdAtAttribute = container.GetAttribute(1, "created_at", true, true, true);
              var taxAttribute = container.GetAttribute(1, "taxvat", true, true, true);
              var kvkAttribute = container.GetAttribute(1, "kvknr", true, true, true);
              var accountManagerNameAttribute = container.GetAttribute(1, "accountmanagername", true, true, true);
              var accountManagerEmailAttribute = container.GetAttribute(1, "accountmanageremailaddress", true, true, true);
              var companyNameAttribute = container.GetAttribute(1, "companyname", true, true, true);
              var accountManagerPhoneNumber = container.GetAttribute(1, "accountmanagerphonenumber", true, true, true);
              var invoiceAmountAttribute = container.GetAttribute(1, "invoiceamount", true, true, true);
              var openInvoiceAmountAttribute = container.GetAttribute(1, "openinvoiceamount", true, true, true);
              var paymentInstrumentAttribute = container.GetAttribute(1, "paymentinstrument", true, true, true);
              var paymentDaysAttribute = container.GetAttribute(1, "paymentdays", true, true, true);
              var creditLimitAttribute = container.GetAttribute(1, "creditlimit", true, true, true);
              var routeCodeAttribute = container.GetAttribute(1, "routecode", true, true, true);
              var defaultCarrierAttribute = container.GetAttribute(1, "defaultcarrier", true, true, true);
              var defaultCarrierNameAttribute = container.GetAttribute(1, "defaultcarriername", true, true, true);
              var parentAttribute = container.GetAttribute(1, "parentrelationid", true, true, true);
              var accountNumberAttribute = container.GetAttribute(1, "accountnumber", true, true, true);
              var financialAttribute = container.GetAttribute(1, "financialemail", true, true, true);
              var websiteAttribute = container.GetAttribute(1, "website", true, true, true);
              var extraEmailAttribute = container.GetAttribute(1, "extraemail", true, true, true);


              var telephoneAttribute = container.GetAttribute(2, "telephones", true, true, true);
              var cityAttribute = container.GetAttribute(2, "city", true, true, true);
              var countyAttribute = container.GetAttribute(2, "country_id", true, true, true);
              var companyAttribute = container.GetAttribute(2, "company", true, true, true);
              var postcodeAttribute = container.GetAttribute(2, "postcode", true, true, true);
              var addressLineAttribute = container.GetAttribute(2, "street", true, true, true);
              var billingAddressAttribute = container.GetAttribute(1, "default_billing", true, true, true);
              var shippingAddressAttribute = container.GetAttribute(1, "default_shipping", true, true, true);
              var changePendingAttribute = container.GetAttribute(1, "changepending", true, true, true);
              var firstNameAddressAttribute = container.GetAttribute(2, "firstname", true, true);
              var lastNameAddressAttribute = container.GetAttribute(2, "lastname", true, true);
              var telephoneAddressAttribute = container.GetAttribute(2, "telephone", true, true);
              var emailAttribute = container.GetAttribute(1, "email", true, true, true);
              #endregion

              log.Info("Updated attributes");

              #region Customer data

              #region General customer information
              foreach (var customer in customers)
              {
                try
                {
                  long backEndRelationID = 0;
                  long parentRelationID = 0;
                  long.TryParse(customer.BackendRelationID, out backEndRelationID);
                  long.TryParse(customer.ParentBackendRelationID, out parentRelationID);
                  var magentoCustomer = container.customer_entity.FirstOrDefault(c => c.entity_id == backEndRelationID);

                  if (magentoCustomer == null)
                  {
                    magentoCustomer = new customer_entity()
                    {
                      entity_id = backEndRelationID,
                      entity_type_id = 1,
                      attribute_set_id = 0,
                      website_id = 1,
                      group_id = 1,
                      store_id = 1,
                      created_at = DateTime.Now,
                      updated_at = DateTime.Now,
                      is_active = true,
                      increment_id = ""

                    };

                    container.AddTocustomer_entity(magentoCustomer);
                  }
                  magentoCustomer.email = "undefined@" + customer.BackendRelationID + ".com";

                  #region customer entity attributes

                  SyncCustomerEntityVarchar(container, changePendingAttribute, (x => x.entity_id == backEndRelationID), string.Empty, backEndRelationID);
                  SyncCustomerEntityVarchar(container, createdInAttribute, x => x.entity_id == backEndRelationID, "JDE Import", backEndRelationID);
                  SyncCustomerEntityVarchar(container, firstnameAttribute, x => x.entity_id == backEndRelationID, "-", backEndRelationID);
                  SyncCustomerEntityVarchar(container, lastNameAttribute, x => x.entity_id == backEndRelationID, "-", backEndRelationID);

                  #region Password

                  var hasher = System.Security.Cryptography.MD5.Create();

                  var bytes = hasher.ComputeHash(System.Text.Encoding.Default.GetBytes(customer.Password));

                  StringBuilder sBuilder = new StringBuilder();

                  // Loop through each byte of the hashed data 
                  // and format each one as a hexadecimal string.
                  for (int i = 0; i < bytes.Length; i++)
                  {
                    sBuilder.Append(bytes[i].ToString("x2"));
                  }

                  // Return the hexadecimal string.
                  SyncCustomerEntityVarchar(container, passwordAttribute, (x => x.entity_id == backEndRelationID), string.Format("{0}:", sBuilder.ToString()), backEndRelationID);
                  #endregion

                  SyncCustomerEntityDateTime(container, createdAtAttribute, (x => x.entity_id == backEndRelationID), DateTime.Now, backEndRelationID);

                  SyncCustomerEntityVarchar(container, taxAttribute, (x => x.entity_id == backEndRelationID), string.Format("{0}", customer.TaxNumber), backEndRelationID);

                  SyncCustomerEntityVarchar(container, kvkAttribute, (x => x.entity_id == backEndRelationID), string.Format("{0}", customer.KvkNr), backEndRelationID);

                  SyncCustomerEntityVarchar(container, accountManagerNameAttribute, (x => x.entity_id == backEndRelationID), customer.AccountManager.Name, backEndRelationID);

                  SyncCustomerEntityVarchar(container, accountManagerEmailAttribute, (x => x.entity_id == backEndRelationID), customer.AccountManager.Email, backEndRelationID);

                  SyncCustomerEntityVarchar(container, companyNameAttribute, (x => x.entity_id == backEndRelationID), customer.Name, backEndRelationID);

                  SyncCustomerEntityVarchar(container, accountManagerPhoneNumber, (x => x.entity_id == backEndRelationID), customer.AccountManager.PhoneNumber, backEndRelationID);

                  SyncCustomerEntityDecimal(container, invoiceAmountAttribute, (x => x.entity_id == backEndRelationID), customer.Try(c => decimal.Parse(c.InvoiceAmount), 0), backEndRelationID);

                  SyncCustomerEntityDecimal(container, openInvoiceAmountAttribute, (x => x.entity_id == backEndRelationID), customer.Try(c => decimal.Parse(c.OpenInvoiceAmount), 0), backEndRelationID);

                  SyncCustomerEntityVarchar(container, paymentInstrumentAttribute, (x => x.entity_id == backEndRelationID), customer.PaymentInstrument, backEndRelationID);

                  SyncCustomerEntityVarchar(container, paymentDaysAttribute, (x => x.entity_id == backEndRelationID), customer.PaymentDays, backEndRelationID);

                  SyncCustomerEntityDecimal(container, creditLimitAttribute, (x => x.entity_id == backEndRelationID), customer.Try(c => decimal.Parse(c.CreditLimit), 0), backEndRelationID);

                  SyncCustomerEntityVarchar(container, routeCodeAttribute, (x => x.entity_id == backEndRelationID), customer.RouteCode, backEndRelationID);

                  SyncCustomerEntityVarchar(container, defaultCarrierAttribute, (x => x.entity_id == backEndRelationID), customer.DefaultCarrier, backEndRelationID);

                  SyncCustomerEntityVarchar(container, defaultCarrierNameAttribute, (x => x.entity_id == backEndRelationID), customer.DefaultCarrierName, backEndRelationID);

                  SyncCustomerEntityInt(container, parentAttribute, (x => x.entity_id == backEndRelationID), int.Parse(customer.ParentBackendRelationID), backEndRelationID);

                  SyncCustomerEntityInt(container, accountNumberAttribute, (x => x.entity_id == backEndRelationID), (int)backEndRelationID, backEndRelationID);

                  #region Email addresses

                  var mainEmail = customer.EmailAddresses.FirstOrDefault(c => c.EAddressType == "A");
                  var website = customer.EmailAddresses.FirstOrDefault(c => c.EAddressType == "I");
                  var financialEmail = customer.EmailAddresses.FirstOrDefault(c => c.EAddressType == "F");
                  var extraEmails = customer.EmailAddresses.Where(c => c.EAddressType != "A" && c.EAddressType != "I" && c.EAddressType != "F");

                  SyncCustomerEntityVarchar(container, emailAttribute, (x => x.entity_id == backEndRelationID), mainEmail.Try(c => c.EAddress, "undefined@" + customer.BackendRelationID + ".com"), backEndRelationID);

                  SyncCustomerEntityVarchar(container, financialAttribute, (x => x.entity_id == backEndRelationID), financialEmail.Try(c => c.EAddress, "undefined@" + customer.BackendRelationID + ".com"), backEndRelationID);

                  SyncCustomerEntityVarchar(container, websiteAttribute, (x => x.entity_id == backEndRelationID), website.Try(c => c.EAddress, "undefined@" + customer.BackendRelationID + ".com"), backEndRelationID);

                  string emails = string.Empty;
                  extraEmails.ForEach((email, index) =>
                  {
                    emails += email.EAddress;
                    if (index < extraEmails.Count() - 1) emails += ",";
                  });

                  SyncCustomerEntityVarchar(container, extraEmailAttribute, (x => x.entity_id == backEndRelationID), emails, backEndRelationID);

                  #endregion

                  #endregion

                  foreach (var contactPerson in customer.Contacts)
                  {
                    var contact = container.customercontact.FirstOrDefault(c => c.customer_id == backEndRelationID && c.email == contactPerson.Email);
                    if (contact == null)
                    {
                      contact = new customercontact()
                      {
                        customer_id = backEndRelationID,
                        firstname = contactPerson.FirstName,
                        lastname = contactPerson.LastName,
                        is_changed = 0,
                        is_deleted = 0
                      };
                      container.AddTocustomercontact(contact);
                    }
                    contact.email = contactPerson.Email;
                    contact.contacttype = contactPerson.ContactType;
                    contact.phone = contactPerson.Phone;
                    contact.occupation = contactPerson.Occupation;
                    contact.phonemobile = "";
                  }

                  //var customerAddressEntity = container.customer_address_entity.FirstOrDefault(c => c.parent_id == backEndRelationID);
                  //if (customerAddressEntity == null)
                  //{
                  //  customerAddressEntity = new customer_address_entity()
                  //  {
                  //    entity_type_id = 2,
                  //    attribute_set_id = 0,
                  //    increment_id = "0",
                  //    parent_id = backEndRelationID,
                  //    created_at = DateTime.Now,
                  //    updated_at = DateTime.Now,
                  //    is_active = true
                  //  };
                  //  container.AddTocustomer_address_entity(customerAddressEntity);
                  //}
                  container.SaveChanges();
                }
                catch (Exception e)
                {
                  log.AuditError("Customer export failed for customer " + customer.BackendRelationID, e, "Magento customer export");
                }
              }
              #endregion

              log.Info("Starting customers address import");

              #region Customer address information new

              foreach (var customer in customers.OrderBy(cc => cc.Level))
              {
                long backEndRelationID = 0;
                long parentRelationID = 0;
                long.TryParse(customer.BackendRelationID, out backEndRelationID);
                long.TryParse(customer.ParentBackendRelationID, out parentRelationID);

                var customerEntity = container.customer_entity.FirstOrDefault(c => c.entity_id == backEndRelationID);
                //var customerAddressEntity = container.customer_address_entity.FirstOrDefault(c => c.parent_id == backEndRelationID);

                //if (customerAddressEntity == null) continue;addressEntityIDFromParent

  string commaSepTels = "";
  var telList = customer.TelephoneNumbers
                .Select(c => c.Number).ToList();
  var count = telList.Count;
  telList.ForEach((tel, index) =>
  {
    commaSepTels += tel;
    if (index < count - 1) commaSepTels += " , ";
  });


                string cityAddress = customer.AddressInformation.City;
                string postcodeAddress = customer.AddressInformation.ZipCode;
                string addressLines = string.Format("{0}\n{1}", customer.AddressInformation.AddressLine1, customer.AddressInformation.AddressLine2);

                //shipping address
                var addressEntityID = (int)SyncAddress(container, customer.AddressInformation.Country, countyAttribute, cityAddress, cityAttribute,
                                      commaSepTels, telephoneAttribute, backEndRelationID, customer.Name, companyAttribute,
                                      postcodeAddress, postcodeAttribute, firstNameAddressAttribute, lastNameAddressAttribute,
                                      telephoneAddressAttribute, addressLines, addressLineAttribute);


                #region mapping
                //the inserted values are the shipping address of the customer
                SyncCustomerEntityInt(container, shippingAddressAttribute, (x => x.entity_id == customerEntity.entity_id), (int)addressEntityID, customerEntity.entity_id);

                container.SaveChanges();

                var parentCustomerEntity = container.customer_entity.FirstOrDefault(c => c.entity_id == parentRelationID);

                var hasParent = (parentCustomerEntity != null && parentRelationID != 0);



                //if no parent just map to billing address as well
                if (customer.AddressInformation.AddressType == "X" || !hasParent)
                {
                  SyncCustomerEntityInt(container, billingAddressAttribute, (x => x.entity_id == backEndRelationID), addressEntityID, backEndRelationID);
                  container.SaveChanges();
                }
                else //billing address of parent
                {
                  //var parentAddressEntity = container.customer_address_entity.FirstOrDefault(c => c.parent_id == parentRelationID);

                  if (parentCustomerEntity == null)
                  {
                    log.Debug("Parent entity/Parent address entity is null");
                    continue;
                  }

                  //take the billing address of the parent
                  var billingAdressEntityID = container.customer_entity_int.FirstOrDefault(l => l.attribute_id == billingAddressAttribute.attribute_id && l.entity_id == parentRelationID).value;

                  //var billingAddressEntity = container.customer_address_entity.FirstOrDefault(l => l.entity_id == billingAdressEntityID);
                  var country = container.customer_address_entity_varchar.FirstOrDefault(l => l.entity_id == billingAdressEntityID && l.attribute_id == countyAttribute.attribute_id).value;
                  var city = container.customer_address_entity_varchar.FirstOrDefault(l => l.entity_id == billingAdressEntityID && l.attribute_id == cityAttribute.attribute_id).value;
                  var telephones = container.customer_address_entity_varchar.FirstOrDefault(l => l.entity_id == billingAdressEntityID && l.attribute_id == telephoneAttribute.attribute_id).value;
                  var company = container.customer_address_entity_varchar.FirstOrDefault(l => l.entity_id == billingAdressEntityID && l.attribute_id == companyAttribute.attribute_id).value;
                  var postcode = container.customer_address_entity_varchar.FirstOrDefault(l => l.entity_id == billingAdressEntityID && l.attribute_id == postcodeAttribute.attribute_id).value;
                  var lines = container.customer_address_entity_text.FirstOrDefault(l => l.entity_id == billingAdressEntityID && l.attribute_id == addressLineAttribute.attribute_id).value;

                  var addressEntityIDFromParent = (int)SyncAddress(container, country, countyAttribute, city, cityAttribute, telephones, telephoneAttribute,
                              backEndRelationID, company, companyAttribute, postcode, postcodeAttribute, firstNameAddressAttribute, lastNameAddressAttribute, telephoneAddressAttribute,
                               lines, addressLineAttribute);


                  //map the parent default billing address to the child
                  SyncCustomerEntityInt(container, billingAddressAttribute, (l => l.entity_id == backEndRelationID), addressEntityIDFromParent, backEndRelationID);


                  //push child address to parent without mapping as default shipping
                  SyncAddress(container, customer.AddressInformation.Country, countyAttribute, cityAddress, cityAttribute,
                                      commaSepTels, telephoneAttribute, parentRelationID, customer.Name, companyAttribute,
                                      postcodeAddress, postcodeAttribute, firstNameAddressAttribute, lastNameAddressAttribute,
                                      telephoneAddressAttribute, addressLines, addressLineAttribute);
                  container.SaveChanges();
                }

                #endregion

                container.SaveChanges();
              }
              #endregion

              #endregion
            }
            catch (Exception e) { log.Debug("Customer import failed", e); }
          }
        }
      }
      log.AuditSuccess("Magento customer import finished", "Magento customer export");
    }

    private long SyncAddress(magentoContext container, string country,
                            eav_attribute countryAttribute, string city,
                            eav_attribute cityAttribute, string telephones,
                            eav_attribute telephoneAttribute,
                            long backEndRelationID,
                            string company,
                            eav_attribute companyAttribute, string postcode,
                            eav_attribute postcodeAttribute, eav_attribute firstNameAddressAttribute,
                            eav_attribute lastNameAddressAttribute, eav_attribute telephoneAddressAttribute,
                            string addressLines, eav_attribute addressLinesAttribute,
                            long addressEntityID = 0)
    {

      customer_address_entity customerAddressEntity = null;

      if (addressEntityID == 0)
      {
        customerAddressEntity = container.customer_address_entity.FirstOrDefault(c => c.parent_id == backEndRelationID && c.customer_address_entity_text.Any(al => al.value == addressLines));
        if (customerAddressEntity == null)
        {
          customerAddressEntity = new customer_address_entity()
          {
            entity_type_id = 2,
            attribute_set_id = 0,
            increment_id = "0",
            parent_id = backEndRelationID,
            created_at = DateTime.Now,
            updated_at = DateTime.Now,
            is_active = true
          };
          container.AddTocustomer_address_entity(customerAddressEntity);
          container.SaveChanges();
        }
        addressEntityID = customerAddressEntity.entity_id;
      }

      SyncCustomerAddressEntityVarchar(container, telephoneAttribute, (c => c.entity_id == addressEntityID), addressEntityID, telephones);
      SyncCustomerAddressEntityVarchar(container, countryAttribute, (c => c.entity_id == addressEntityID), addressEntityID, country);
      SyncCustomerAddressEntityVarchar(container, cityAttribute, (c => c.entity_id == addressEntityID), addressEntityID, city);
      SyncCustomerAddressEntityVarchar(container, companyAttribute, (c => c.entity_id == addressEntityID), addressEntityID, company);
      SyncCustomerAddressEntityVarchar(container, postcodeAttribute, (c => c.entity_id == addressEntityID), addressEntityID, postcode);
      SyncCustomerAddressEntityVarchar(container, firstNameAddressAttribute, (c => c.entity_id == addressEntityID), addressEntityID);
      SyncCustomerAddressEntityVarchar(container, lastNameAddressAttribute, (c => c.entity_id == addressEntityID), addressEntityID);
      SyncCustomerAddressEntityVarchar(container, telephoneAddressAttribute, (c => c.entity_id == addressEntityID), addressEntityID);
      SyncCustomerAddressEntityText(container, addressLinesAttribute, (c => c.entity_id == addressEntityID), addressLines, addressEntityID);

      return addressEntityID;
    }

    #region Convenience methods

    private void SyncCustomerAddressEntityVarchar(magentoContext container, eav_attribute attribute, Expression<Func<customer_address_entity_varchar, bool>> predicate, long entityID, string value = "-")
    {
      var entity = container.customer_address_entity_varchar.Where(c => c.attribute_id == attribute.attribute_id).FirstOrDefault(predicate);
      if (entity == null)
      {
        entity = new customer_address_entity_varchar()
        {
          eav_attribute = attribute,
          entity_type_id = 2,
          entity_id = entityID
        };
        container.AddTocustomer_address_entity_varchar(entity);
      }
      entity.value = value;
    }

    private void SyncCustomerAddressEntityText(magentoContext container, eav_attribute attribute, Expression<Func<customer_address_entity_text, bool>> predicate, string value, long entityID)
    {
      var entity = container.customer_address_entity_text.Where(c => c.attribute_id == attribute.attribute_id).FirstOrDefault(predicate);
      if (entity == null)
      {
        entity = new customer_address_entity_text()
        {
          eav_attribute = attribute,
          entity_type_id = 2,
          entity_id = entityID
        };
        container.AddTocustomer_address_entity_text(entity);
      }
      entity.value = value;

    }

    private void SyncCustomerEntityInt(magentoContext container, eav_attribute attribute, Expression<Func<customer_entity_int, bool>> predicate, int value, long entityID, int entityTypeID = 1)
    {
      var entity = container.customer_entity_int.Where(c => c.attribute_id == attribute.attribute_id).FirstOrDefault(predicate);
      if (entity == null)
      {
        entity = new customer_entity_int()
        {
          entity_id = entityID,
          eav_attribute = attribute,
          entity_type_id = entityTypeID
        };
        container.AddTocustomer_entity_int(entity);
      }
      entity.value = value;
    }

    private void SyncCustomerEntityVarchar(magentoContext container, eav_attribute attribute, Expression<Func<customer_entity_varchar, bool>> predicate, string value, long entityID, int entityTypeID = 1)
    {
      var entity = container.customer_entity_varchar.Where(c => c.attribute_id == attribute.attribute_id).FirstOrDefault(predicate);
      if (entity == null)
      {
        entity = new customer_entity_varchar()
        {
          entity_id = entityID,
          eav_attribute = attribute,
          entity_type_id = entityTypeID
        };
        container.AddTocustomer_entity_varchar(entity);
      }
      entity.value = value;
    }

    private void SyncCustomerEntityDateTime(magentoContext container, eav_attribute attribute, Expression<Func<customer_entity_datetime, bool>> predicate, DateTime value, long entityID, int entityTypeID = 1)
    {
      var entity = container.customer_entity_datetime.Where(c => c.attribute_id == attribute.attribute_id).FirstOrDefault(predicate);
      if (entity == null)
      {
        entity = new customer_entity_datetime()
        {
          entity_id = entityID,
          eav_attribute = attribute,
          entity_type_id = entityTypeID
        };
        container.AddTocustomer_entity_datetime(entity);
      }
      entity.value = value;
    }

    private void SyncCustomerEntityDecimal(magentoContext container, eav_attribute attribute, Expression<Func<customer_entity_decimal, bool>> predicate, decimal value, long entityID, int entityTypeID = 1)
    {
      attribute.backend_type = "decimal";

      var entity = container.customer_entity_decimal.Where(c => c.attribute_id == attribute.attribute_id).FirstOrDefault(predicate);
      if (entity == null)
      {
        entity = new customer_entity_decimal()
        {
          entity_id = entityID,
          eav_attribute = attribute,
          entity_type_id = entityTypeID
        };
        container.AddTocustomer_entity_decimal(entity);
      }
      entity.value = value;
    }

    #endregion
  }
}